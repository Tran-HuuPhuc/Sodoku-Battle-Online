using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class RankingForm : Form
    {
        public RankingForm()
        {
            Text = "Bảng Xếp Hạng";
            Size = new Size(700, 400);

            DataGridView dgv = new DataGridView();

            dgv.Dock = DockStyle.Fill;

            dgv.Columns.Add("Rank", "Hạng");
            dgv.Columns.Add("User", "Tên");
            dgv.Columns.Add("Elo", "ELO");

            dgv.Rows.Add("1", "Player01", "1500");
            dgv.Rows.Add("2", "Player02", "1450");
            dgv.Rows.Add("3", "Player03", "1400");

            Controls.Add(dgv);
        }
    }
}