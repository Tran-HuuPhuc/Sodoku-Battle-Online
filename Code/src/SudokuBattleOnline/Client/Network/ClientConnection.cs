x1 using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SudokuBattleOnline.Shared.Packets;

namespace Client.Network
{
    /// <summary>
    /// quản lý kết nối tcp client
    /// cái này để test commit - tên - email
    /// </summary>
    public class ClientConnection
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;

        public event Action<string> OnMessageReceived;
        public event Action OnDisconnected;

        public async Task ConnectAsync(string ip, int port)
        {
            int maxThuKetNoi = 5;
            int ketNoi = 0;

            while (_tcpClient == null || !_tcpClient.Connected)
            {
                try
                {
                    ketNoi++;
                    Console.WriteLine($"Đang kết nối đến Server... (Thử lần {ketNoi}/{maxThuKetNoi})");

                    _tcpClient = new TcpClient();
                    //Thử kết nối bất đồng bộ sang Server
                    await _tcpClient.ConnectAsync(ip, port);
                    _stream = _tcpClient.GetStream();

                    // start loop chờ nhận tin nhắn
                    _ = ReceiveDataAsync();
                    break; // Kết nối thành công, thoát vòng lặp
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi kết nối: {ex.Message}");
                    _tcpClient?.Close();
                    _tcpClient = null;

                    await Task.Delay(3000); // Đợi 2 giây trước khi thử lại
                }
            }
        }

        public void Disconnect()
        {
            _stream?.Close();
            _tcpClient?.Close();
            OnDisconnected?.Invoke();
        }

        /// <summary>
        /// Hàm gửi gói tin đối tượng dạng JSON (Bắt buộc cộng \n ở cuối để chống dính gói mạng)
        /// </summary>
        public async Task SendPacketAsync(BasePacket packet)
        {
            if (_stream != null && _stream.CanWrite)
            {
                try
                {
                    // 1. Chuyển đối tượng Packet sang chuỗi văn bản JSON
                    string jsonString = JsonSerializer.Serialize(packet);

                    // 2. Thêm ký tự xuống dòng \n ở cuối để phân tách gói
                    jsonString += "\n";

                    // 3. Mã hóa chuỗi JSON sang mảng byte UTF-8 và bắn qua mạng
                    byte[] data = Encoding.UTF8.GetBytes(jsonString);
                    await _stream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LỖI SENDER] Gửi gói tin JSON thất bại: {ex.Message}");
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Hàm gửi chuỗi string văn bản thô
        /// </summary>
        public async Task SendRawMessageAsync(string message)
        {
            if (_stream != null && _stream.CanWrite)
            {
                try
                {
                    // Đảm bảo tin nhắn thô cũng có \n để đầu Server đọc được bằng ReadLineAsync
                    if (!message.EndsWith("\n"))
                    {
                        message += "\n";
                    }

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    await _stream.WriteAsync(data, 0, data.Length);
                    System.Diagnostics.Debug.WriteLine($"[CLIENT GỬI RAW]: {message.Trim()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi gửi tin nhắn thô: {ex.Message}");
                    Disconnect();
                }
            }
        }

        private async Task ReceiveDataAsync()
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (_tcpClient != null && _tcpClient.Connected)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        OnMessageReceived?.Invoke(message);
                    }
                    else
                    {
                        Disconnect();
                        break;
                    }
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }
}
