using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class MainMenuForm : Form
    {
        public MainMenuForm()
        {
            Text = "Main Menu";
            Size = new Size(500, 500);
            StartPosition = FormStartPosition.CenterScreen;

            Button btnSingle = new Button();
            btnSingle.Text = "Chơi đơn";
            btnSingle.Location = new Point(150, 80);
            btnSingle.Size = new Size(180, 40);

            btnSingle.Click += (s, e) =>
            {
                SinglePlayerForm form = new SinglePlayerForm();
                form.ShowDialog();
            };

            Button btnOnline = new Button();
            btnOnline.Text = "Chơi online";
            btnOnline.Location = new Point(150, 140);
            btnOnline.Size = new Size(180, 40);

            btnOnline.Click += (s, e) =>
            {
                LobbyForm form = new LobbyForm();
                form.ShowDialog();
            };

            Button btnProfile = new Button();
            btnProfile.Text = "Hồ sơ";
            btnProfile.Location = new Point(150, 200);
            btnProfile.Size = new Size(180, 40);

            btnProfile.Click += (s, e) =>
            {
                ProfileForm form = new ProfileForm();
                form.ShowDialog();
            };

            Button btnRanking = new Button();
            btnRanking.Text = "Bảng xếp hạng";
            btnRanking.Location = new Point(150, 260);
            btnRanking.Size = new Size(180, 40);

            btnRanking.Click += (s, e) =>
            {
                RankingForm form = new RankingForm();
                form.ShowDialog();
            };

            Button btnHistory = new Button();
            btnHistory.Text = "Lịch sử đấu";
            btnHistory.Location = new Point(150, 320);
            btnHistory.Size = new Size(180, 40);

            btnHistory.Click += (s, e) =>
            {
                MatchHistoryForm form = new MatchHistoryForm();
                form.ShowDialog();
            };

            Controls.Add(btnSingle);
            Controls.Add(btnOnline);
            Controls.Add(btnProfile);
            Controls.Add(btnRanking);
            Controls.Add(btnHistory);
        }
    }
}