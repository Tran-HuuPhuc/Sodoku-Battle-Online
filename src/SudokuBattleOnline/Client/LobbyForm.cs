using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class LobbyForm : Form
    {
        public LobbyForm()
        {
            Text = "Lobby";
            Size = new Size(500, 400);

            ListBox lstRooms = new ListBox();
            lstRooms.Location = new Point(50, 50);
            lstRooms.Size = new Size(250, 200);

            lstRooms.Items.Add("Room 1");
            lstRooms.Items.Add("Room 2");
            lstRooms.Items.Add("Room 3");

            Button btnCreate = new Button();
            btnCreate.Text = "Tạo phòng";
            btnCreate.Location = new Point(330, 80);

            Button btnJoin = new Button();
            btnJoin.Text = "Tham gia";
            btnJoin.Location = new Point(330, 140);

            Controls.Add(lstRooms);
            Controls.Add(btnCreate);
            Controls.Add(btnJoin);
        }
    }
}