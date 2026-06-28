using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Shared.Enums;
using SudokuBattleOnline.Client.Game;

namespace SudokuBattleOnline.Forms
{
    public class SinglePlayerForm : Form
    {
        // ── Board ─────────────────────────────────────────────
        private Panel board = null!;
        private TextBox[,] cells = new TextBox[9, 9];

        // ── State ──────────────────────────────────────────────
        private Random random = new();
        private DateTime startedAt = DateTime.Now;
        private Difficulty currentDifficulty = Difficulty.Medium;
        private System.Windows.Forms.Timer gameTimer = null!;
        private int remainingSeconds;
        private int totalLimitSeconds;
        private bool gameStarted = false;
        private int checkCount = 0;
        private const int MaxChecks = 3;
        private readonly SudokuGenerator sudokuGenerator = new();
        private int[,] currentPuzzle = new int[9, 9];
        private int[,] currentSolution = new int[9, 9];

        // ── UI Controls ────────────────────────────────────────
        private Label lblTimer = null!;
        private Label lblDiffLabel = null!;
        private ComboBox cmbDifficulty = null!;
        private Button btnCheck = null!;
        private ProgressBar pbTime = null!;

        // Fallback puzzles
        private string[][,] puzzles =
        {
            new string[,]
            {
                {"5","3","","","7","","","",""},
                {"6","","","1","9","5","","",""},
                {"","9","8","","","","","6",""},
                {"8","","","","6","","","","3"},
                {"4","","","8","","3","","","1"},
                {"7","","","","2","","","","6"},
                {"","6","","","","","2","8",""},
                {"","","","4","1","9","","","5"},
                {"","","","","8","","","7","9"}
            },
            new string[,]
            {
                {"","","3","","2","","6","",""},
                {"9","","","3","","5","","","1"},
                {"","","1","8","","6","4","",""},
                {"","","8","1","","2","9","",""},
                {"7","","","","","","","","8"},
                {"","","6","7","","8","2","",""},
                {"","","2","6","","9","5","",""},
                {"8","","","2","","3","","","9"},
                {"","","5","","1","","3","",""}
            }
        };

        public SinglePlayerForm()
        {
            UITheme.ApplyFormTheme(this);
            Text = "Sudoku – Chế độ Một Mình";
            Size = new Size(920, 620);
            MinimumSize = new Size(880, 580);

            InitializeUI();
            LoadNewPuzzleByDifficulty();
        }

        // ─────────────────────────────────────────────────────────
        // UI INIT
        // ─────────────────────────────────────────────────────────
        private void InitializeUI()
        {
            // ── Header bar ────────────────────────────────────────
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = UITheme.BgDeep,
                Padding = new Padding(16, 0, 16, 0)
            };

            var lblTitle = new Label
            {
                Text = "🎯  Chơi Một Mình",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 260
            };

            lblTimer = new Label
            {
                Text = "⏱  --:--",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = UITheme.Success,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Width = 160
            };

            headerPanel.Controls.Add(lblTimer);
            headerPanel.Controls.Add(lblTitle);

            // ── Main body: board (left) + controls (right) ────────
            var bodyPanel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.BgMain };

            // ── Board Panel ───────────────────────────────────────
            board = new Panel
            {
                Location = new Point(28, 20),
                Size = new Size(434, 434),
                BackColor = UITheme.BgMain
            };
            board.Paint += DrawBoardLines;

            // ── Timer progress bar ────────────────────────────────
            pbTime = new ProgressBar
            {
                Location = new Point(28, board.Bottom + 10),
                Size = new Size(434, 8),
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                Style = ProgressBarStyle.Continuous,
                ForeColor = UITheme.Success
            };

            // ── Right controls panel ──────────────────────────────
            var rightPanel = new Panel
            {
                Location = new Point(board.Right + 20, 20),
                Size = new Size(380, 540),
                BackColor = UITheme.BgCard
            };

            // Difficulty selector
            var lblDiff = new Label
            {
                Text = "ĐỘ KHÓ",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                Location = new Point(20, 20),
                AutoSize = true
            };

