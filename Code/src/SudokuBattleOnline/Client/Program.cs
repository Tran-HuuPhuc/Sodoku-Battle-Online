//using Client.Network; // Đảm bảo import đúng thư mục mạng chứa ClientConnection
//using SudokuBattleOnline.Shared.Packets; // Thêm dòng này để gọi được LoginPacket từ Shared
//using SudokuBattleOnline;
//using SudokuBattleOnline.Forms; // Import thư mục chứa LoginForm nếu cần
//using System;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//namespace Client
//{
//    internal static class Program
//    {
//        public static ClientConnection NetworkClient { get; private set; }

//        [STAThread]
//        // BƯỚC 1: ĐỔI LẠI THÀNH void CHUẨN CỦA WINFORMS ĐỂ KHÔNG BỊ MẤT LUỒNG CHÍNH
//        static void Main()
//        {
//            ApplicationConfiguration.Initialize();
//            NetworkClient = new ClientConnection();

//            NetworkClient.OnMessageReceived += (msg) =>
//            {
//                System.Diagnostics.Debug.WriteLine($"[CLIENT NHẬN] {msg}");
//            };

//            // BƯỚC 2: NÉM TOÀN BỘ KẾT NỐI MẠNG VÀO MỘT TASK CHẠY NGẦM ĐỘC LẬP
//            _ = Task.Run(async () =>
//            {
//                try
//                {
//                    // Chạy kết nối ngầm thoải mái không sợ đơ Form
//                    await NetworkClient.ConnectAsync("127.0.0.1", 8888);

//                    // Khởi tạo gói tin JSON
//                    LoginPacket testLogin = new LoginPacket
//                    {
//                        Username = "test   ",
//                        Password = "test"
//                    };

//                    // Bắn gói JSON đi
//                    await NetworkClient.SendPacketAsync(testLogin);

//                    // BƯỚC 3: XOÁ HẲN DÒNG GỬI "SendRawMessageAsync" ĐI. 
//                    // Server đã nâng cấp lên chuẩn JSON, cấm tuyệt đối gửi chuỗi text thô sang làm sập Server!
//                }
//                catch (Exception ex)
//                {
//                    System.Diagnostics.Debug.WriteLine($"[LỖI MẠNG]: {ex.Message}");
//                }
//            });

//            // Giao diện sẽ hiển thị mượt mà và không bao giờ bị văng app nữa
//            Application.Run(new LoginForm());
//        }
//    }
//}

using SudokuBattleOnline.Forms;
using System;
using System.Windows.Forms;

namespace Client
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }
}
