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

            Label lblUser = new Label();
            lblUser.Text = "Username";
            lblUser.Location = new Point(50, 100);

            TextBox txtUser = new TextBox();
            txtUser.Location = new Point(180, 100);
            txtUser.Width = 200;

            Label lblPass = new Label();
            lblPass.Text = "Password";
            lblPass.Location = new Point(50, 150);

            TextBox txtPass = new TextBox();
            txtPass.Location = new Point(180, 150);
            txtPass.Width = 200;
            txtPass.PasswordChar = '*';

            Label lblConfirm = new Label();
            lblConfirm.Text = "Confirm";
            lblConfirm.Location = new Point(50, 200);

            TextBox txtConfirm = new TextBox();
            txtConfirm.Location = new Point(180, 200);
            txtConfirm.Width = 200;
            txtConfirm.PasswordChar = '*';

            Button btnRegister = new Button();
            btnRegister.Text = "Đăng ký";
            btnRegister.Location = new Point(120, 290);

            Button btnBack = new Button();
            btnBack.Text = "Quay lại";
            btnBack.Location = new Point(250, 290);
            btnBack.Click += (s, e) =>
{
            this.Close();
};

            Controls.Add(lblTitle);
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