            cmbDifficulty = new ComboBox
            {
                Location = new Point(20, 42),
                Size = new Size(340, 34),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                BackColor = UITheme.BgElevated,
                ForeColor = UITheme.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            cmbDifficulty.Items.AddRange(new object[]
            {
                Difficulty.Easy.ToVietnamese(),
                Difficulty.Medium.ToVietnamese(),
                Difficulty.Hard.ToVietnamese()
            });
            cmbDifficulty.SelectedIndex = 1;
            cmbDifficulty.SelectedIndexChanged += (s, e) =>
            {
                currentDifficulty = cmbDifficulty.SelectedIndex switch
                {
                    0 => Difficulty.Easy,
                    1 => Difficulty.Medium,
                    2 => Difficulty.Hard,
                    _ => Difficulty.Medium
                };
            };

            // Buttons
            var btnNew = UITheme.MakeButton("🔄  Ván Mới", 340, 46);
            btnNew.Location = new Point(20, 100);
            btnNew.Click += (s, e) => LoadNewPuzzleByDifficulty();

            btnCheck = UITheme.MakeSuccessButton($"✓  Kiểm tra ({MaxChecks - checkCount} lần)", 340, 46);
            btnCheck.Location = new Point(20, 162);
            btnCheck.Click += BtnCheck_Click;

            var btnSolve = UITheme.MakeOutlineButton("💡  Hiện đáp án", 340, 46);
            btnSolve.Location = new Point(20, 224);
            btnSolve.Click += BtnSolve_Click;

            // Separator
            var sep = UITheme.MakeSeparator(340);
            sep.Location = new Point(20, 292);

            // Stats label
            var lblStatsTitle = new Label
            {
                Text = "THỐNG KÊ",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                Location = new Point(20, 308),
                AutoSize = true
            };

            lblDiffLabel = new Label
            {
                Text = "Độ khó: Trung Bình",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, 330),
                AutoSize = true
            };

