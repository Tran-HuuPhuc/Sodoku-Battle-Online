using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class ProfileForm : Form
    {
        private Label lblAvatar = null!;
        private Label lblUsername = null!;
        private Label lblSubtitle = null!;

        private Label lblEloValue = null!;
        private Label lblWinValue = null!;
        private Label lblLossValue = null!;

        private Label lblCreatedAt = null!;
        private Label lblStatus = null!;

        public ProfileForm()
        {
            InitializeProfileUI();
            Shown += async (s, e) => await LoadProfileFromServerAsync();
        }

        private void InitializeProfileUI()
        {
            SuspendLayout();

            Text = "Hồ Sơ Người Chơi";
            BackColor = UITheme.BgMain;
            ForeColor = UITheme.TextPrimary;
            Size = new Size(900, 560);
            MinimumSize = new Size(800, 500);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;
            AutoScaleMode = AutoScaleMode.None;
            Font = new Font(UITheme.FontFamily, 10F, FontStyle.Regular);

            Controls.Clear();

            // ================= HEADER =================

            Panel headerPanel = new Panel
            {
                Location = new Point(40, 35),
                Size = new Size(760, 95),
                BackColor = UITheme.BgMain
            };

            lblAvatar = new Label
            {
                Text = "USER",
                Location = new Point(0, 0),
                Size = new Size(82, 82),
                Font = new Font(UITheme.FontFamily, 15F, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                BackColor = UITheme.BgCard,
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using Pen pen = new Pen(UITheme.Border, 2);
                Rectangle rect = new Rectangle(1, 1, lblAvatar.Width - 3, lblAvatar.Height - 3);
                e.Graphics.DrawEllipse(pen, rect);
            };

            lblUsername = new Label
            {
                Text = AppSession.IsLoggedIn ? AppSession.CurrentUsername : "Chưa đăng nhập",
                Location = new Point(110, 5),
                Size = new Size(600, 60),
                Font = new Font(UITheme.FontFamily, 26F, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                BackColor = UITheme.BgMain,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblSubtitle = new Label
            {
                Text = "Thông tin tài khoản và thống kê người chơi",
                Location = new Point(112, 65),
                Size = new Size(620, 28),
                Font = new Font(UITheme.FontFamily, 12F, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                BackColor = UITheme.BgMain,
                TextAlign = ContentAlignment.MiddleLeft
            };

            headerPanel.Controls.Add(lblAvatar);
            headerPanel.Controls.Add(lblUsername);
            headerPanel.Controls.Add(lblSubtitle);

            Panel separator = new Panel
            {
                Location = new Point(40, 145),
                Size = new Size(760, 1),
                BackColor = UITheme.Border
            };

            // ================= STAT CARDS =================

            Panel cardElo = CreateStatCard(
                location: new Point(40, 180),
                valueLabel: out lblEloValue,
                valueText: "—",
                captionText: "ELO Rating",
                valueColor: UITheme.Gold
            );

            Panel cardWin = CreateStatCard(
                location: new Point(270, 180),
                valueLabel: out lblWinValue,
                valueText: "—",
                captionText: "Thắng",
                valueColor: UITheme.Success
            );

            Panel cardLoss = CreateStatCard(
                location: new Point(500, 180),
                valueLabel: out lblLossValue,
                valueText: "—",
                captionText: "Thua",
                valueColor: UITheme.Danger
            );

            // ================= INFO =================

            lblCreatedAt = new Label
            {
                Text = "Ngày tạo tài khoản: —",
                Location = new Point(45, 330),
                Size = new Size(720, 30),
                Font = new Font(UITheme.FontFamily, 12F, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                BackColor = UITheme.BgMain,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblStatus = new Label
            {
                Text = "Đang lấy dữ liệu từ Server...",
                Location = new Point(45, 365),
                Size = new Size(720, 30),
                Font = new Font(UITheme.FontFamily, 10F, FontStyle.Regular),
                ForeColor = UITheme.TextMuted,
                BackColor = UITheme.BgMain,
                TextAlign = ContentAlignment.MiddleLeft
            };

            Button btnClose = UITheme.MakeOutlineButton("Đóng", 130, 40);
            btnClose.Location = new Point(640, 430);
            btnClose.Click += (s, e) => Close();

            Controls.Add(headerPanel);
            Controls.Add(separator);
            Controls.Add(cardElo);
            Controls.Add(cardWin);
            Controls.Add(cardLoss);
            Controls.Add(lblCreatedAt);
            Controls.Add(lblStatus);
            Controls.Add(btnClose);

            ResumeLayout(false);
        }

        private Panel CreateStatCard(
            Point location,
            out Label valueLabel,
            string valueText,
            string captionText,
            Color valueColor)
        {
            Panel card = new Panel
            {
                Location = location,
                Size = new Size(190, 120),
                BackColor = UITheme.BgCard
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using Pen pen = new Pen(UITheme.Border, 1);
                Rectangle rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                e.Graphics.DrawRectangle(pen, rect);
            };

            valueLabel = new Label
            {
                Text = valueText,
                Location = new Point(0, 22),
                Size = new Size(card.Width, 45),
                Font = new Font(UITheme.FontFamily, 22F, FontStyle.Bold),
                ForeColor = valueColor,
                BackColor = UITheme.BgCard,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            Label captionLabel = new Label
            {
                Text = captionText,
                Location = new Point(0, 70),
                Size = new Size(card.Width, 30),
                Font = new Font(UITheme.FontFamily, 10F, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                BackColor = UITheme.BgCard,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            card.Controls.Add(valueLabel);
            card.Controls.Add(captionLabel);

            return card;
        }

        private async Task LoadProfileFromServerAsync()
        {
            if (!AppSession.IsLoggedIn)
            {
                lblStatus.Text = "Chưa đăng nhập.";
                lblStatus.ForeColor = UITheme.Danger;
                return;
            }

            try
            {
                lblStatus.Text = "Đang lấy dữ liệu từ Server...";
                lblStatus.ForeColor = UITheme.TextMuted;

                var request = new UserProfilePacket
                {
                    PacketType = "PROFILE"
                };

                UserProfilePacket? response =
                    await AppSession.SendAndWaitAsync<UserProfilePacket>(
                        request,
                        "PROFILE_RESULT"
                    );

                if (response == null)
                {
                    lblStatus.Text = "Server không trả về dữ liệu hồ sơ.";
                    lblStatus.ForeColor = UITheme.Danger;
                    return;
                }

                if (!response.Success)
                {
                    lblStatus.Text = response.Message;
                    lblStatus.ForeColor = UITheme.Danger;
                    return;
                }

                lblUsername.Text = string.IsNullOrWhiteSpace(response.Username)
                    ? AppSession.CurrentUsername
                    : response.Username;

                lblEloValue.Text = response.Elo.ToString();
                lblWinValue.Text = response.TotalWins.ToString();
                lblLossValue.Text = response.TotalLosses.ToString();

                lblCreatedAt.Text = "Ngày tạo tài khoản: " + response.CreatedAt;

                lblStatus.Text = "Dữ liệu được lấy từ Server.";
                lblStatus.ForeColor = UITheme.Success;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không lấy được hồ sơ từ Server: " + ex.Message;
                lblStatus.ForeColor = UITheme.Danger;
            }
        }
    }
}