using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class LoginForm : Form
    {
        public LoginForm()
        {
            Text = "Đăng nhập";
            Size = new Size(500, 350);
            StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label();
            lblTitle.Text = "SUDOKU BATTLE ONLINE";
            lblTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(80, 30);

            Label lblUser = new Label();
            lblUser.Text = "Username";
            lblUser.Location = new Point(60, 100);

            TextBox txtUser = new TextBox();
            txtUser.Location = new Point(170, 100);
            txtUser.Width = 200;

            Label lblPass = new Label();
            lblPass.Text = "Password";
            lblPass.Location = new Point(60, 150);

            TextBox txtPass = new TextBox();
            txtPass.Location = new Point(170, 150);
            txtPass.Width = 200;
            txtPass.PasswordChar = '*';

            Button btnLogin = new Button();
            btnLogin.Text = "Đăng nhập";
            btnLogin.Location = new Point(100, 230);
            btnLogin.Size = new Size(120, 40);

            btnLogin.Click += (s, e) =>
            {
                MainMenuForm menu = new MainMenuForm();
                menu.Show();
                this.Hide();
            };

            Button btnRegister = new Button();
            btnRegister.Text = "Đăng ký";
            btnRegister.Location = new Point(250, 230);
            btnRegister.Size = new Size(120, 40);

            btnRegister.Click += (s, e) =>
            {
                RegisterForm register = new RegisterForm();
                register.ShowDialog();
            };

            Controls.Add(lblTitle);
            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPass);
            Controls.Add(txtPass);
            Controls.Add(btnLogin);
            Controls.Add(btnRegister);
        }
    }
}