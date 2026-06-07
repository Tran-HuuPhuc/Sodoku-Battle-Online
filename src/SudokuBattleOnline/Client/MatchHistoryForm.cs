using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class MatchHistoryForm : Form
    {
        public MatchHistoryForm()
        {
            Text = "Lịch Sử Đấu";
            Width = 700;
            Height = 400;

            DataGridView dgv = new DataGridView();

            dgv.Dock = DockStyle.Fill;

            dgv.Columns.Add("Date", "Ngày");
            dgv.Columns.Add("Opponent", "Đối thủ");
            dgv.Columns.Add("Result", "Kết quả");

            dgv.Rows.Add("01/06/2025", "Player01", "Win");
            dgv.Rows.Add("02/06/2025", "Player02", "Lose");

            Controls.Add(dgv);
        }
    }
}