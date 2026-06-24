using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SudokuBattleOnline.Shared.Packets;

namespace SudokuBattle.Server.Network
{
    /// <summary>
    /// Đại diện cho một phiên kết nối của client (người chơi) tới Server.
    /// Mỗi ClientSession bọc một TcpClient, quản lý việc đọc/gửi dữ liệu
    /// và lưu trữ thông tin phiên làm việc (username, trạng thái đăng nhập...).
    /// </summary>
    public class ClientSession
    {
        // ─── Thông tin kết nối ───
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        // ─── Chống Spam (Rate Limiting) ───
        private int _packetCount;
        private DateTime _lastResetTime;
        private const int MAX_PACKETS_PER_SECOND = 20;
        private const int MAX_PACKET_LENGTH = 100 * 1024; 

        // ─── Thông tin phiên ───

        /// <summary>
        /// Mã định danh duy nhất cho phiên kết nối này.
        /// </summary>
        public string SessionId { get; }

        /// <summary>
        /// Tên đăng nhập của người chơi (null nếu chưa đăng nhập).
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Trạng thái đã xác thực (đăng nhập thành công) hay chưa.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Mã phòng hiện tại mà người chơi đang ở (null nếu chưa vào phòng nào).
        /// </summary>
        public string? CurrentRoomId { get; set; }

        /// <summary>
        /// Thời điểm kết nối được thiết lập.
        /// </summary>
        public DateTime ConnectedAt { get; }

        /// <summary>
        /// Kiểm tra kết nối TCP có còn sống hay không.
        /// </summary>
        public bool IsConnected => _tcpClient.Connected;

        // ─── Sự kiện ───

        /// <summary>
        /// Sự kiện được kích hoạt khi nhận được một dòng JSON hoàn chỉnh từ client.
        /// Tham số: (ClientSession session, string jsonLine)
        /// </summary>
        public event Action<ClientSession, string>? OnDataReceived;

        /// <summary>
        /// Sự kiện được kích hoạt khi client ngắt kết nối.
        /// Tham số: (ClientSession session)
        /// </summary>
        public event Action<ClientSession>? OnDisconnected;

        // ─── Constructor ───

        public ClientSession(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            
            _tcpClient.ReceiveTimeout = 30000; // 30s
            _tcpClient.SendTimeout = 15000;    // 15s

            _stream = tcpClient.GetStream();

            // Khởi tạo thời gian Rate Limiting
            _lastResetTime = DateTime.UtcNow;

            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8)
            {
                AutoFlush = true // Tự động flush sau mỗi lần ghi
            };

            SessionId = Guid.NewGuid().ToString("N")[..8]; // Mã ngắn gọn 8 ký tự
            ConnectedAt = DateTime.Now;
        }

        // ─── Vòng lặp nhận dữ liệu ───

        /// <summary>
        /// Bắt đầu vòng lặp bất đồng bộ liên tục đọc dữ liệu từ client.
        /// Mỗi dòng JSON nhận được sẽ kích hoạt sự kiện OnDataReceived.
        /// Khi client ngắt kết nối, sự kiện OnDisconnected được kích hoạt.
        /// </summary>
        public async Task StartReceivingAsync()
        {
            try
            {
                string? jsonLine;
                while ((jsonLine = await _reader.ReadLineAsync()) != null)
                {
                    // Bỏ qua các dòng trống
                    if (string.IsNullOrWhiteSpace(jsonLine))
                        continue;

                    // tránh tràn RAM
                    if (jsonLine.Length > MAX_PACKET_LENGTH)
                    {
                        Console.WriteLine($"[BẢO VỆ] {this} gửi gói tin quá lớn ({jsonLine.Length} bytes). Đã ngắt kết nối.");
                        Disconnect();
                        break;
                    }

                    // chống spam
                    var now = DateTime.UtcNow;
                    if ((now - _lastResetTime).TotalSeconds >= 1)
                    {
                        _packetCount = 0;
                        _lastResetTime = now;
                    }
                    
                    _packetCount++;
                    if (_packetCount > MAX_PACKETS_PER_SECOND)
                    {
                        Console.WriteLine($"[BẢO VỆ] {this} spam quá {MAX_PACKETS_PER_SECOND} gói/s. Đã ngắt kết nối.");
                        Disconnect();
                        break;
                    }

                    // Kích hoạt sự kiện để PacketRouter xử lý
                    OnDataReceived?.Invoke(this, jsonLine);
                }
            }
            catch (IOException)
            {
                // Client ngắt kết nối đột ngột (mất mạng, đóng app...)
            }
            catch (ObjectDisposedException)
            {
                // Stream đã bị đóng từ phía server (kick, shutdown...)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI SESSION {SessionId}] Lỗi không mong đợi: {ex.Message}");
            }
            finally
            {
                // Luôn kích hoạt sự kiện ngắt kết nối để dọn dẹp
                OnDisconnected?.Invoke(this);
            }
        }

        // ─── Gửi dữ liệu ───

        /// <summary>
        /// Gửi một đối tượng gói tin (kế thừa BasePacket) sang client dưới dạng JSON.
        /// Thread-safe nhờ SemaphoreSlim, có thể gọi từ nhiều luồng đồng thời.
        /// </summary>
        public async Task SendPacketAsync(BasePacket packet)
        {
            await _writeLock.WaitAsync();
            try
            {
                if (_tcpClient.Connected)
                {
                    // Serialize đối tượng sang JSON, lấy kiểu thực tế (derived type)
                    string json = JsonSerializer.Serialize(packet, packet.GetType());
                    await _writer.WriteLineAsync(json); // WriteLine tự thêm \n
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI GỬI {SessionId}] Không thể gửi gói tin: {ex.Message}");
            }
            finally
            {
                _writeLock.Release();
            }
        }

        /// <summary>
        /// Gửi chuỗi JSON thô (đã sẵn sàng) sang client.
        /// Dùng khi cần gửi lại dữ liệu JSON mà không cần serialize lại.
        /// </summary>
        public async Task SendRawJsonAsync(string json)
        {
            await _writeLock.WaitAsync();
            try
            {
                if (_tcpClient.Connected)
                {
                    await _writer.WriteLineAsync(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI GỬI RAW {SessionId}] {ex.Message}");
            }
            finally
            {
                _writeLock.Release();
            }
        }

        // ─── Ngắt kết nối ───

        /// <summary>
        /// Đóng toàn bộ tài nguyên kết nối của phiên này.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _reader.Dispose();
                _writer.Dispose();
                _stream.Dispose();
                _tcpClient.Close();
            }
            catch (Exception)
            {
                // Bỏ qua lỗi khi đóng kết nối đã bị hỏng
            }
        }

        public override string ToString()
        {
            string name = Username ?? "(chưa đăng nhập)";
            return $"[Session {SessionId} | {name}]";
        }
    }
}
