using SudokuBattleOnline.Client;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public partial class MainMenuForm : Form
    {
        public static MainMenuForm? Instance { get; private set; }

        private Panel sidebarPanel = null!;
        private Panel contentPanel = null!;
        private Form? currentChildForm;

        // Sidebar items
        private Button? _activeBtn;
        private Label lblUserInfo = null!;

        public MainMenuForm()
        {
            Instance = this;
            Text = "Sudoku Battle Online";
            Size = new Size(1180, 720);
            MinimumSize = new Size(1000, 620);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UITheme.BgMain;

            CreateLayout();
        }

        private void CreateLayout()
        {
            // ── Sidebar ──────────────────────────────────────────
            sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = UITheme.BgDeep
            };

            // Logo / brand area
            var brandPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = UITheme.BgDeep
            };

            var lblLogo = new Label
            {
                Text = "▦",
                Font = new Font("Segoe UI Symbol", 34, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                Location = new Point(22, 18),
                Size = new Size(50, 54),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblBrandTop = new Label
            {
                Text = "SUDOKU",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Location = new Point(90, 25),
                Size = new Size(135, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblBrandBottom = new Label
            {
                Text = "BATTLE",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Location = new Point(90, 50),
                Size = new Size(135, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblTagline = new Label
            {
                Text = "Online · Real-time",
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = UITheme.TextMuted,
                Location = new Point(90, 80),
                Size = new Size(130, 18),
                TextAlign = ContentAlignment.MiddleLeft
            };

            brandPanel.Controls.AddRange(new Control[]
            {
                lblLogo,
                lblBrandTop,
                lblBrandBottom,
                lblTagline
            });

            //var lblTagline = new Label
            //{
            //    Text = "Online · Real-time",
            //    Font = new Font("Segoe UI", 8, FontStyle.Regular),
            //    ForeColor = UITheme.TextMuted,
            //    Location = new Point(88, 76),
            //    Size = new Size(130, 18),
            //    TextAlign = ContentAlignment.MiddleLeft
            //};

            //brandPanel.Controls.AddRange(new Control[] { lblLogo, lblBrand, lblTagline });

            // Divider
            var sep1 = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = UITheme.Border, Top = 120 };

            // ── Nav Buttons ───────────────────────────────────────
            var navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 12, 0, 0),
                BackColor = UITheme.BgDeep
            };

            var btnSingle   = MakeNavButton("  Chơi Đơn",   "SinglePlayer");
            var btnOnline   = MakeNavButton("  Chơi Online",      "Online");
            var btnRooms    = MakeNavButton("  Phòng Tùy Chỉnh", "CustomRooms");
            var btnProfile  = MakeNavButton("  Hồ Sơ",            "Profile");
            var btnRanking  = MakeNavButton("  Bảng Xếp Hạng",   "Ranking");
            var btnHistory  = MakeNavButton("  Lịch Sử Đấu",     "History");
            var btnBest     = MakeNavButton("  Thành Tích",        "BestScore");

            btnSingle.Click  += (s, e) => { SetActiveNav(btnSingle);  ShowFormInPanel(new SinglePlayerForm()); };
            btnRooms.Click   += (s, e) => { SetActiveNav(btnRooms);   ShowFormInPanel(new LobbyForm()); };
            btnProfile.Click += (s, e) => { SetActiveNav(btnProfile); ShowFormInPanel(new ProfileForm()); };
            btnRanking.Click += (s, e) => { SetActiveNav(btnRanking); ShowFormInPanel(new RankingForm()); };
            btnHistory.Click += (s, e) => { SetActiveNav(btnHistory); ShowFormInPanel(new MatchHistoryForm()); };
            btnBest.Click    += (s, e) => { SetActiveNav(btnBest);    ShowFormInPanel(new BestScoreForm()); };

            btnOnline.Click += async (s, e) =>
            {
                if (!SudokuBattleOnline.Client.AppSession.IsLoggedIn)
                {
                    MessageBox.Show("Bạn cần đăng nhập để chơi chế độ Online.", "Thông báo",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                SetActiveNav(btnOnline);
                await ShowMatchmakingOverlayAsync();
            };

            navFlow.Controls.AddRange(new Control[]
            { btnSingle, btnOnline, btnRooms, btnProfile, btnRanking, btnHistory, btnBest });

            // ── User info footer ──────────────────────────────────
            var footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = Color.FromArgb(10, 13, 22),
                Padding = new Padding(14, 12, 0, 0)
            };

            lblUserInfo = new Label
            {
                Text = SudokuBattleOnline.Client.AppSession.IsLoggedIn
                    ? $"User: {SudokuBattleOnline.Client.AppSession.CurrentUsername}"
                    : "User: Chưa đăng nhập",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                Location = new Point(14, 8),
                AutoSize = true
            };

            var lblVersion = new Label
            {
                Text = "v1.0  |  © 2025 SudokuBattle",
                Font = new Font("Segoe UI", 7),
                ForeColor = UITheme.TextMuted,
                Location = new Point(14, 32),
                AutoSize = true
            };

            footerPanel.Controls.AddRange(new Control[] { lblUserInfo, lblVersion });

            sidebarPanel.Controls.Add(footerPanel);
            sidebarPanel.Controls.Add(navFlow);
            sidebarPanel.Controls.Add(sep1);
            sidebarPanel.Controls.Add(brandPanel);

            // ── Content panel ─────────────────────────────────────
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.BgMain
            };

            // Welcome screen
            ShowWelcomeScreen();

            Controls.Add(contentPanel);
            Controls.Add(sidebarPanel);
        }

        // ─────────────────────────────────────────────────────────
        // HELPER: Tạo nút sidebar
        // ─────────────────────────────────────────────────────────
        private Button MakeNavButton(string text, string tag)
        {
            var btn = new Button
            {
                Text = text,
                Tag = tag,
                Size = new Size(240, 52),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(26, 0, 0, 0),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 2)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = UITheme.BgHover;
            btn.FlatAppearance.MouseDownBackColor = UITheme.AccentGlow;
            return btn;
        }

        private void SetActiveNav(Button btn)
        {
            if (_activeBtn != null)
            {
                _activeBtn.ForeColor = UITheme.TextSecondary;
                _activeBtn.BackColor = Color.Transparent;
                _activeBtn.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            }
            btn.ForeColor = UITheme.Accent;
            btn.BackColor = UITheme.AccentGlow;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            _activeBtn = btn;
        }

        // ─────────────────────────────────────────────────────────
        // Welcome screen
        // ─────────────────────────────────────────────────────────
        private void ShowWelcomeScreen()
        {
            contentPanel.Controls.Clear();

            var container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.BgMain
            };

            var centerPanel = new Panel
            {
                Size = new Size(860, 430),
                BackColor = UITheme.BgMain
            };

            var lblWelcome = new Label
            {
                Text = "Sudoku Battle Online",
                Font = new Font("Segoe UI", 30, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                Location = new Point(0, 30),
                Size = new Size(860, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            string name = SudokuBattleOnline.Client.AppSession.CurrentUsername ?? "Bạn";

            var lblGreet = new Label
            {
                Text = $"Chào mừng, {name}! Chọn chế độ chơi từ thanh menu bên trái.",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(0, 95),
                Size = new Size(860, 34),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            var cardsPanel = new Panel
            {
                Location = new Point(0, 185),
                Size = new Size(860, 150),
                BackColor = UITheme.BgMain
            };

            var card1 = MakeInfoCard("1v1", "Real-time", "Đấu trực tuyến");
            card1.Location = new Point(20, 0);

            var card2 = MakeInfoCard("3x", "Multi-Difficulty", "Dễ / Trung / Khó");
            card2.Location = new Point(230, 0);

            var card3 = MakeInfoCard("ELO", "Ranking", "Bảng xếp hạng");
            card3.Location = new Point(440, 0);

            var card4 = MakeInfoCard("SV", "Anti-Cheat", "Server validation");
            card4.Location = new Point(650, 0);

            cardsPanel.Controls.Add(card1);
            cardsPanel.Controls.Add(card2);
            cardsPanel.Controls.Add(card3);
            cardsPanel.Controls.Add(card4);

            var lblHint = new Label
            {
                Text = "Hệ thống hỗ trợ chơi đơn, chơi online, lưu lịch sử đấu và bảng xếp hạng qua Server.",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = UITheme.TextMuted,
                Location = new Point(0, 360),
                Size = new Size(860, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            centerPanel.Controls.Add(lblWelcome);
            centerPanel.Controls.Add(lblGreet);
            centerPanel.Controls.Add(cardsPanel);
            centerPanel.Controls.Add(lblHint);

            container.Controls.Add(centerPanel);

            container.Layout += (s, e) =>
            {
                int x = Math.Max(0, (container.Width - centerPanel.Width) / 2);
                int y = Math.Max(40, (container.Height - centerPanel.Height) / 2 - 15);

                centerPanel.Location = new Point(x, y);
            };

            contentPanel.Controls.Add(container);
        }
        private Panel MakeInfoCard(string icon, string title, string subtitle)
        {
            var card = new Panel
            {
                Size = new Size(190, 130),
                BackColor = UITheme.BgCard,
                Margin = new Padding(0)
            };

            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using var pen = new Pen(UITheme.Border, 1);
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                e.Graphics.DrawRectangle(pen, rect);
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                Location = new Point(18, 15),
                Size = new Size(150, 42),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Location = new Point(20, 66),
                Size = new Size(150, 24),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            var lblSub = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, 92),
                Size = new Size(150, 22),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            card.Controls.Add(lblIcon);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblSub);

            return card;
        }
        // ─────────────────────────────────────────────────────────
        // Matchmaking overlay
        // ─────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task ShowMatchmakingOverlayAsync()
        {
            var overlay = new Panel
            {
                Size = new Size(400, 220),
                BackColor = UITheme.BgCard,
                BorderStyle = BorderStyle.None
            };
            overlay.Location = new Point(
                (contentPanel.Width - overlay.Width) / 2,
                (contentPanel.Height - overlay.Height) / 2);

            var lblTitle = new Label
            {
                Text = "Tìm đối thủ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                Location = new Point(24, 24),
                AutoSize = true
            };

            var lblStatus = new Label
            {
                Text = "Đang tìm đối thủ phù hợp...",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(24, 68),
                AutoSize = true
            };

            var sep = new Panel
            {
                Location = new Point(24, 100),
                Size = new Size(352, 1),
                BackColor = UITheme.Border
            };

            var btnCancel = UITheme.MakeDangerButton("Hủy tìm trận", 352, 46);
            btnCancel.Location = new Point(24, 150);

            overlay.Controls.AddRange(new Control[] { lblTitle, lblStatus, sep, btnCancel });
            contentPanel.Controls.Add(overlay);
            overlay.BringToFront();

            await SudokuBattleOnline.Client.AppSession.SendPacketAsync(
                new SudokuBattleOnline.Shared.Packets.FindMatchPacket());

            bool searching = true;
            Action<SudokuBattleOnline.Shared.Packets.BasePacket, string> onGameStart = null!;
            onGameStart = (basePacket, rawJson) =>
            {
                if (basePacket.PacketType == "GAME_START" && searching)
                {
                    var startPacket = System.Text.Json.JsonSerializer.Deserialize<SudokuBattleOnline.Shared.Packets.GameStartPacket>(rawJson);
                    if (startPacket != null)
                    {
                        SudokuBattleOnline.Client.AppSession.PacketReceived -= onGameStart;
                        this.Invoke((MethodInvoker)delegate
                        {
                            contentPanel.Controls.Remove(overlay);
                            overlay.Dispose();
                            ShowFormInPanel(new MultiplayerGameForm(startPacket));
                        });
                    }
                }
            };

            btnCancel.Click += (cs, ce) =>
            {
                searching = false;
                SudokuBattleOnline.Client.AppSession.PacketReceived -= onGameStart;
                contentPanel.Controls.Remove(overlay);
                overlay.Dispose();
            };

            SudokuBattleOnline.Client.AppSession.PacketReceived += onGameStart;
        }

        // ─────────────────────────────────────────────────────────
        // Show child form in content panel
        // ─────────────────────────────────────────────────────────
        public void ShowFormInPanel(Form childForm)
        {
            currentChildForm?.Close();
            currentChildForm = childForm;

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(childForm);

            childForm.BringToFront();
            childForm.Show();

            // Update user info
            lblUserInfo.Text = SudokuBattleOnline.Client.AppSession.IsLoggedIn
                ? $"User: {SudokuBattleOnline.Client.AppSession.CurrentUsername}"
                : "User: Chưa đăng nhập";
        }
    }
}