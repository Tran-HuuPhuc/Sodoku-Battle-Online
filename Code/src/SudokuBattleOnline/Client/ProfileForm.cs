using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class ProfileForm : Form
    {
        private readonly Label lblContent;

        public ProfileForm()
        {
            Text = "Hồ sơ người chơi";
            Size = new Size(500, 380);
            StartPosition = FormStartPosition.CenterParent;

            Label lblTitle = new Label
            {
                Text = "THÔNG TIN TÀI KHOẢN",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(30, 25),
                AutoSize = true
            };

            lblContent = new Label
            {
                Text = "Đang lấy dữ liệu từ Server...",
                Location = new Point(30, 75),
                Size = new Size(430, 240),
                Font = new Font("Arial", 10, FontStyle.Regular)
            };

            Controls.Add(lblTitle);
            Controls.Add(lblContent);

            Shown += async (s, e) => await LoadProfileFromServerAsync();
        }

        private async System.Threading.Tasks.Task LoadProfileFromServerAsync()
        {
            if (!AppSession.IsLoggedIn)
            {
                lblContent.Text = "Chưa đăng nhập.";
                return;
            }

            try
            {
                var request = new UserProfilePacket
                {
                    PacketType = "PROFILE"
                };

                UserProfilePacket? response = await AppSession.SendAndWaitAsync<UserProfilePacket>(request, "PROFILE_RESULT");
                if (response == null)
                {
                    lblContent.Text = "Server không trả về dữ liệu hồ sơ.";
                    return;
                }

                if (!response.Success)
                {
                    lblContent.Text = response.Message;
                    return;
                }

                lblContent.Text =
                    $"ID: {response.Id}\n" +
                    $"Username: {response.Username}\n" +
                    $"ELO: {response.Elo}\n" +
                    $"Thắng: {response.TotalWins}\n" +
                    $"Thua: {response.TotalLosses}\n" +
                    $"Ngày tạo: {response.CreatedAt}\n\n" +
                    $"Dữ liệu lấy từ SQLite phía Server: database/sudoku.db";
            }
            catch (Exception ex)
            {
                lblContent.Text = "Không lấy được hồ sơ từ Server.\n" + ex.Message;
            }
        }
    }
}
