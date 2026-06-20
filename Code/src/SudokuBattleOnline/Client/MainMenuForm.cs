using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public partial class MainMenuForm : Form
    {
        private Panel menuPanel;
        private Panel contentPanel;
        private Form currentChildForm;
        public MainMenuForm()
        {
            Text = "Sudoku Battle Online";
            Size = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;

            CreateLayout();
        }

        private void CreateLayout()
        {
            menuPanel = new Panel();
            menuPanel.Dock = DockStyle.Left;
            menuPanel.Width = 400;

            menuPanel.BackColor = Color.WhiteSmoke;

            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.Blue;

            Button btnSinglePlayer = new Button();
            btnSinglePlayer.Text = "Single-Player Mode";
            btnSinglePlayer.Location = new Point(150, 80);
            btnSinglePlayer.Size = new Size(180, 40);

            btnSinglePlayer.Click += (s, e) =>
            {
                //SinglePlayerForm form = new SinglePlayerForm();
                //form.ShowDialog();

                ShowFormInPanel(new SinglePlayerForm());

            };

            Button btnOnline = new Button();
            btnOnline.Text = "Online Mode";
            btnOnline.Location = new Point(150, 140);
            btnOnline.Size = new Size(180, 40);


            btnOnline.Click += (s, e) =>
            {
                ShowFormInPanel(new MultiplayerGameForm());
            };

            Button btnProfile = new Button();
            btnProfile.Text = "Your Profile";
            btnProfile.Location = new Point(150, 200);
            btnProfile.Size = new Size(180, 40);

            btnProfile.Click += (s, e) =>
            {
                ShowFormInPanel(new ProfileForm());
            };

            Button btnRanking = new Button();
            btnRanking.Text = "Ranking";
            btnRanking.Location = new Point(150, 260);
            btnRanking.Size = new Size(180, 40);

            btnRanking.Click += (s, e) =>
            {
                ShowFormInPanel(new RankingForm());
            };

            Button btnMatchHistory = new Button();
            btnMatchHistory.Text = "Match History";
            btnMatchHistory.Location = new Point(150, 320);
            btnMatchHistory.Size = new Size(180, 40);
            btnMatchHistory.Click += (s, e) =>
            {
                ShowFormInPanel(new MatchHistoryForm());
            };

            menuPanel.Controls.Add(btnProfile);
            menuPanel.Controls.Add(btnRanking);
            menuPanel.Controls.Add(btnMatchHistory);
            menuPanel.Controls.Add(btnOnline);
            menuPanel.Controls.Add(btnSinglePlayer);

            Controls.Add(contentPanel);
            Controls.Add(menuPanel);
        }

        //private void ShowFormInPanel(Form form)
        //{
        //    contentPanel.Controls.Clear();

        //    form.TopLevel = false;
        //    form.FormBorderStyle = FormBorderStyle.None;
        //    form.Dock = DockStyle.Fill;

        //    contentPanel.Controls.Add(form);
        //    form.Show();
        //}
        private void ShowFormInPanel(Form childForm)
        {
            if (currentChildForm != null)
            {
                currentChildForm.Close();
            }

            currentChildForm = childForm;

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(childForm);

            childForm.BringToFront();
            childForm.Show();
        }
    }
}