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
            Size = new Size(500, 380);
            StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label();
            lblTitle.Text = "SUDOKU BATTLE ONLINE";
            lblTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(80, 30);

            Label lblServer = new Label();
            lblServer.Text = $"Server: {AppSession.ServerIp}:{AppSession.ServerPort}";
            lblServer.Location = new Point(170, 70);
            lblServer.AutoSize = true;

            Label lblUser = new Label();
            lblUser.Text = "Username";
            lblUser.Location = new Point(60, 115);

            TextBox txtUser = new TextBox();
            txtUser.Location = new Point(170, 115);
            txtUser.Width = 200;

            Label lblPass = new Label();
            lblPass.Text = "Password";
            lblPass.Location = new Point(60, 165);

            TextBox txtPass = new TextBox();
            txtPass.Location = new Point(170, 165);
            txtPass.Width = 200;
            txtPass.PasswordChar = '*';

            Button btnLogin = new Button();
            btnLogin.Text = "Đăng nhập";
            btnLogin.Location = new Point(100, 250);
            btnLogin.Size = new Size(120, 40);

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

            Button btnRegister = new Button();
            btnRegister.Text = "Đăng ký";
            btnRegister.Location = new Point(250, 250);
            btnRegister.Size = new Size(120, 40);

            btnRegister.Click += (s, e) =>
            {
                using RegisterForm register = new RegisterForm();
                register.ShowDialog();
            };

            AcceptButton = btnLogin;

            Controls.Add(lblTitle);
            Controls.Add(lblServer);
            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPass);
            Controls.Add(txtPass);
            Controls.Add(btnLogin);
            Controls.Add(btnRegister);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
        }
    }
}
