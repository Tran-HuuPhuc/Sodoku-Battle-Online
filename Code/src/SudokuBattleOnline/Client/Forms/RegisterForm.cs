using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class RegisterForm : Form
    {
        // ── Controls ──────────────────────────────────────────────
        private TextBox txtUser        = null!;
        private TextBox txtPass        = null!;
        private TextBox txtConfirm     = null!;
        private Button  btnRegister    = null!;
        private Label   lblStatus      = null!;
        private Label   lblStrength    = null!;

        public RegisterForm()
        {
            // ── Cấu hình Form ──────────────────────────────────────
            Text            = "Sudoku Battle Online – Đăng ký";
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

            // Trang trí gradient strip bên phải
            var strip = new Panel
            {
                Size      = new Size(3, 560),
                Location  = new Point(447, 0),
                BackColor = UITheme.Accent
            };
            leftPanel.Controls.Add(strip);

            // Logo
            var lblLogo = new Label
            {
                Text      = "🎮",
                Font      = new Font("Segoe UI Emoji", 42, FontStyle.Regular),
                ForeColor = UITheme.Accent,
                AutoSize  = false,
                Size      = new Size(450, 80),
                Location  = new Point(0, 80),
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
                Size      = new Size(450, 42),
                Location  = new Point(0, 170),
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblGameName);

            // Tagline
            var lblTagline = new Label
            {
                Text      = "Tạo tài khoản miễn phí ngay hôm nay",
                Font      = new Font(UITheme.FontFamily, 11, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                AutoSize  = false,
                Size      = new Size(450, 26),
                Location  = new Point(0, 218),
                TextAlign = ContentAlignment.MiddleCenter
            };
            leftPanel.Controls.Add(lblTagline);

            // Divider
            var divider = new Label
            {
                Size      = new Size(280, 1),
                Location  = new Point(85, 262),
                BackColor = UITheme.BorderSub
            };
            leftPanel.Controls.Add(divider);

            // Bullets
            string[] bullets = {
                "✔  Thi đấu ngay sau khi đăng ký",
                "✔  Theo dõi điểm ELO của bạn",
                "✔  Miễn phí – không giới hạn ván đấu"
            };
            int bulletY = 282;
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

            // Server info
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

            // ── RIGHT PANEL – Register ──────────────────────────────
            var rightPanel = new Panel
            {
                Size      = new Size(450, 560),
                Location  = new Point(450, 0),
                BackColor = UITheme.BgCard
            };

            // Tiêu đề
            var lblTitle = new Label
            {
                Text      = "Tạo tài khoản",
                Font      = new Font(UITheme.FontFamily, 18, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                AutoSize  = false,
                Size      = new Size(350, 36),
                Location  = new Point(50, 50),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightPanel.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text      = "Điền đầy đủ thông tin để tạo tài khoản mới.",
                Font      = new Font(UITheme.FontFamily, 9, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                AutoSize  = false,
                Size      = new Size(350, 20),
                Location  = new Point(50, 88),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightPanel.Controls.Add(lblSub);

            // ── Username ──────────────────────────────────────────
            var lblUser = new Label
            {
                Text      = "Tên đăng nhập",
                Font      = new Font(UITheme.FontFamily, 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(50, 124)
            };
            rightPanel.Controls.Add(lblUser);

            txtUser = UITheme.MakeInput(350, 38);
            txtUser.Location = new Point(50, 144);
            txtUser.Font     = new Font(UITheme.FontFamily, 10.5f, FontStyle.Regular);
            rightPanel.Controls.Add(txtUser);

            // ── Password ──────────────────────────────────────────
            var lblPass = new Label
            {
                Text      = "Mật khẩu",
                Font      = new Font(UITheme.FontFamily, 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(50, 198)
            };
            rightPanel.Controls.Add(lblPass);

            txtPass = UITheme.MakeInput(350, 38, password: true);
            txtPass.Location  = new Point(50, 218);
            txtPass.Font      = new Font(UITheme.FontFamily, 10.5f, FontStyle.Regular);
            rightPanel.Controls.Add(txtPass);

            // Password strength indicator
            lblStrength = new Label
            {
                Text      = "",
                Font      = new Font(UITheme.FontFamily, 8.5f, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                AutoSize  = false,
                Size      = new Size(350, 18),
                Location  = new Point(50, 260),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightPanel.Controls.Add(lblStrength);

            txtPass.TextChanged += (s, e) => UpdateStrength(txtPass.Text);

            // ── Confirm Password ──────────────────────────────────
            var lblConfirm = new Label
            {
                Text      = "Xác nhận mật khẩu",
                Font      = new Font(UITheme.FontFamily, 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(50, 284)
            };
            rightPanel.Controls.Add(lblConfirm);

            txtConfirm = UITheme.MakeInput(350, 38, password: true);
            txtConfirm.Location = new Point(50, 304);
            txtConfirm.Font     = new Font(UITheme.FontFamily, 10.5f, FontStyle.Regular);
            rightPanel.Controls.Add(txtConfirm);

            // ── Status label ──────────────────────────────────────
            lblStatus = new Label
            {
                Text      = "",
                Font      = new Font(UITheme.FontFamily, 9, FontStyle.Regular),
                ForeColor = UITheme.Danger,
                AutoSize  = false,
                Size      = new Size(350, 20),
                Location  = new Point(50, 350),
                TextAlign = ContentAlignment.MiddleLeft
            };
            rightPanel.Controls.Add(lblStatus);

            // ── Register button ───────────────────────────────────
            btnRegister = UITheme.MakeButton("ĐĂNG KÝ NGAY", 350, 46);
            btnRegister.Location = new Point(50, 376);
            btnRegister.Font     = new Font(UITheme.FontFamily, 11, FontStyle.Bold);
            rightPanel.Controls.Add(btnRegister);

            // ── Divider ───────────────────────────────────────────
            var sep = UITheme.MakeSeparator(350);
            sep.Location = new Point(50, 438);
            rightPanel.Controls.Add(sep);

            var lblOr = new Label
            {
                Text      = "hoặc",
                Font      = new Font(UITheme.FontFamily, 8.5f, FontStyle.Regular),
                ForeColor = UITheme.TextMuted,
                AutoSize  = false,
                Size      = new Size(350, 20),
                Location  = new Point(50, 448),
                TextAlign = ContentAlignment.MiddleCenter
            };
            rightPanel.Controls.Add(lblOr);

            // ── Back (outline) button ─────────────────────────────
            var btnBack = UITheme.MakeOutlineButton("Đã có tài khoản? Đăng nhập", 350, 44);
            btnBack.Location = new Point(50, 474);
            btnBack.Font     = new Font(UITheme.FontFamily, 10, FontStyle.Regular);
            btnBack.Click   += (s, e) => Close();
            rightPanel.Controls.Add(btnBack);

            Controls.Add(rightPanel);

            // ── Wire events ───────────────────────────────────────
            btnRegister.Click += BtnRegister_Click;
            AcceptButton       = btnRegister;
            CancelButton       = btnBack;
        }

        // ────────────────────────────────────────────────────────────
        //  PASSWORD STRENGTH INDICATOR
        // ────────────────────────────────────────────────────────────
        private void UpdateStrength(string pass)
        {
            if (pass.Length == 0)
            {
                lblStrength.Text      = "";
                lblStrength.ForeColor = UITheme.TextMuted;
            }
            else if (pass.Length < 6)
            {
                lblStrength.Text      = "🔴  Độ mạnh: Yếu";
                lblStrength.ForeColor = UITheme.Danger;
            }
            else if (pass.Length < 10)
            {
                lblStrength.Text      = "🟡  Độ mạnh: Trung bình";
                lblStrength.ForeColor = UITheme.Warning;
            }
            else
            {
                lblStrength.Text      = "🟢  Độ mạnh: Mạnh";
                lblStrength.ForeColor = UITheme.Success;
            }
        }

        // ────────────────────────────────────────────────────────────
        //  EVENTS
        // ────────────────────────────────────────────────────────────
        private async void BtnRegister_Click(object? sender, EventArgs e)
        {
            string username        = txtUser.Text.Trim();
            string password        = txtPass.Text;
            string confirmPassword = txtConfirm.Text;

            lblStatus.Text = "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblStatus.ForeColor = UITheme.Danger;
                lblStatus.Text      = "⚠  Username và Password không được để trống.";
                return;
            }

            if (password != confirmPassword)
            {
                lblStatus.ForeColor = UITheme.Danger;
                lblStatus.Text      = "⚠  Mật khẩu xác nhận không khớp.";
                return;
            }

            btnRegister.Enabled = false;
            btnRegister.Text    = "Đang đăng ký...";
            try
            {
                var request = new RegisterPacket
                {
                    PacketType      = "REGISTER",
                    Username        = username,
                    Password        = password,
                    ConfirmPassword = confirmPassword
                };

                RegisterPacket? response = await AppSession.SendAndWaitAsync<RegisterPacket>(request, "REGISTER_RESULT");
                if (response == null)
                {
                    lblStatus.ForeColor = UITheme.Danger;
                    lblStatus.Text      = "✕  Server không trả về dữ liệu đăng ký.";
                    return;
                }

                if (!response.Success)
                {
                    lblStatus.ForeColor = UITheme.Danger;
                    lblStatus.Text      = "✕  " + response.Message;
                    return;
                }

                lblStatus.ForeColor = UITheme.Success;
                lblStatus.Text      = "✔  " + response.Message;

                MessageBox.Show(
                    response.Message + "\nBạn có thể đăng nhập bằng tài khoản vừa tạo.",
                    "Đăng ký thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = UITheme.Danger;
                lblStatus.Text      = "✕  Không kết nối được Server.";
                MessageBox.Show(
                    "Không kết nối được Server. Hãy chạy Server trước rồi đăng ký.\n\nChi tiết: " + ex.Message,
                    "Lỗi kết nối Server",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text    = "ĐĂNG KÝ NGAY";
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtUser.Focus();
        }
    }
}
