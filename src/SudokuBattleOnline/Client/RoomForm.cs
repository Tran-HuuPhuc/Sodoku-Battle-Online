using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class RoomForm : Form
    {
        public RoomForm()
        {
            Text = "Room";
            Size = new Size(600, 450);

            Label lblRoom = new Label();
            lblRoom.Text = "Room 1";
            lblRoom.Location = new Point(30, 20);
            lblRoom.AutoSize = true;

            ListBox lstPlayers = new ListBox();
            lstPlayers.Location = new Point(30, 60);
            lstPlayers.Size = new Size(150, 200);

            lstPlayers.Items.Add("Player01");
            lstPlayers.Items.Add("Player02");

            TextBox txtChat = new TextBox();
            txtChat.Location = new Point(220, 60);
            txtChat.Size = new Size(300, 200);
            txtChat.Multiline = true;

            Button btnStart = new Button();
            btnStart.Text = "Start";
            btnStart.Location = new Point(220, 300);

            Button btnLeave = new Button();
            btnLeave.Text = "Leave";
            btnLeave.Location = new Point(340, 300);

            Controls.Add(lblRoom);
            Controls.Add(lstPlayers);
            Controls.Add(txtChat);
            Controls.Add(btnStart);
            Controls.Add(btnLeave);
        }
    }
}