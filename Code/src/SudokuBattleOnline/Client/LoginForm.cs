using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public partial class LoginForm : Form
    {
        private Panel panelLogin;

        public LoginForm()
        {
            InitializeComponent();

            Text = "Đăng nhập";
            Size = new Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;

            panelLogin = new Panel();
            panelLogin.Size = new Size(400, 250);

            Label lblTitle = new Label();
            lblTitle.Text = "SUDOKU BATTLE ONLINE";
            lblTitle.Font = new Font("Arial", 16, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(35, 20);

            Label lblUser = new Label();
            lblUser.Text = "Username";
            lblUser.Location = new Point(20, 80);

            TextBox txtUser = new TextBox();
            txtUser.Location = new Point(130, 80);
            txtUser.Width = 200;

            Label lblPass = new Label();
            lblPass.Text = "Password";
            lblPass.Location = new Point(20, 120);

            TextBox txtPass = new TextBox();
            txtPass.Location = new Point(130, 120);
            txtPass.Width = 200;
            txtPass.PasswordChar = '*';

            Button btnLogin = new Button();
            btnLogin.Text = "Đăng nhập";
            btnLogin.Location = new Point(70, 180);
            btnLogin.Size = new Size(100, 35);

            btnLogin.Click += (s, e) =>
            {
                MainMenuForm menu = new MainMenuForm();
                menu.Show();
                this.Hide();
            };

            Button btnRegister = new Button();
            btnRegister.Text = "Đăng ký";
            btnRegister.Location = new Point(220, 180);
            btnRegister.Size = new Size(100, 35);

            btnRegister.Click += (s, e) =>
            {
                RegisterForm register = new RegisterForm();
                register.ShowDialog();
            };

            panelLogin.Controls.Add(lblTitle);
            panelLogin.Controls.Add(lblUser);
            panelLogin.Controls.Add(txtUser);
            panelLogin.Controls.Add(lblPass);
            panelLogin.Controls.Add(txtPass);
            panelLogin.Controls.Add(btnLogin);
            panelLogin.Controls.Add(btnRegister);

            Controls.Add(panelLogin);

            CenterPanel();

            Resize += (s, e) => CenterPanel();
        }

        private void CenterPanel()
        {
            panelLogin.Left = (ClientSize.Width - panelLogin.Width) / 2;
            panelLogin.Top = (ClientSize.Height - panelLogin.Height) / 2;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}