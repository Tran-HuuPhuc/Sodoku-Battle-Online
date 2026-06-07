using System;
<<<<<<< HEAD
using System.Windows.Forms;
using SudokuBattleOnline.Forms;

namespace SudokuBattleOnline
{
    internal static class Program
    {
=======
using System.Threading.Tasks;
using System.Windows.Forms;
using Client.Network; // Đảm bảo import đúng namespace của class ClientConnection

namespace Client
{
    internal static class Program
    {
        // Khởi tạo một thực thể static dùng chung toàn hệ thống Client
        public static ClientConnection NetworkClient { get; private set; }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
>>>>>>> upstream/main
        [STAThread]
        static async Task Main() // Chuyển void thành async Task để chạy bất đồng bộ
        {
            ApplicationConfiguration.Initialize();

<<<<<<< HEAD
=======
            // 1. Khởi tạo đối tượng kết nối mạng
            NetworkClient = new ClientConnection();

            // Đăng ký thử nghiệm sự kiện nhận dữ liệu (Echo) từ Server để kiểm tra đường truyền
            NetworkClient.OnMessageReceived += (msg) =>
            {
                // In tạm ra cửa sổ Output hoặc thông báo nhanh khi nhận được phản hồi từ Server
                System.Diagnostics.Debug.WriteLine($"[CLIENT NHẬN] {msg}");
            };

            // 2. Thực hiện kết nối ngầm tới Server (IP Localhost: 127.0.0.1, Port: 8888)
            // Dùng await giúp Form không bị đóng băng, đơ cứng khi đang tìm kiếm Server
            Console.WriteLine("[CLIENT] Đang kết nối tới Server...");
            await NetworkClient.ConnectAsync("127.0.0.1", 8888);

            // 3. Gửi gói tin chào hỏi đầu tiên để test luồng truyền nhận thô
            await NetworkClient.SendMessageAsync("Hello Server! Tôi là Client đã kết nối thành công từ WinForm.");

            // 4. Bật giao diện LoginForm lên bình thường cho người chơi thao tác
>>>>>>> upstream/main
            Application.Run(new LoginForm());
        }
    }
}