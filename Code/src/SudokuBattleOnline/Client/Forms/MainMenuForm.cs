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
            // Panel Left (Menu)
            menuPanel = new Panel();
            menuPanel.Dock = DockStyle.Left;
            menuPanel.Width = 280;
            menuPanel.BackColor = Color.FromArgb(41, 53, 65); // Dark blue-gray

            // Panel Right (Content)
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.FromArgb(236, 240, 241); // Soft light gray
            
            try
            {
                // Try to load background image from Resources
                string bgPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Resources\bg.png"));
                if (System.IO.File.Exists(bgPath))
                {
                    contentPanel.BackgroundImage = Image.FromFile(bgPath);
                    contentPanel.BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
            catch { /* Ignore if not found */ }

            // Game Title
            Label lblTitle = new Label();
            lblTitle.Text = "SUDOKU BATTLE";
            lblTitle.Font = new Font("Segoe UI Black", 20F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.White;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 100;
            menuPanel.Controls.Add(lblTitle);

            // Single Mode
            Button btnSinglePlayer = new Button { Text = "Single-Player" };
            StyleMenuButton(btnSinglePlayer, 100);
            btnSinglePlayer.Click += (s, e) => { ShowFormInPanel(new SinglePlayerForm()); };

            // Online Mode
            Button btnOnline = new Button { Text = "Online Mode" };
            StyleMenuButton(btnOnline, 160);
            btnOnline.Click += async (s, e) => 
            { 
                if (!SudokuBattleOnline.Client.AppSession.IsLoggedIn)
                {
                    MessageBox.Show("Bạn cần đăng nhập để chơi chế độ Online.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                btnOnline.Text = "Đang tìm trận...";
                btnOnline.Enabled = false;

                // Send FindMatchPacket
                await SudokuBattleOnline.Client.AppSession.SendPacketAsync(new SudokuBattleOnline.Shared.Packets.FindMatchPacket());

                Action<SudokuBattleOnline.Shared.Packets.BasePacket, string> onGameStart = null;
                onGameStart = (basePacket, rawJson) =>
                {
                    if (basePacket.PacketType == "GAME_START")
                    {
                        var startPacket = System.Text.Json.JsonSerializer.Deserialize<SudokuBattleOnline.Shared.Packets.GameStartPacket>(rawJson);
                        if (startPacket != null)
                        {
                            SudokuBattleOnline.Client.AppSession.PacketReceived -= onGameStart;
                            this.Invoke((MethodInvoker)delegate
                            {
                                btnOnline.Text = "Online Mode";
                                btnOnline.Enabled = true;
                                ShowFormInPanel(new MultiplayerGameForm(startPacket));
                            });
                        }
                    }
                };

                SudokuBattleOnline.Client.AppSession.PacketReceived += onGameStart;
            };

            // Your Profile
            Button btnProfile = new Button { Text = "Your Profile" };
            StyleMenuButton(btnProfile, 220);
            btnProfile.Click += (s, e) => { ShowFormInPanel(new ProfileForm()); };

            // Ranking
            Button btnRanking = new Button { Text = "Ranking" };
            StyleMenuButton(btnRanking, 280);
            btnRanking.Click += (s, e) => { ShowFormInPanel(new RankingForm()); };

            // Match History
            Button btnMatchHistory = new Button { Text = "Match History" };
            StyleMenuButton(btnMatchHistory, 340);
            btnMatchHistory.Click += (s, e) => { ShowFormInPanel(new MatchHistoryForm()); };

            // Best Score
            Button btnBestScore = new Button { Text = "Best Score" };
            StyleMenuButton(btnBestScore, 400);
            btnBestScore.Click += (s, e) => { ShowFormInPanel(new BestScoreForm()); };

            menuPanel.Controls.Add(btnBestScore);
            menuPanel.Controls.Add(btnMatchHistory);
            menuPanel.Controls.Add(btnRanking);
            menuPanel.Controls.Add(btnProfile);
            menuPanel.Controls.Add(btnOnline);
            menuPanel.Controls.Add(btnSinglePlayer);
            
            Controls.Add(contentPanel);
            Controls.Add(menuPanel);
        }

        private void StyleMenuButton(Button btn, int yPos)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 152, 219); // Blue on hover
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(41, 128, 185); // Darker blue on click
            btn.BackColor = Color.FromArgb(41, 53, 65); // Transparent/Match panel
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            btn.Cursor = Cursors.Hand;
            btn.Location = new Point(0, yPos);
            btn.Size = new Size(280, 60);
            btn.TextAlign = ContentAlignment.MiddleCenter;
        }
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