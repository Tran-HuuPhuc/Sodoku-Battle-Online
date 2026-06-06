using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SudokuBattle.Server.Network
{
    public class TcpServer
    {
        private TcpListener _listener;
        private readonly int _port = 8888; // Định nghĩa cổng kết nối hệ thống
        private bool _isRunning;

        // Danh sách lưu trữ tập trung các Client đang trực tuyến
        private readonly List<TcpClient> _connectedClients = new List<TcpClient>();

        public async Task StartAsync()
        {
            try
            {
                // 1. Khởi tạo và kích hoạt lắng nghe tại cổng 8888
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                _isRunning = true;

                Console.WriteLine($"[SERVER] Sudoku TCP Server đã mở thành công tại cổng {_port}...");
                Console.WriteLine("[SERVER] Đang sẵn sàng đón nhận các client kết nối vào...\n---");

                // 2. Vòng lặp vô tận dùng async/await để liên tục đón client mới mà không gây nghẽn luồng
                while (_isRunning)
                {
                    // Đứng đợi kết nối bất đồng bộ từ client
                    TcpClient client = await _listener.AcceptTcpClientAsync();

                    // Sử dụng lock để tránh xung đột tài nguyên mạng khi thêm client vào danh sách
                    lock (_connectedClients)
                    {
                        _connectedClients.Add(client);
                    }
                    Console.WriteLine($"[KẾT NỐI] Có người chơi mới tham gia! Tổng số trực tuyến: {_connectedClients.Count}");

                    // 3. Đẩy Client này vào một luồng xử lý riêng biệt để quản lý việc nhận dữ liệu
                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI CRITICAL] Hệ thống Server gặp sự cố: {ex.Message}");
            }
        }

        // Hàm xử lý truyền nhận dữ liệu độc lập cho từng phiên kết nối của client
        private async Task HandleClientAsync(TcpClient client)
        {
            // Lấy luồng dữ liệu mạng để đọc/ghi
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[4096]; // Cấu hình kích thước bộ đệm < 4KB theo yêu cầu phi chức năng
                int bytesRead;

                try
                {
                    // Vòng lặp liên tục lắng nghe thông điệp thô gửi lên từ client
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        // Giải mã mảng byte thành chuỗi ký tự UTF-8
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"[DỮ LIỆU] Nhận từ Client: {receivedData}");

                        // Thử nghiệm cơ chế Echo: Phản hồi lại thông điệp vừa nhận để Client kiểm tra kết nối
                        byte[] responseData = Encoding.UTF8.GetBytes($"[SERVER ECHO]: {receivedData}");
                        await stream.WriteAsync(responseData, 0, responseData.Length);
                    }
                }
                catch (Exception)
                {
                    // Bắt các lỗi ngắt kết nối vật lý hoặc crash app phía Client
                }
                finally
                {
                    // 4. Dọn dẹp bộ nhớ và xóa client khỏi danh sách quản lý khi ngắt kết nối
                    lock (_connectedClients)
                    {
                        _connectedClients.Remove(client);
                    }
                    client.Close();
                    Console.WriteLine($"[NGẮT KẾT NỐI] Một người chơi đã rời đi. Còn lại: {_connectedClients.Count}");
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            Console.WriteLine("[SERVER] Đã dừng lắng nghe hệ thống.");
        }
    }
}