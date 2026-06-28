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
                Text = "⊞",
                Font = new Font("Segoe UI", 36, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                Location = new Point(20, 16),
                AutoSize = true
            };

            var lblBrand = new Label
            {
                Text = "SUDOKU\nBATTLE",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Location = new Point(72, 18),
                AutoSize = true
            };

            var lblTagline = new Label
            {
                Text = "Online  •  Real-time",
                Font = new Font("Segoe UI", 7, FontStyle.Regular),
                ForeColor = UITheme.TextMuted,
                Location = new Point(73, 82),
                AutoSize = true
            };

            brandPanel.Controls.AddRange(new Control[] { lblLogo, lblBrand, lblTagline });

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

            var btnSingle   = MakeNavButton("🎯  Chơi Một Mình",   "SinglePlayer");
            var btnOnline   = MakeNavButton("⚔   Chơi Online",      "Online");
            var btnRooms    = MakeNavButton("🏠  Phòng Tùy Chỉnh", "CustomRooms");
            var btnProfile  = MakeNavButton("👤  Hồ Sơ",            "Profile");
            var btnRanking  = MakeNavButton("🏆  Bảng Xếp Hạng",   "Ranking");
            var btnHistory  = MakeNavButton("📋  Lịch Sử Đấu",     "History");
            var btnBest     = MakeNavButton("⭐  Thành Tích",        "BestScore");

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
                    ? $"👤  {SudokuBattleOnline.Client.AppSession.CurrentUsername}"
                    : "👤  Chưa đăng nhập",
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
                Padding = new Padding(20, 0, 0, 0),
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

            var container = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.BgMain };

            var lblWelcome = new Label
            {
                Text = "⊞  Sudoku Battle Online",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                AutoSize = true
            };

            string name = SudokuBattleOnline.Client.AppSession.CurrentUsername ?? "Bạn";
            var lblGreet = new Label
            {
                Text = $"Chào mừng, {name}! Chọn chế độ chơi từ thanh menu bên trái.",
                Font = new Font("Segoe UI", 12),
                ForeColor = UITheme.TextSecondary,
                AutoSize = true
            };

            // Stats cards
            var cardsPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            cardsPanel.Controls.Add(MakeInfoCard("⚔", "1v1 Real-time", "Đấu trực tuyến"));
            cardsPanel.Controls.Add(MakeInfoCard("🧩", "Multi-Difficulty", "Dễ / Trung / Khó"));
            cardsPanel.Controls.Add(MakeInfoCard("🏆", "ELO Rating", "Bảng xếp hạng"));
            cardsPanel.Controls.Add(MakeInfoCard("🔒", "Anti-Cheat", "Server validation"));

            container.Controls.Add(cardsPanel);
            container.Controls.Add(lblGreet);
            container.Controls.Add(lblWelcome);

            // Auto-layout
            container.Layout += (s, e) =>
            {
                lblWelcome.Location = new Point((container.Width - lblWelcome.Width) / 2, 120);
                lblGreet.Location   = new Point((container.Width - lblGreet.Width)   / 2, lblWelcome.Bottom + 16);
                cardsPanel.Location = new Point((container.Width - cardsPanel.Width) / 2, lblGreet.Bottom + 50);
            };

            contentPanel.Controls.Add(container);
        }

        private Panel MakeInfoCard(string icon, string title, string subtitle)
        {
            var card = new Panel
            {
                Size = new Size(170, 110),
                BackColor = UITheme.BgCard,
                Margin = new Padding(10)
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 24),
                ForeColor = UITheme.Accent,
                Location = new Point(12, 12),
                AutoSize = true
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Location = new Point(12, 52),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 8),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(12, 74),
                AutoSize = true
            };

            card.Controls.AddRange(new Control[] { lblIcon, lblTitle, lblSub });
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
                Text = "⚔  Tìm đối thủ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                Location = new Point(24, 24),
                AutoSize = true
            };

            var lblStatus = new Label
            {
                Text = "⏳  Đang tìm đối thủ phù hợp...",
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

            var btnCancel = UITheme.MakeDangerButton("✕  Hủy tìm trận", 352, 46);
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
                ? $"👤  {SudokuBattleOnline.Client.AppSession.CurrentUsername}"
                : "👤  Chưa đăng nhập";
        }
    }
}