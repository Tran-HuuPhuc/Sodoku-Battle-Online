using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class RegisterForm : Form
    {
        public RegisterForm()
        {
            Text = "Đăng ký";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label();
            lblTitle.Text = "ĐĂNG KÝ TÀI KHOẢN";
            lblTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(110, 30);

            Label lblServer = new Label();
            lblServer.Text = $"Lưu tài khoản qua Server: {AppSession.ServerIp}:{AppSession.ServerPort}";
            lblServer.Location = new Point(120, 70);
            lblServer.AutoSize = true;

            Label lblUser = new Label();
            lblUser.Text = "Username";
            lblUser.Location = new Point(50, 110);

            TextBox txtUser = new TextBox();
            txtUser.Location = new Point(180, 110);
            txtUser.Width = 200;

            Label lblPass = new Label();
            lblPass.Text = "Password";
            lblPass.Location = new Point(50, 160);

            TextBox txtPass = new TextBox();
            txtPass.Location = new Point(180, 160);
            txtPass.Width = 200;
            txtPass.PasswordChar = '*';

            Label lblConfirm = new Label();
            lblConfirm.Text = "Confirm";
            lblConfirm.Location = new Point(50, 210);

            TextBox txtConfirm = new TextBox();
            txtConfirm.Location = new Point(180, 210);
            txtConfirm.Width = 200;
            txtConfirm.PasswordChar = '*';

            Button btnRegister = new Button();
            btnRegister.Text = "Đăng ký";
            btnRegister.Location = new Point(120, 300);
            btnRegister.Size = new Size(100, 35);

            btnRegister.Click += async (s, e) =>
            {
                string username = txtUser.Text.Trim();
                string password = txtPass.Text;
                string confirmPassword = txtConfirm.Text;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Username và Password không được để trống.", "Đăng ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (password != confirmPassword)
                {
                    MessageBox.Show("Mật khẩu xác nhận không khớp.", "Đăng ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnRegister.Enabled = false;
                try
                {
                    var request = new RegisterPacket
                    {
                        PacketType = "REGISTER",
                        Username = username,
                        Password = password,
                        ConfirmPassword = confirmPassword
                    };

                    RegisterPacket? response = await AppSession.SendAndWaitAsync<RegisterPacket>(request, "REGISTER_RESULT");
                    if (response == null)
                    {
                        MessageBox.Show("Server không trả về dữ liệu đăng ký.", "Lỗi Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!response.Success)
                    {
                        MessageBox.Show(response.Message, "Đăng ký thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    MessageBox.Show(response.Message + "\nBạn có thể đăng nhập bằng tài khoản vừa tạo.", "Đăng ký thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Không kết nối được Server. Hãy chạy Server trước rồi đăng ký.\n\nChi tiết: " + ex.Message,
                        "Lỗi kết nối Server",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    btnRegister.Enabled = true;
                }
            };

            Button btnBack = new Button();
            btnBack.Text = "Quay lại";
            btnBack.Location = new Point(250, 300);
            btnBack.Size = new Size(100, 35);
            btnBack.Click += (s, e) => Close();

            AcceptButton = btnRegister;
            CancelButton = btnBack;

            Controls.Add(lblTitle);
            Controls.Add(lblServer);
            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPass);
            Controls.Add(txtPass);
            Controls.Add(lblConfirm);
            Controls.Add(txtConfirm);
            Controls.Add(btnRegister);
            Controls.Add(btnBack);
        }
    }
}
