using Client.Network; // Đảm bảo import đúng thư mục mạng chứa ClientConnection
using SudokuBattleOnline.Shared.Packets; // Thêm dòng này để gọi được LoginPacket từ Shared
using SudokuBattleOnline;
using SudokuBattleOnline.Forms; // Import thư mục chứa LoginForm nếu cần
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

//namespace Client
//{
//    internal static class Program
//    {
//        // Khởi tạo một thực thể static dùng chung toàn hệ thống Client
//        public static ClientConnection NetworkClient { get; private set; }

//        /// <summary>
//        ///  The main entry point for the application.
//        /// </summary>
//        [STAThread]
//        static async Task Main() // Đồng bộ chuẩn Async Task để không treo Form
//        {
//            ApplicationConfiguration.Initialize();

//            // 1. Khởi tạo đối tượng kết nối mạng tập trung
//            NetworkClient = new ClientConnection();

//            // Đăng ký nhận dữ liệu thử nghiệm từ Server trả về (Echo)
//            NetworkClient.OnMessageReceived += (msg) =>
//            {
//                System.Diagnostics.Debug.WriteLine($"[CLIENT NHẬN BẤT ĐỒNG BỘ] {msg}");
//            };

//            try
//            {
//                // 2. Thực hiện kết nối ngầm tới Server (IP Localhost: 127.0.0.1, Port: 8888)
//                // Dùng await giúp Form không bị đơ/đóng băng khi đang tìm kiếm Server mạng
//                await NetworkClient.ConnectAsync("127.0.0.1", 8888);

//                // ========================================================
//                // ĐOẠN BƯỚC 4: THỬ NGHIỆM GỬI GÓI TIN THỰC TẾ ĐƯỢC CHÈN VÀO ĐÂY:
//                // ========================================================
//                LoginPacket testLogin = new LoginPacket
//                {
//                    Username = "test",
//                    Password = "test"
//                };

//                // Gọi hàm bắn gói tin JSON đã cài đặt ở ClientConnection sang Server
//                await NetworkClient.SendPacketAsync(testLogin);
//                // ========================================================

//                // 3. Gửi gói tin chào hỏi đầu tiên ngay khi kết nối thành công để test luồng (Giữ lại code cũ của fen)
//                await NetworkClient.SendRawMessageAsync("Hello Server! Tôi là Client đã kết nối thành công từ WinForm.");
//            }
//            catch (Exception ex)
//            {
//                // Nếu Server chưa bật, ghi log tạm thời để tránh crash app Client đột ngột
//                System.Diagnostics.Debug.WriteLine($"[LỖI KẾT NỐI BAN ĐẦU]: {ex.Message}");
//            }

//            // 4. Bật giao diện LoginForm lên bình thường cho người chơi tương tác
//            Application.Run(new LoginForm());
//        }
//    }
//}
//Chưa hiểu lỗi chổ nào:))

namespace Client
{
    internal static class Program
    {
        public static ClientConnection NetworkClient { get; private set; }

        [STAThread]
        // BƯỚC 1: ĐỔI LẠI THÀNH void CHUẨN CỦA WINFORMS ĐỂ KHÔNG BỊ MẤT LUỒNG CHÍNH
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            NetworkClient = new ClientConnection();

            NetworkClient.OnMessageReceived += (msg) =>
            {
                System.Diagnostics.Debug.WriteLine($"[CLIENT NHẬN] {msg}");
            };

            // BƯỚC 2: NÉM TOÀN BỘ KẾT NỐI MẠNG VÀO MỘT TASK CHẠY NGẦM ĐỘC LẬP
            _ = Task.Run(async () =>
            {
                try
                {
                    // Chạy kết nối ngầm thoải mái không sợ đơ Form
                    await NetworkClient.ConnectAsync("127.0.0.1", 8888);

                    // Khởi tạo gói tin JSON
                    LoginPacket testLogin = new LoginPacket
                    {
                        Username = "test   ",
                        Password = "test"
                    };

                    // Bắn gói JSON đi
                    await NetworkClient.SendPacketAsync(testLogin);

                    // BƯỚC 3: XOÁ HẲN DÒNG GỬI "SendRawMessageAsync" ĐI. 
                    // Server đã nâng cấp lên chuẩn JSON, cấm tuyệt đối gửi chuỗi text thô sang làm sập Server!
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LỖI MẠNG]: {ex.Message}");
                }
            });

            // Giao diện sẽ hiển thị mượt mà và không bao giờ bị văng app nữa
            Application.Run(new LoginForm());
        }
    }
}