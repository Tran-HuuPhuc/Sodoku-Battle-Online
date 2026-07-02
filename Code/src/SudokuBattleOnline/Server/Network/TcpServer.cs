using SudokuBattle.Server.Rooms;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SudokuBattle.Server.Network
{
    /// <summary>
    /// Máy chủ TCP Socket chính của hệ thống Sudoku Battle Online.
    /// 
    /// Kiến trúc xử lý gói tin:
    ///   Client gửi JSON ──► TcpServer (lắng nghe)
    ///                            │
    ///                            ▼
    ///                       ClientSession (đọc theo dòng, chống dính gói)
    ///                            │
    ///                            ▼
    ///                       PacketRouter (phân tích PacketType, định tuyến)
    ///                            │
    ///                            ▼
    ///                       PacketHandler (xử lý logic nghiệp vụ)
    ///                            │
    ///                            ▼
    ///                       ClientSession.SendPacketAsync() (phản hồi về Client)
    /// </summary>
    public class TcpServer
    {
        private TcpListener? _listener;
        private bool _isRunning;
        private readonly CancellationTokenSource _cts = new();

        // ─── Các module quản lý ───
        private readonly SessionManager _sessionManager;
        private readonly PacketRouter _packetRouter;
        private readonly PacketHandler _packetHandler;
        private readonly SudokuBattle.Server.Matchmaking.MatchmakingQueue _matchmakingQueue;
        private readonly SudokuBattle.Server.Matchmaking.MatchmakingManager _matchmakingManager;

        // ─── Cấu hình ───
        private readonly int _port;
        private readonly int _maxConnections;
        private readonly int _maxConnectionsPerIP;

        // ─── Đếm IP ───
        private readonly ConcurrentDictionary<string, int> _ipConnectionCounts = new();

        /// <summary>
        /// Khởi tạo TcpServer với cổng chỉ định (mặc định 8888).
        /// Tự động khởi tạo toàn bộ hệ thống quản lý phiên, định tuyến và xử lý gói tin.
        /// </summary>
        public TcpServer(int port = 8888, int maxConnections = 500, int maxConnectionsPerIP = 3)
        {
            _port = port;
            _maxConnections = maxConnections;
            _maxConnectionsPerIP = maxConnectionsPerIP;

            // Khởi tạo các module theo đúng thứ tự phụ thuộc
            _sessionManager = new SessionManager();
            _matchmakingQueue = new SudokuBattle.Server.Matchmaking.MatchmakingQueue();
            
            var roomManager = new RoomManager();
            _packetHandler = new PacketHandler(_sessionManager, _matchmakingQueue, roomManager);
            _matchmakingManager = new SudokuBattle.Server.Matchmaking.MatchmakingManager(_matchmakingQueue, roomManager, _packetHandler);
            _packetRouter = new PacketRouter(_packetHandler);
        }

        // ─── Điểm truy cập các module (cho các service bên ngoài) ───

        /// <summary>
        /// Truy cập bộ quản lý phiên kết nối.
        /// </summary>
        public SessionManager Sessions => _sessionManager;

        // ═══════════════════════════════════════════════
        //  KHỞI ĐỘNG SERVER
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Khởi động TCP Server, bắt đầu lắng nghe kết nối tại cổng được chỉ định.
        /// Phương thức này sẽ chạy liên tục (blocking) cho đến khi Stop() được gọi.
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _isRunning = true;
                
                _matchmakingManager.Start();

                Console.WriteLine("╔══════════════════════════════════════════════════╗");
                Console.WriteLine("║         SUDOKU BATTLE ONLINE - TCP SERVER        ║");
                Console.WriteLine("╠══════════════════════════════════════════════════╣");
                Console.WriteLine($"║  Cổng (Port)  : {_port,-33}║");
                Console.WriteLine($"║  Địa chỉ      : 0.0.0.0 (tất cả giao diện)    ║");
                Console.WriteLine($"║  Thời gian    : {DateTime.Now:yyyy-MM-dd HH:mm:ss,-33}║");
                Console.WriteLine("╠══════════════════════════════════════════════════╣");
                Console.WriteLine("║  Trạng thái   : ✓ SẴN SÀNG NHẬN KẾT NỐI       ║");
                Console.WriteLine("╚══════════════════════════════════════════════════╝");
                Console.WriteLine();

                // Vòng lặp chính: lắng nghe kết nối mới
                while (_isRunning)
                {
                    try
                    {
                        TcpClient tcpClient = await _listener.AcceptTcpClientAsync(_cts.Token);
                        _ = Task.Run(() => HandleNewClientAsync(tcpClient));
                    }
                    catch (OperationCanceledException)
                    {
                        // Server đang tắt, thoát vòng lặp bình thường
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[LỖI SOCKET] Không thể mở cổng {_port}: {ex.Message}");
                Console.WriteLine("[GỢI Ý] Kiểm tra xem cổng đã bị chiếm bởi tiến trình khác chưa.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI CRITICAL] Hệ thống Server gặp sự cố: {ex.Message}");
            }
            finally
            {
                Stop();
            }
        }

        // ═══════════════════════════════════════════════
        //  XỬ LÝ CLIENT MỚI KẾT NỐI
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Xử lý khi có một TcpClient mới kết nối tới Server.
        /// 1. Tạo ClientSession bọc TcpClient.
        /// 2. Đăng ký vào SessionManager.
        /// 3. Gắn sự kiện nhận dữ liệu -> PacketRouter.
        /// 4. Gắn sự kiện ngắt kết nối -> SessionManager dọn dẹp.
        /// 5. Bắt đầu vòng lặp nhận dữ liệu.
        /// </summary>
        private async Task HandleNewClientAsync(TcpClient tcpClient)
        {
            try
            {
                // Bảo vệ: Kiểm tra giới hạn tổng số kết nối
                if (_sessionManager.OnlineCount >= _maxConnections)
                {
                    Console.WriteLine($"[BẢO VỆ] Đạt giới hạn tối đa {_maxConnections} kết nối. Tạm thời từ chối.");
                    tcpClient.Close();
                    return;
                }

                // kiểm tra kết nối mỗi IP
                string ipAddress = ((IPEndPoint)tcpClient.Client.RemoteEndPoint!).Address.ToString();
                int currentConnections = _ipConnectionCounts.AddOrUpdate(ipAddress, 1, (key, count) => count + 1);

                if (currentConnections > _maxConnectionsPerIP)
                {
                    Console.WriteLine($"[BẢO VỆ] IP {ipAddress} vượt quá giới hạn {_maxConnectionsPerIP} kết nối/IP. Đã chặn.");
                    _ipConnectionCounts.AddOrUpdate(ipAddress, 0, (key, count) => count - 1);
                    tcpClient.Close();
                    return;
                }

                // 1. Tạo phiên kết nối mới
                ClientSession session = new ClientSession(tcpClient);

                // 2. Đăng ký phiên vào hệ thống quản lý
                _sessionManager.AddSession(session);

                // 3. Khi nhận dữ liệu JSON -> chuyển cho PacketRouter xử lý
                session.OnDataReceived += (sender, jsonLine) =>
                {
                    _packetRouter.Route(sender, jsonLine);
                };

                // 4. Khi client ngắt kết nối -> gỡ khỏi SessionManager và hàng đợi Matchmaking
                session.OnDisconnected += (sender) =>
                {
                    _sessionManager.RemoveSession(sender);
                    _matchmakingQueue.Remove(sender);

                    if (!string.IsNullOrEmpty(sender.CurrentRoomId))
                    {
                        _ = _packetHandler.HandlePlayerDisconnectForfeitAsync(sender);
                    }

                    // Giảm bộ đếm IP khi ngắt kết nối
                    _ipConnectionCounts.AddOrUpdate(ipAddress, 0, (key, count) => Math.Max(0, count - 1));
                };

                // 5. Bắt đầu lắng nghe dữ liệu (chạy cho đến khi client ngắt)
                await session.StartReceivingAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI MỞ KẾT NỐI] {ex.Message}");
                tcpClient.Close();
            }
        }

        // ═══════════════════════════════════════════════
        //  DỪNG SERVER
        // ═══════════════════════════════════════════════

        /// <summary>
        /// Dừng TCP Server, ngắt tất cả kết nối và giải phóng tài nguyên.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;

            // Hủy vòng lặp AcceptTcpClientAsync
            _cts.Cancel();

            // Ngắt kết nối tất cả client
            _sessionManager.DisconnectAll();

            // Dừng lắng nghe cổng
            _listener?.Stop();

            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════════╗");
            Console.WriteLine("║  Server đã DỪNG hoạt động.                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════╝");
        }
    }
}