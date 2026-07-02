using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public partial class LoginForm : Form
    {
        // ── Controls được khai báo ở class level để dễ tham chiếu ──
        private TextBox txtUser   = null!;
        private TextBox txtPass   = null!;
        private Button  btnLogin  = null!;
        private Label   lblStatus = null!;

        public LoginForm()
        {
            // ── Cấu hình Form ──────────────────────────────────────
            Text            = "Sudoku Battle Online";
            Size            = new Size(900, 560);
            MinimumSize     = new Size(900, 560);
            MaximumSize     = new Size(900, 560);
            StartPosition   = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox     = false;
            BackColor       = UITheme.BgMain;
            AutoScaleMode   = AutoScaleMode.Font;

            BuildLayout();
        }

        // ────────────────────────────────────────────────────────────
        //  BUILD LAYOUT
        // ────────────────────────────────────────────────────────────
        private void BuildLayout()
        {
            // ── LEFT PANEL – Branding ───────────────────────────────
            var leftPanel = new Panel
            {
                Size      = new Size(450, 560),
                Location  = new Point(0, 0),
                BackColor = UITheme.BgDeep
            };

            // Trang trí gradient strip bên phải left panel
            var strip = new Panel
            {
                Size      = new Size(3, 560),
                Location  = new Point(447, 0),
                BackColor = UITheme.Accent
            };
            leftPanel.Controls.Add(strip);

            // Logo emoji
            var lblLogo = new Label
            {
                Text      = "🎮",
                Font      = new Font("Segoe UI Emoji", 42, FontStyle.Regular),
                ForeColor = UITheme.Accent,
                AutoSize  = false,
                Size      = new Size(450, 100),
                Location  = new Point(0, 100),
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblLogo);

            // Tên game
            var lblGameName = new Label
            {
                Text      = "SUDOKU BATTLE",
                Font      = new Font(UITheme.FontFamily, 28, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                AutoSize  = false,
                Size      = new Size(450, 60),
                Location  = new Point(0, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblGameName);

            // Tagline
            var lblTagline = new Label
            {
                Text      = "Thách đấu trực tuyến",
                Font      = new Font(UITheme.FontFamily, 12, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                AutoSize  = false,
                Size      = new Size(450, 26),
                Location  = new Point(0, 238),
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblTagline);

            // Divider
            var divider = new Label
            {
                Size      = new Size(280, 1),
                Location  = new Point(85, 282),
                BackColor = UITheme.BorderSub
            };
            leftPanel.Controls.Add(divider);

            // Bullet features
            string[] bullets = {
                "⚔  Đấu 1v1 real-time",
                "🏆  Bảng xếp hạng ELO",
                "🧩  Thuật toán Sudoku thông minh"
            };
            int bulletY = 302;
            foreach (var b in bullets)
            {
                var lbl = new Label
                {
                    Text      = b,
                    Font      = new Font(UITheme.FontFamily, 10.5f, FontStyle.Regular),
                    ForeColor = UITheme.TextSecondary,
                    AutoSize  = false,
                    Size      = new Size(370, 28),
                    Location  = new Point(40, bulletY),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                leftPanel.Controls.Add(lbl);
                bulletY += 36;
            }

            // Server info nhỏ ở cuối
            var lblServer = new Label
            {
                Text      = $"Server  {AppSession.ServerIp}:{AppSession.ServerPort}",
                Font      = new Font(UITheme.FontFamily, 8, FontStyle.Regular),
                ForeColor = UITheme.TextMuted,
                AutoSize  = false,
                Size      = new Size(450, 20),
                Location  = new Point(0, 520),
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblServer);

            Controls.Add(leftPanel);

            // ── RIGHT PANEL – Login ─────────────────────────────────
            var rightPanel = new Panel
            {
                Size      = new Size(450, 560),
                Location  = new Point(450, 0),
                BackColor = UITheme.BgCard
            };

            // Tiêu đề form
            var lblTitle = new Label
            {
                Text      = "Đăng nhập",
                Font      = new Font(UITheme.FontFamily, 18, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                AutoSize  = false,
                Size      = new Size(350, 36),
                Location  = new Point(50, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightPanel.Controls.Add(lblTitle);

            var lblWelcome = new Label
            {
                Text      = "Chào mừng trở lại! Vui lòng đăng nhập.",
                Font      = new Font(UITheme.FontFamily, 9, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                AutoSize  = false,
                Size      = new Size(350, 20),
                Location  = new Point(50, 120),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightPanel.Controls.Add(lblWelcome);

            // ── Username ─────────────────────────────────────────
            var lblUser = new Label
            {
                Text      = "Tên đăng nhập",
                Font      = new Font(UITheme.FontFamily, 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(50, 168)
            };
            rightPanel.Controls.Add(lblUser);

            txtUser = UITheme.MakeInput(350, 40);
            txtUser.Location  = new Point(50, 190);
            txtUser.Font      = new Font(UITheme.FontFamily, 10.5f, FontStyle.Regular);
            rightPanel.Controls.Add(txtUser);

            // ── Password ──────────────────────────────────────────
            var lblPass = new Label
            {
                Text      = "Mật khẩu",
                Font      = new Font(UITheme.FontFamily, 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(50, 246)
            };
            rightPanel.Controls.Add(lblPass);

            txtPass = UITheme.MakeInput(350, 40, password: true);
            txtPass.Location  = new Point(50, 268);
            txtPass.Font      = new Font(UITheme.FontFamily, 10.5f, FontStyle.Regular);
            rightPanel.Controls.Add(txtPass);

            // ── Status label ─────────────────────────────────────
            lblStatus = new Label
            {
                Text      = "",
                Font      = new Font(UITheme.FontFamily, 9, FontStyle.Regular),
                ForeColor = UITheme.Danger,
                AutoSize  = false,
                Size      = new Size(350, 22),
                Location  = new Point(50, 316),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightPanel.Controls.Add(lblStatus);

            // ── Login button ─────────────────────────────────────
            btnLogin = UITheme.MakeButton("ĐĂNG NHẬP", 350, 46);
            btnLogin.Location = new Point(50, 346);
            btnLogin.Font     = new Font(UITheme.FontFamily, 11, FontStyle.Bold);
            rightPanel.Controls.Add(btnLogin);

            // ── Divider ───────────────────────────────────────────
            var sep = UITheme.MakeSeparator(350);
            sep.Location = new Point(50, 410);
            rightPanel.Controls.Add(sep);

            var lblOr = new Label
            {
                Text      = "hoặc",
                Font      = new Font(UITheme.FontFamily, 8.5f, FontStyle.Regular),
                ForeColor = UITheme.TextMuted,
                AutoSize  = false,
                Size      = new Size(350, 20),
                Location  = new Point(50, 420),
                TextAlign = ContentAlignment.MiddleCenter
            };
            rightPanel.Controls.Add(lblOr);

            // ── Register outline button ───────────────────────────
            var btnRegister = UITheme.MakeOutlineButton("Chưa có tài khoản? Đăng ký", 350, 44);
            btnRegister.Location = new Point(50, 448);
            btnRegister.Font     = new Font(UITheme.FontFamily, 10, FontStyle.Regular);
            rightPanel.Controls.Add(btnRegister);

            Controls.Add(rightPanel);

            // ── Wire events ───────────────────────────────────────
            btnLogin.Click   += BtnLogin_Click;
            btnRegister.Click += (s, e) =>
            {
                using var reg = new RegisterForm();
                reg.ShowDialog();
            };

            AcceptButton = btnLogin;
        }

        // ────────────────────────────────────────────────────────────
        //  EVENTS
        // ────────────────────────────────────────────────────────────
        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text;

            lblStatus.Text = "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.Text = "⚠  Vui lòng nhập username và password.";
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text    = "Đang đăng nhập...";
            try
            {
                var request = new LoginPacket
                {
                    PacketType = "LOGIN",
                    Username   = username,
                    Password   = password
                };

                LoginPacket? response = await AppSession.SendAndWaitAsync<LoginPacket>(request, "LOGIN_RESULT");
                if (response == null)
                {
                    lblStatus.Text = "✕  Server không trả về dữ liệu đăng nhập.";
                    return;
                }

                if (!response.Success)
                {
                    lblStatus.ForeColor = UITheme.Danger;
                    lblStatus.Text      = "✕  " + response.Message;
                    return;
                }

                AppSession.CurrentUsername = response.Username;

                var menu = new MainMenuForm();
                menu.FormClosed += (sender2, args) => this.Close();
                menu.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = UITheme.Danger;
                lblStatus.Text      = "✕  Không kết nối được Server.";
                MessageBox.Show(
                    "Không kết nối được Server. Hãy chạy Server trước sau đó mở Client.\n\nChi tiết: " + ex.Message,
                    "Lỗi kết nối Server Thất Bại!!!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text    = "ĐĂNG NHẬP";
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtUser.Focus();
        }

        private void LoginForm_Load(object sender, EventArgs e) { }
    }
}