            var lblHint = new Label
            {
                Text = "💡  Điền 1–9 vào ô trống\n🔴  Màu đỏ = xung đột\n✓   Check tối đa 3 lần",
                Font = new Font("Segoe UI", 9),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, 360),
                AutoSize = true
            };

            rightPanel.Controls.AddRange(new Control[]
            { lblDiff, cmbDifficulty, btnNew, btnCheck, btnSolve, sep, lblStatsTitle, lblDiffLabel, lblHint });

            bodyPanel.Controls.Add(board);
            bodyPanel.Controls.Add(pbTime);
            bodyPanel.Controls.Add(rightPanel);

            Controls.Add(bodyPanel);
            Controls.Add(headerPanel);

            // Timer
            gameTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            gameTimer.Tick += GameTimer_Tick;

            CreateBoard();
        }

        // ─────────────────────────────────────────────────────────
        // Vẽ đường kẻ bàn cờ dark theme
        // ─────────────────────────────────────────────────────────
        private void DrawBoardLines(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cellSize = 44;
            int offset = 8;

            using var thinPen  = new Pen(Color.FromArgb(55, 65, 95), 1);
            using var thickPen = new Pen(Color.FromArgb(100, 180, 255), 3);
            using var borderPen= new Pen(UITheme.Accent, 4);

            for (int i = 0; i <= 9; i++)
            {
                int pos = offset + i * cellSize + (i / 3) * 4;
                var pen = (i % 3 == 0) ? thickPen : thinPen;
                g.DrawLine(pen, offset, pos, offset + 9 * cellSize + 8, pos);
                g.DrawLine(pen, pos, offset, pos, offset + 9 * cellSize + 8);
            }

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
                        Font = UITheme.FontNumber,
                        TextAlign = HorizontalAlignment.Center,
                        MaxLength = 1,
                        BackColor = UITheme.BgElevated,
                        ForeColor = UITheme.TextPrimary
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
                    };

                    txt.TextChanged += (s, e) =>
                    {
                        if (txt.ReadOnly) return;

                        if (string.IsNullOrEmpty(txt.Text))
                        {
                            txt.ForeColor = UITheme.TextPrimary;
                            txt.BackColor = UITheme.BgElevated;
                            RevalidateRelated(captureR, captureC);
                            return;
                        }

                        if (!gameStarted) { gameStarted = true; gameTimer.Start(); }

                        if (int.TryParse(txt.Text, out int value))
                        {
                            bool conflict = HasConflict(captureR, captureC, value);
                            if (conflict)
                            {
                                txt.ForeColor = UITheme.Danger;
                                txt.BackColor = Color.FromArgb(60, 20, 20);
                            }
                            else
                            {
                                txt.ForeColor = UITheme.Accent;
                                txt.BackColor = UITheme.BgElevated;
                            }
                            RevalidateRelated(captureR, captureC);
                        }
                    };

                    // Highlight row/col on focus
                    txt.GotFocus  += (s, e) => HighlightGroup(captureR, captureC, true);
                    txt.LostFocus += (s, e) => HighlightGroup(captureR, captureC, false);

                    cells[r, c] = txt;
                    board.Controls.Add(txt);
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        // Highlight hàng/cột/ô khi focus
        // ─────────────────────────────────────────────────────────
        private void HighlightGroup(int row, int col, bool on)
        {
            Color highlight = on ? Color.FromArgb(30, 50, 80) : UITheme.BgElevated;
            Color givenHighlight = on ? Color.FromArgb(25, 30, 48) : UITheme.BgDeep;

            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    bool same = r == row || c == col
                        || (r / 3 == row / 3 && c / 3 == col / 3);
                    if (same && cells[r, c] != null)
                    {
                        if (!cells[r, c].ReadOnly)
                            cells[r, c].BackColor = on && same ? highlight : UITheme.BgElevated;
                        else
                            cells[r, c].BackColor = on && same ? givenHighlight : UITheme.BgDeep;
                    }
                }
        }

        // ─────────────────────────────────────────────────────────
        // Load puzzle mới theo độ khó
        // ─────────────────────────────────────────────────────────
        private void LoadNewPuzzleByDifficulty()
        {
            currentDifficulty = cmbDifficulty.SelectedIndex switch
            {
                0 => Difficulty.Easy,
                1 => Difficulty.Medium,
                2 => Difficulty.Hard,
                _ => Difficulty.Medium
            };

            startedAt = DateTime.Now;
            checkCount = 0;
            if (btnCheck != null) btnCheck.Text = $"✓  Kiểm tra ({MaxChecks} lần)";
            if (btnCheck != null) btnCheck.Enabled = true;

            var generated = sudokuGenerator.GeneratePuzzleWithSolution(currentDifficulty);

            if (generated.Puzzle == null || generated.Solution == null)
            {
                MessageBox.Show("Lỗi tạo bảng Sudoku. Thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentPuzzle   = generated.Puzzle;
            currentSolution = generated.Solution;

            LoadPuzzleToBoard(currentPuzzle);

            totalLimitSeconds = currentDifficulty.GetTimeLimitSeconds();
            remainingSeconds = totalLimitSeconds;
            lblTimer.Text = FormatTime(remainingSeconds);
            lblTimer.ForeColor = UITheme.Success;
            if (pbTime != null) pbTime.Value = 100;

            if (lblDiffLabel != null)
                lblDiffLabel.Text = $"Độ khó: {currentDifficulty.ToVietnamese()}  |  Giới hạn: {totalLimitSeconds / 60} phút";

            gameStarted = false;
            gameTimer.Stop();
        }

        private void LoadPuzzleToBoard(int[,] puzzle)
        {
            if (puzzle == null) return;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    int value = puzzle[r, c];
                    if (value != 0)
                    {
                        cells[r, c].ReadOnly = true;
                        cells[r, c].Text = value.ToString();
                        cells[r, c].BackColor = UITheme.BgDeep;
                        cells[r, c].ForeColor = Color.FromArgb(180, 195, 225);
                    }
                    else
                    {
                        cells[r, c].ReadOnly = false;
                        cells[r, c].Text = "";
                        cells[r, c].BackColor = UITheme.BgElevated;
                        cells[r, c].ForeColor = UITheme.Accent;
                    }
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        // Nút Kiểm tra
        // ─────────────────────────────────────────────────────────
        private async void BtnCheck_Click(object? sender, EventArgs e)
        {
            if (checkCount >= MaxChecks)
            {
                MessageBox.Show("Bạn đã hết lượt Check!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            checkCount++;
            int remaining = MaxChecks - checkCount;
            btnCheck.Text = remaining > 0 ? $"✓  Kiểm tra ({remaining} lần)" : "✓  Hết lượt check";
            if (remaining == 0) btnCheck.Enabled = false;

            gameTimer.Stop();

            int emptyCells = 0, wrongCells = 0;
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    if (string.IsNullOrWhiteSpace(cells[r, c].Text)) { emptyCells++; continue; }
                    if (int.TryParse(cells[r, c].Text, out int val) && val != currentSolution[r, c])
                    {
                        wrongCells++;
                        cells[r, c].ForeColor = UITheme.Danger;
                        cells[r, c].BackColor = Color.FromArgb(60, 20, 20);
                    }
                }

            if (emptyCells > 0 || wrongCells > 0)
            {
                MessageBox.Show(
                    $"Bảng chưa hoàn thành.\n  • Ô trống: {emptyCells}\n  • Ô sai:   {wrongCells}",
                    "Kết quả kiểm tra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                gameTimer.Start();
                return;
            }

            // WIN
            int timeSeconds = Math.Max(1, totalLimitSeconds - remainingSeconds);
            int score = Math.Max(0, 1000 - timeSeconds * 2);

            try
            {
                var request = new SaveMatchResultPacket
                {
                    PacketType = "SAVE_MATCH_RESULT",
                    Opponent = "Single Player",
                    Result = "Win",
                    Difficulty = currentDifficulty.ToString(),
                    Score = score,
                    TimeSeconds = timeSeconds
                };

                SaveMatchResultPacket? response = await AppSession.SendAndWaitAsync<SaveMatchResultPacket>(request, "SAVE_MATCH_RESULT");
                if (response == null || !response.Success)
                {
                    MessageBox.Show(response?.Message ?? "Không lưu được kết quả.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    $"🏆  CHÚC MỪNG!\n\nBạn đã hoàn thành bảng Sudoku!\n\n  Độ khó:  {currentDifficulty.ToVietnamese()}\n  Điểm:    {score}\n  Thời gian: {timeSeconds}s",
                    "Chiến Thắng!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi Server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────────────────────────────────────────────
        // Nút Hiện đáp án
        // ─────────────────────────────────────────────────────────
        private void BtnSolve_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show(
                "Hiện đáp án sẽ kết thúc ván chơi. Bạn có chắc không?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            gameTimer.Stop();
            if (currentSolution != null && currentSolution[0, 0] != 0)
            {
                for (int r = 0; r < 9; r++)
                    for (int c = 0; c < 9; c++)
                        if (!cells[r, c].ReadOnly)
                        {
                            cells[r, c].Text = currentSolution[r, c].ToString();
                            cells[r, c].ForeColor = UITheme.TextMuted;
                            cells[r, c].BackColor = Color.FromArgb(25, 40, 60);
                            cells[r, c].ReadOnly = true;
                        }
            }
        }

        // ─────────────────────────────────────────────────────────
        // Timer
        // ─────────────────────────────────────────────────────────
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                lblTimer.Text = FormatTime(remainingSeconds);

                int pct = totalLimitSeconds > 0 ? (remainingSeconds * 100 / totalLimitSeconds) : 0;
                pbTime.Value = Math.Max(0, Math.Min(100, pct));

                if (remainingSeconds <= 60)       lblTimer.ForeColor = UITheme.Danger;
                else if (remainingSeconds <= 120) lblTimer.ForeColor = UITheme.Warning;
                else                              lblTimer.ForeColor = UITheme.Success;
            }
            else
            {
                gameTimer.Stop();
                pbTime.Value = 0;
                for (int r = 0; r < 9; r++)
                    for (int c = 0; c < 9; c++)
                        cells[r, c].ReadOnly = true;
                MessageBox.Show("⏱  Hết giờ! Bạn đã không hoàn thành bảng Sudoku.", "Kết Thúc", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                SaveMatchTimeout();
            }
        }

        private async void SaveMatchTimeout()
        {
            try
            {
                var request = new SaveMatchResultPacket
                {
                    PacketType = "SAVE_MATCH_RESULT",
                    Opponent = "Single Player",
                    Result = "Lose",
                    Difficulty = currentDifficulty.ToString(),
                    Score = 0,
                    TimeSeconds = totalLimitSeconds
                };
                await AppSession.SendAndWaitAsync<SaveMatchResultPacket>(request, "SAVE_MATCH_RESULT");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[TIMEOUT LOG] " + ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────
        // Logic kiểm tra xung đột
        // ─────────────────────────────────────────────────────────
        private bool HasConflict(int row, int col, int value)
        {
            for (int i = 0; i < 9; i++)
            {
                if (i != col && int.TryParse(cells[row, i].Text, out int rv) && rv == value) return true;
                if (i != row && int.TryParse(cells[i, col].Text, out int cv) && cv == value) return true;
            }

            int sR = row / 3 * 3, sC = col / 3 * 3;
            for (int r = sR; r < sR + 3; r++)
                for (int c = sC; c < sC + 3; c++)
                {
                    if (r == row && c == col) continue;
                    if (int.TryParse(cells[r, c].Text, out int bv) && bv == value) return true;
                }

            return false;
        }

        private void RevalidateRelated(int row, int col)
        {
            var toCheck = new System.Collections.Generic.HashSet<(int, int)>();
            for (int i = 0; i < 9; i++) { toCheck.Add((row, i)); toCheck.Add((i, col)); }
            int sR = row / 3 * 3, sC = col / 3 * 3;
            for (int r = sR; r < sR + 3; r++)
                for (int c = sC; c < sC + 3; c++)
                    toCheck.Add((r, c));

            foreach (var (r, c) in toCheck)
            {
                if (r == row && c == col) continue;
                var cell = cells[r, c];
                if (cell.ReadOnly || string.IsNullOrEmpty(cell.Text)) continue;
                if (!int.TryParse(cell.Text, out int v)) continue;
                bool conflict = HasConflict(r, c, v);
                cell.ForeColor = conflict ? UITheme.Danger : UITheme.Accent;
                cell.BackColor = conflict ? Color.FromArgb(60, 20, 20) : UITheme.BgElevated;
            }
        }

        private static string FormatTime(int totalSeconds)
        {
            int m = totalSeconds / 60, s = totalSeconds % 60;
            return $"⏱  {m:D2}:{s:D2}";
        }
    }
}
