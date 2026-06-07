using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class ProfileForm : Form
    {
        public ProfileForm()
        {
            Text = "Profile";
            Size = new Size(400, 300);

            Controls.Add(new Label()
            {
                Text = "Tên: Player01",
                Location = new Point(50, 50),
                AutoSize = true
            });

            Controls.Add(new Label()
            {
                Text = "ELO: 1000",
                Location = new Point(50, 100),
                AutoSize = true
            });

            Controls.Add(new Label()
            {
                Text = "Thắng: 10",
                Location = new Point(50, 150),
                AutoSize = true
            });

            Controls.Add(new Label()
            {
                Text = "Thua: 5",
                Location = new Point(50, 200),
                AutoSize = true
            });
        }
    }
}