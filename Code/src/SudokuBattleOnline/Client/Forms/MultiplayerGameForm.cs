using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class MultiplayerGameForm : Form
    {
        // ─── Board ───────────────────────────────────────────────
        private Panel board;
        private TextBox[,] cells = new TextBox[9, 9];

        // ─── Stats labels ────────────────────────────────────────
        private Label lblTimer = null!;
        private Label lblMyName = null!;
        private Label lblMyStats = null!;
        private Label lblOppName = null!;
        private Label lblOppStats = null!;

        // ─── Progress bars ───────────────────────────────────────
        private ProgressBar pbMyProgress = null!;
        private ProgressBar pbOppProgress = null!;
        private Label lblMyPct = null!;
        private Label lblOppPct = null!;

        // ─── Chat ────────────────────────────────────────────────
        private TextBox txtChat = null!;
        private TextBox txtInputChat = null!;
        private Button btnSendChat = null!;

        // ─── State ───────────────────────────────────────────────
        private System.Windows.Forms.Timer gameTimer = null!;
        private int remainingSeconds;
        private GameStartPacket _gameData;
        private bool isUpdatingFromServer = false;
        private bool isFrozen = false;

        public MultiplayerGameForm(GameStartPacket startPacket)
        {
            _gameData = startPacket;
            InitializeForm();

            AppSession.PacketReceived += AppSession_PacketReceived;

            this.Load += (s, e) => gameTimer.Start();

            this.FormClosed += (s, e) =>
            {
                AppSession.PacketReceived -= AppSession_PacketReceived;
                gameTimer.Stop();
                gameTimer.Dispose();
            };
        }

        private void InitializeForm()
        {
            Text = "Sudoku Battle – Trận Đấu Trực Tuyến";
            Size = new Size(900, 660);
            MinimumSize = new Size(900, 660);
            BackColor = Color.FromArgb(24, 26, 36);
            ForeColor = Color.White;

            // ── Tiêu đề ─────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "⚔  SUDOKU BATTLE",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(30, 14),
                AutoSize = true
            };

            // ── Timer ────────────────────────────────────────────
            remainingSeconds = _gameData.TimeLimitSeconds;
            lblTimer = new Label
            {
                Text = FormatTime(remainingSeconds),
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 213, 115),
                Location = new Point(680, 10),
                AutoSize = true
            };

            // ── Panel bên trái: thống kê Bạn ─────────────────────
            bool amIPlayer1 = _gameData.Player1Username == AppSession.CurrentUsername;
            string myName  = AppSession.CurrentUsername ?? "Bạn";
            string oppName = _gameData.OpponentUsername;

            var panelMyStats  = BuildStatsPanel(30,  450, myName,  true,  out lblMyName,  out lblMyStats,  out pbMyProgress,  out lblMyPct);
            var panelOppStats = BuildStatsPanel(30,  520, oppName, false, out lblOppName, out lblOppStats, out pbOppProgress, out lblOppPct);

            // ── Bàn cờ ───────────────────────────────────────────
            board = new Panel
            {
                Location = new Point(30, 60),
                Size = new Size(420, 420),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(24, 26, 36)
            };
            board.Paint += DrawBoardLines;

            // ── Chat panel ───────────────────────────────────────
            var lblChatTitle = new Label
            {
                Text = "💬  Trò chuyện",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 180, 200),
                Location = new Point(480, 60),
                AutoSize = true
            };

            txtChat = new TextBox
            {
                Location = new Point(480, 85),
                Size = new Size(385, 370),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(35, 38, 52),
                ForeColor = Color.FromArgb(200, 210, 230),
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Hệ thống: Trận đấu bắt đầu!\r\n"
            };

            txtInputChat = new TextBox
            {
                Location = new Point(480, 465),
                Size = new Size(280, 32),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(45, 48, 62),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtInputChat.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnSendChat_Click(this, EventArgs.Empty); }
            };

            btnSendChat = new Button
            {
                Text = "Gửi",
                Location = new Point(768, 465),
                Size = new Size(97, 32),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSendChat.FlatAppearance.BorderSize = 0;
            btnSendChat.Click += BtnSendChat_Click;

            // ── Nút Rời trận ──────────────────────────────────────
            var btnLeave = new Button
            {
                Text = "🚪  Rời trận đấu",
                Location = new Point(480, 510),
                Size = new Size(385, 44),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLeave.FlatAppearance.BorderSize = 0;
            btnLeave.Click += async (sender, e) =>
            {
                var confirm = MessageBox.Show(
                    "Bạn có chắc muốn rời trận đấu?\nĐối thủ sẽ thắng ngay lập tức và ELO của bạn bị trừ.",
                    "Xác nhận rời trận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm == DialogResult.Yes)
                {
                    try { await AppSession.SendPacketAsync(new BasePacket { PacketType = "PLAYER_FORFEIT" }); }
                    catch { /* ignore */ }
                    AppSession.PacketReceived -= AppSession_PacketReceived;
                    Close();
                    MainMenuForm.Instance?.ShowFormInPanel(new LobbyForm());
                }
            };

            // ── Thêm controls ──────────────────────────────────────
            Controls.Add(lblTitle);
            Controls.Add(lblTimer);
            Controls.Add(board);
            Controls.Add(panelMyStats);
            Controls.Add(panelOppStats);
            Controls.Add(lblChatTitle);
            Controls.Add(txtChat);
            Controls.Add(txtInputChat);
            Controls.Add(btnSendChat);
            Controls.Add(btnLeave);

            // ── Tạo ô cờ & nạp đề bài ─────────────────────────────
            CreateBoard();
            LoadBoardData(_gameData.Board);

            // ── Timer đếm ngược ────────────────────────────────────
            gameTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            gameTimer.Tick += GameTimer_Tick;
        }

        // ─────────────────────────────────────────────────────────
        // HELPER: Tạo panel hiển thị thống kê + progress bar
        // ─────────────────────────────────────────────────────────
        private Panel BuildStatsPanel(int x, int y, string playerName, bool isMe,
            out Label nameLabel, out Label statsLabel,
            out ProgressBar progressBar, out Label pctLabel)
        {
            Color accentColor = isMe ? Color.FromArgb(46, 213, 115) : Color.FromArgb(255, 107, 107);

            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(420, 58),
                BackColor = Color.FromArgb(35, 38, 52)
            };

            nameLabel = new Label
            {
                Text = (isMe ? "🟢 " : "🔴 ") + playerName,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(8, 4),
                AutoSize = true
            };

            statsLabel = new Label
            {
                Text = "Lỗi: 0/5",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(170, 180, 200),
                Location = new Point(310, 4),
                AutoSize = true
            };

            progressBar = new ProgressBar
            {
                Location = new Point(8, 24),
                Size = new Size(330, 18),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous,
                ForeColor = accentColor
            };

            pctLabel = new Label
            {
                Text = "0%",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(346, 22),
                AutoSize = true
            };

            panel.Controls.Add(nameLabel);
            panel.Controls.Add(statsLabel);
            panel.Controls.Add(progressBar);
            panel.Controls.Add(pctLabel);

            return panel;
        }

        // ─────────────────────────────────────────────────────────
        // Vẽ đường kẻ bàn cờ
        // ─────────────────────────────────────────────────────────
        private void DrawBoardLines(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cellSize = 44;
            int offset = 8;

            using var thinPen = new Pen(Color.FromArgb(80, 90, 120), 1);
            using var thickPen = new Pen(Color.FromArgb(180, 200, 255), 3);
            using var borderPen = new Pen(Color.FromArgb(100, 200, 255), 4);

            // Vẽ đường mỏng
            for (int i = 0; i <= 9; i++)
            {
                int pos = offset + i * cellSize + (i / 3) * 4;
                var pen = (i % 3 == 0) ? thickPen : thinPen;
                g.DrawLine(pen, offset, pos, offset + 9 * cellSize + 8, pos);
                g.DrawLine(pen, pos, offset, pos, offset + 9 * cellSize + 8);
            }

            // Viền ngoài
            g.DrawRectangle(borderPen, offset - 2, offset - 2, 9 * cellSize + 12, 9 * cellSize + 12);
        }

        // ─────────────────────────────────────────────────────────
        // Tạo các TextBox ô cờ
        // ─────────────────────────────────────────────────────────
        private void CreateBoard()
        {
            int size = 44;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    var txt = new TextBox
                    {
                        BorderStyle = BorderStyle.None,
                        Font = new Font("Segoe UI", 17, FontStyle.Bold),
                        TextAlign = HorizontalAlignment.Center,
                        MaxLength = 1,
                        BackColor = Color.FromArgb(40, 44, 58),
                        ForeColor = Color.FromArgb(220, 230, 255)
                    };

                    int offset = 8;
                    txt.Location = new Point(offset + c * size + (c / 3) * 4 + 2, offset + r * size + (r / 3) * 4 + 2);
                    txt.Size = new Size(size - 2, size - 2);

                    int captureR = r;
                    int captureC = c;

                    txt.KeyPress += (s, e) =>
                    {
                        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
                        if (e.KeyChar == '0') e.Handled = true;
                        // Nếu đang bị đóng băng thì chặn tất cả phím số
                        if (isFrozen && char.IsDigit(e.KeyChar)) e.Handled = true;
                    };

                    txt.TextChanged += async (s, e) =>
                    {
                        if (txt.ReadOnly || isUpdatingFromServer || isFrozen) return;
                        if (string.IsNullOrEmpty(txt.Text)) { txt.ForeColor = Color.FromArgb(220, 230, 255); return; }

                        if (int.TryParse(txt.Text, out int value))
                        {
                            // Anti-cheat: Chỉ gửi số lên Server, KHÔNG tự kiểm tra đúng/sai trên Client
                            var updatePacket = new CellUpdatePacket
                            {
                                Row = captureR,
                                Col = captureC,
                                Value = value
                            };
                            await AppSession.SendPacketAsync(updatePacket);
                        }
                    };

                    cells[r, c] = txt;
                    board.Controls.Add(txt);
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        // Nạp đề bài từ Server vào bàn cờ
        // ─────────────────────────────────────────────────────────
        private void LoadBoardData(int[] flatBoard)
        {
            isUpdatingFromServer = true;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    int val = flatBoard[r * 9 + c];
                    if (val != 0)
                    {
                        cells[r, c].ReadOnly = true;
                        cells[r, c].Text = val.ToString();
                        cells[r, c].BackColor = Color.FromArgb(30, 34, 48);
                        cells[r, c].ForeColor = Color.FromArgb(180, 190, 215);
                    }
                    else
                    {
                        cells[r, c].ReadOnly = false;
                        cells[r, c].Text = "";
                        cells[r, c].BackColor = Color.FromArgb(40, 44, 58);
                        cells[r, c].ForeColor = Color.FromArgb(220, 230, 255);
                    }
                }
            }
            isUpdatingFromServer = false;
        }

        // ─────────────────────────────────────────────────────────
        // Nhận và xử lý gói tin từ Server
        // ─────────────────────────────────────────────────────────
        private void AppSession_PacketReceived(BasePacket basePacket, string rawJson)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                switch (basePacket.PacketType)
                {
                    case "CELL_UPDATE":
                        HandleCellUpdate(rawJson);
                        break;

                    case "ROOM_SYSTEM":
                        var sysMsg = System.Text.Json.JsonSerializer.Deserialize<ChatPacket>(rawJson);
                        if (sysMsg != null)
                            txtChat.AppendText($"[Hệ thống] {sysMsg.Content}\r\n");
                        break;

                    case "CHAT":
                        var chat = System.Text.Json.JsonSerializer.Deserialize<ChatPacket>(rawJson);
                        if (chat != null)
                        {
                            string ts = string.IsNullOrEmpty(chat.Timestamp) ? DateTime.Now.ToString("HH:mm:ss") : chat.Timestamp;
                            txtChat.AppendText($"[{ts}] {chat.Sender}: {chat.Content}\r\n");
                        }
                        break;

                    case "GAME_OVER":
                        HandleGameOver(rawJson);
                        break;
                }
            });
        }

        private void HandleCellUpdate(string rawJson)
        {
            var update = System.Text.Json.JsonSerializer.Deserialize<CellUpdatePacket>(rawJson);
            if (update == null) return;

            int r = update.Row;
            int c = update.Col;

            if (r >= 0 && r < 9 && c >= 0 && c < 9)
            {
                isUpdatingFromServer = true;
                if (update.Username == AppSession.CurrentUsername)
                {
                    if (update.IsCorrect)
                    {
                        cells[r, c].Text = update.Value.ToString();
                        cells[r, c].ForeColor = Color.FromArgb(46, 213, 115);
                        cells[r, c].BackColor = Color.FromArgb(20, 60, 35);
                        cells[r, c].ReadOnly = true;
                    }
                    else
                    {
                        cells[r, c].Text = "";
                        cells[r, c].BackColor = Color.FromArgb(80, 20, 20);
                        // Gọi đóng băng 2 giây
                        _ = FreezePlayerAsync();
                    }
                }
                else // Nước đi của đối thủ
                {
                    if (update.IsCorrect)
                    {
                        cells[r, c].Text = update.Value.ToString();
                        cells[r, c].ForeColor = Color.FromArgb(255, 107, 107);
                        cells[r, c].BackColor = Color.FromArgb(60, 20, 20);
                        cells[r, c].ReadOnly = true;
                    }
                }
                isUpdatingFromServer = false;
            }

            // ── Cập nhật thanh tiến trình ─────────────────────────
            bool isPlayer1 = _gameData.Player1Username == AppSession.CurrentUsername;
            int myProgress  = isPlayer1 ? update.Player1Progress : update.Player2Progress;
            int oppProgress = isPlayer1 ? update.Player2Progress : update.Player1Progress;
            int myMistakes  = isPlayer1 ? update.Player1Mistakes : update.Player2Mistakes;
            int oppMistakes = isPlayer1 ? update.Player2Mistakes : update.Player1Mistakes;

            pbMyProgress.Value  = Math.Max(0, Math.Min(100, myProgress));
            pbOppProgress.Value = Math.Max(0, Math.Min(100, oppProgress));
            lblMyPct.Text  = $"{myProgress}%";
            lblOppPct.Text = $"{oppProgress}%";
            lblMyStats.Text  = $"Lỗi: {myMistakes}/5";
            lblOppStats.Text = $"Lỗi: {oppMistakes}/5";

            // Đổi màu số lỗi khi sắp cạn
            lblMyStats.ForeColor  = myMistakes  >= 4 ? Color.OrangeRed : Color.FromArgb(170, 180, 200);
            lblOppStats.ForeColor = oppMistakes >= 4 ? Color.OrangeRed : Color.FromArgb(170, 180, 200);
        }

        private void HandleGameOver(string rawJson)
        {
            var gameOver = System.Text.Json.JsonSerializer.Deserialize<GameOverPacket>(rawJson);
            if (gameOver == null) return;

            gameTimer.Stop();
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    cells[r, c].ReadOnly = true;

            bool iWon = gameOver.WinnerUsername == AppSession.CurrentUsername;
            bool isDraw = gameOver.WinnerUsername == "Draw" || string.IsNullOrEmpty(gameOver.WinnerUsername);
            string header = isDraw ? "🤝  HÒA!" : (iWon ? "🏆  CHIẾN THẮNG!" : "💀  THẤT BẠI!");
            string color  = isDraw ? "Orange"    : (iWon ? "LimeGreen"          : "Tomato");

            MessageBox.Show(
                $"{header}\n\n{gameOver.Reason}\n\nTiến trình: Bạn {(gameOver.WinnerUsername == _gameData.Player1Username ? gameOver.Player1Progress : gameOver.Player2Progress)}%  vs  Đối thủ {(gameOver.WinnerUsername == _gameData.Player1Username ? gameOver.Player2Progress : gameOver.Player1Progress)}%",
                "Kết thúc trận đấu", MessageBoxButtons.OK, MessageBoxIcon.Information);

            AppSession.PacketReceived -= AppSession_PacketReceived;
            Close();
            MainMenuForm.Instance?.ShowFormInPanel(new LobbyForm());
        }

        // ─────────────────────────────────────────────────────────
        // Gửi chat
        // ─────────────────────────────────────────────────────────
        private async void BtnSendChat_Click(object? sender, EventArgs e)
        {
            string content = txtInputChat.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;
            txtInputChat.Clear();
            try
            {
                await AppSession.SendPacketAsync(new ChatPacket { Content = content });
            }
            catch (Exception ex)
            {
                txtChat.AppendText($"[Lỗi mạng] {ex.Message}\r\n");
            }
        }

        // ─────────────────────────────────────────────────────────
        // Timer đếm ngược 10 phút
        // ─────────────────────────────────────────────────────────
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                lblTimer.Text = FormatTime(remainingSeconds);
                if (remainingSeconds <= 60)
                    lblTimer.ForeColor = Color.OrangeRed;
                else if (remainingSeconds <= 120)
                    lblTimer.ForeColor = Color.Orange;
            }
            else
            {
                gameTimer.Stop();
                for (int r = 0; r < 9; r++)
                    for (int c = 0; c < 9; c++)
                        cells[r, c].ReadOnly = true;
                // Server sẽ gửi GAME_OVER khi hết giờ – Client chỉ khóa bàn cờ
            }
        }

        // ─────────────────────────────────────────────────────────
        // Phạt đóng băng 2 giây khi điền sai
        // ─────────────────────────────────────────────────────────
        private async System.Threading.Tasks.Task FreezePlayerAsync()
        {
            if (isFrozen) return; // Tránh chồng chéo
            isFrozen = true;

            // Overlay bán trong suốt che toàn bàn cờ
            var overlay = new Panel
            {
                Location = board.Location,
                Size = board.Size,
                BackColor = Color.FromArgb(180, 10, 10, 30)
            };

            var lblOverlay = new Label
            {
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 80, 80),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            overlay.Controls.Add(lblOverlay);
            this.Controls.Add(overlay);
            overlay.BringToFront();
            this.ActiveControl = null;

            // Đếm ngược 2 giây
            for (int s = 2; s > 0; s--)
            {
                lblOverlay.Text = $"🔒 ĐÓNG BĂNG\n{s}s";
                await System.Threading.Tasks.Task.Delay(1000);
            }

            this.Controls.Remove(overlay);
            overlay.Dispose();
            isFrozen = false;
        }

        private static string FormatTime(int totalSeconds)
        {
            int m = totalSeconds / 60;
            int s = totalSeconds % 60;
            return $"⏱  {m:D2}:{s:D2}";
        }
    }
}