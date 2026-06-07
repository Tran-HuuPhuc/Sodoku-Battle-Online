using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class MultiplayerGameForm : Form
    {
        public MultiplayerGameForm()
        {
            Text = "Multiplayer Game";
            Size = new Size(800, 600);

            Label lblPlayer = new Label();
            lblPlayer.Text = "Bạn: 45%";
            lblPlayer.Location = new Point(30, 20);

            Label lblOpponent = new Label();
            lblOpponent.Text = "Đối thủ: 40%";
            lblOpponent.Location = new Point(150, 20);

            Label lblTimer = new Label();
            lblTimer.Text = "05:00";
            lblTimer.Location = new Point(300, 20);

            Panel board = new Panel();
            board.Location = new Point(30, 60);
            board.Size = new Size(450, 450);
            board.BorderStyle = BorderStyle.FixedSingle;

            TextBox txtChat = new TextBox();
            txtChat.Location = new Point(520, 60);
            txtChat.Size = new Size(220, 300);
            txtChat.Multiline = true;

            Controls.Add(lblPlayer);
            Controls.Add(lblOpponent);
            Controls.Add(lblTimer);
            Controls.Add(board);
            Controls.Add(txtChat);
        }
    }
}