using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            Text = "Đăng nhập";
            Size = new Size(600, 450);
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Font;

            // Create outer container panel
            Panel containerPanel = new Panel();
            containerPanel.AutoSize = true;
            containerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            containerPanel.Dock = DockStyle.None;
            containerPanel.Anchor = AnchorStyles.None;

            Label lblTitle = new Label();
            lblTitle.Text = "SUDOKU BATTLE ONLINE";
            lblTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(0, 0);
            containerPanel.Controls.Add(lblTitle);

            Label lblServer = new Label();
            lblServer.Text = $"Server: {AppSession.ServerIp}:{AppSession.ServerPort}";
            lblServer.AutoSize = true;
            lblServer.Location = new Point(0, 30);
            containerPanel.Controls.Add(lblServer);

            Label lblUser = new Label();
            lblUser.Text = "Username";
            lblUser.AutoSize = true;
            lblUser.Location = new Point(0, 65);
            containerPanel.Controls.Add(lblUser);

            TextBox txtUser = new TextBox();
            txtUser.Width = 200;
            txtUser.Location = new Point(90, 65);
            containerPanel.Controls.Add(txtUser);

            Label lblPass = new Label();
            lblPass.Text = "Password";
            lblPass.AutoSize = true;
            lblPass.Location = new Point(0, 95);
            containerPanel.Controls.Add(lblPass);

            TextBox txtPass = new TextBox();
            txtPass.Width = 200;
            txtPass.PasswordChar = '*';
            txtPass.Location = new Point(90, 95);
            containerPanel.Controls.Add(txtPass);

            Button btnLogin = new Button();
            btnLogin.Text = "Đăng nhập";
            btnLogin.Size = new Size(95, 40);
            btnLogin.Location = new Point(0, 140);
            containerPanel.Controls.Add(btnLogin);

            Button btnRegister = new Button();
            btnRegister.Text = "Đăng ký";
            btnRegister.Size = new Size(95, 40);
            btnRegister.Location = new Point(105, 140);
            containerPanel.Controls.Add(btnRegister);

            this.Controls.Add(containerPanel);

            // Center container on form resize
            this.Resize += (s, e) =>
            {
                containerPanel.Left = (this.ClientSize.Width - containerPanel.Width) / 2;
                containerPanel.Top = (this.ClientSize.Height - containerPanel.Height) / 2;
            };

            btnLogin.Click += async (s, e) =>
            {
                string username = txtUser.Text.Trim();
                string password = txtPass.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Vui lòng nhập username và password.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnLogin.Enabled = false;
                try
                {
                    var request = new LoginPacket
                    {
                        PacketType = "LOGIN",
                        Username = username,
                        Password = password
                    };

                    LoginPacket? response = await AppSession.SendAndWaitAsync<LoginPacket>(request, "LOGIN_RESULT");
                    if (response == null)
                    {
                        MessageBox.Show("Server không trả về dữ liệu đăng nhập.", "Lỗi Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!response.Success)
                    {
                        MessageBox.Show(response.Message, "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    AppSession.CurrentUsername = response.Username;
                    MessageBox.Show(response.Message, "Đăng nhập thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    MainMenuForm menu = new MainMenuForm();
                    menu.FormClosed += (sender, args) =>
                    {
                        this.Close();
                    };
                    menu.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không kết nối được Server. Hãy chạy Server trước sau đó mở Client.\n\nChi tiết: " + ex.Message,
                        "Lỗi kết nối Server Thất Bại!!!",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    btnLogin.Enabled = true;
                }
            };

            btnRegister.Click += (s, e) =>
            {
                using RegisterForm register = new RegisterForm();
                register.ShowDialog();
            };

            AcceptButton = btnLogin;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Center container on initial show
            if (this.Controls.Count > 0)
            {
                Panel containerPanel = (Panel)this.Controls[0];
                containerPanel.Left = (this.ClientSize.Width - containerPanel.Width) / 2;
                containerPanel.Top = (this.ClientSize.Height - containerPanel.Height) / 2;
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
        }
    }
}
