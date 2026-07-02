using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared.Enums;
using SudokuBattleOnline.Client.Game;

namespace SudokuBattleOnline.Forms
{
    public class SinglePlayerForm : Form
    {
        private Panel board = null!;
        private readonly TextBox[,] cells = new TextBox[9, 9];

        private DateTime startedAt = DateTime.Now;
        private Difficulty currentDifficulty = Difficulty.Medium;
        private System.Windows.Forms.Timer gameTimer = null!;
        private int remainingSeconds;
        private int totalLimitSeconds;
        private bool gameStarted;
        private bool gameFinished;
        private bool suppressCellEvents;

        private const int MaxWrongAttempts = 5;
        private int wrongAttempts;

        private readonly SudokuGenerator sudokuGenerator = new();
        private int[,] currentPuzzle = new int[9, 9];
        private int[,] currentSolution = new int[9, 9];

        private Label lblTimer = null!;
        private Label lblDiffLabel = null!;
        private Label lblWrongLabel = null!;
        private Label lblStatus = null!;
        private ComboBox cmbDifficulty = null!;
        private ProgressBar pbTime = null!;

        public SinglePlayerForm()
        {
            UITheme.ApplyFormTheme(this);
            Text = "Sudoku - Chế độ chơi một mình";
            Size = new Size(920, 620);
            MinimumSize = new Size(880, 580);

            InitializeUI();
            LoadNewPuzzleByDifficulty();
        }

        private void InitializeUI()
        {
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = UITheme.BgDeep,
                Padding = new Padding(16, 0, 16, 0)
            };

            var lblTitle = new Label
            {
                Text = "Chế Độ Chơi Đơn",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = UITheme.Accent,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Width = 300
            };

            lblTimer = new Label
            {
                Text = "--:--",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = UITheme.Success,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false,
                Width = 160
            };

            headerPanel.Controls.Add(lblTimer);
            headerPanel.Controls.Add(lblTitle);

            var bodyPanel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.BgMain };

            board = new Panel
            {
                Location = new Point(28, 20),
                Size = new Size(434, 434),
                BackColor = UITheme.BgMain
            };
            board.Paint += DrawBoardLines;

            pbTime = new ProgressBar
            {
                Location = new Point(28, board.Bottom + 10),
                Size = new Size(434, 8),
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                Style = ProgressBarStyle.Continuous
            };

            var rightPanel = new Panel
            {
                Location = new Point(board.Right + 20, 20),
                Size = new Size(380, 540),
                BackColor = UITheme.BgCard
            };

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
                currentDifficulty = GetSelectedDifficulty();
                lblStatus.Text = "Đã đổi độ khó. Bấm Ván Mới để tạo bảng mới.";
                lblStatus.ForeColor = UITheme.Warning;
            };

            var btnNew = UITheme.MakeButton("Ván Mới", 340, 46);
            btnNew.Location = new Point(20, 100);
            btnNew.Click += (s, e) => LoadNewPuzzleByDifficulty();

            var btnSolve = UITheme.MakeOutlineButton("Hiện Đáp Án", 340, 46);
            btnSolve.Location = new Point(20, 162);
            btnSolve.Click += BtnSolve_Click;

            var sep = UITheme.MakeSeparator(340);
            sep.Location = new Point(20, 224);

            var lblRuleTitle = new Label
            {
                Text = "LUẬT CHƠI",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                Location = new Point(20, 240),
                AutoSize = true
            };

            lblWrongLabel = new Label
            {
                Text = "Sai: 0/5",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = UITheme.Success,
                Location = new Point(20, 265),
                AutoSize = true
            };

            var sep2 = UITheme.MakeSeparator(340);
            sep2.Location = new Point(20, 304);

            var lblStatsTitle = new Label
            {
                Text = "THỐNG KÊ",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                Location = new Point(20, 320),
                AutoSize = true
            };

            lblDiffLabel = new Label
            {
                Text = "Độ khó: Trung bình",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, 342),
                AutoSize = true
            };

            lblStatus = new Label
            {
                Text = "Bấm Ván Mới để bắt đầu.",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, 372),
                Size = new Size(340, 58)
            };

            var lblHint = new Label
            {
                Text = "Lưu ý:\n- Sai 5 lần: thua trận.",
                Font = new Font("Segoe UI", 9),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, 446),
                Size = new Size(340, 90)
            };

            rightPanel.Controls.AddRange(new Control[]
            {
                lblDiff, cmbDifficulty, btnNew, btnSolve, sep,
                lblRuleTitle, lblWrongLabel, sep2,
                lblStatsTitle, lblDiffLabel, lblStatus, lblHint
            });

            bodyPanel.Controls.Add(board);
            bodyPanel.Controls.Add(pbTime);
            bodyPanel.Controls.Add(rightPanel);

            Controls.Add(bodyPanel);
            Controls.Add(headerPanel);

            gameTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            gameTimer.Tick += GameTimer_Tick;

            CreateBoard();
        }

        private Difficulty GetSelectedDifficulty()
        {
            return cmbDifficulty.SelectedIndex switch
            {
                0 => Difficulty.Easy,
                1 => Difficulty.Medium,
                2 => Difficulty.Hard,
                _ => Difficulty.Medium
            };
        }

        private void DrawBoardLines(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int cellSize = 44;
            int offset = 8;

            using var thinPen = new Pen(Color.FromArgb(55, 65, 95), 1);
            using var thickPen = new Pen(Color.FromArgb(100, 180, 255), 3);
            using var borderPen = new Pen(UITheme.Accent, 4);

            for (int i = 0; i <= 9; i++)
            {
                int pos = offset + i * cellSize + (i / 3) * 4;
                var pen = (i % 3 == 0) ? thickPen : thinPen;
                g.DrawLine(pen, offset, pos, offset + 9 * cellSize + 8, pos);
                g.DrawLine(pen, pos, offset, pos, offset + 9 * cellSize + 8);
            }

            g.DrawRectangle(borderPen, offset - 2, offset - 2, 9 * cellSize + 12, 9 * cellSize + 12);
        }

        private void CreateBoard()
        {
            int size = 44;
            int offset = 8;

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
                        ForeColor = UITheme.TextPrimary,
                        Tag = new Point(r, c)
                    };

                    txt.Location = new Point(offset + c * size + (c / 3) * 4 + 2,
                                             offset + r * size + (r / 3) * 4 + 2);
                    txt.Size = new Size(size - 2, size - 2);

                    txt.KeyPress += Cell_KeyPress;
                    txt.TextChanged += Cell_TextChanged;
                    txt.GotFocus += (s, e) => HighlightGroup(r, c, true);
                    txt.LostFocus += (s, e) => HighlightGroup(r, c, false);

                    cells[r, c] = txt;
                    board.Controls.Add(txt);
                }
            }
        }

        private void Cell_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (gameFinished)
            {
                e.Handled = true;
                return;
            }

            if (char.IsControl(e.KeyChar)) return;

            if (e.KeyChar < '1' || e.KeyChar > '9')
                e.Handled = true;
        }

        private async void Cell_TextChanged(object? sender, EventArgs e)
        {
            if (suppressCellEvents || gameFinished) return;
            if (sender is not TextBox txt) return;
            if (txt.ReadOnly) return;
            if (txt.Tag is not Point p) return;

            int row = p.X;
            int col = p.Y;

            if (string.IsNullOrWhiteSpace(txt.Text))
            {
                txt.ForeColor = UITheme.TextPrimary;
                txt.BackColor = UITheme.BgElevated;
                return;
            }

            if (!gameStarted)
            {
                gameStarted = true;
                startedAt = DateTime.Now;
                gameTimer.Start();
            }

            if (!int.TryParse(txt.Text, out int value)) return;

            if (value == currentSolution[row, col])
            {
                txt.ForeColor = UITheme.Success;
                txt.BackColor = Color.FromArgb(15, 55, 35);
                txt.ReadOnly = true;
                txt.TabStop = false;
                lblStatus.Text = "Chính xác. Tiếp tục hoàn thành các ô còn lại.";
                lblStatus.ForeColor = UITheme.Success;

                if (IsBoardCorrectAndFull())
                    await FinishSinglePlayerGameAsync(true, "Hoàn thành bảng Sudoku.");
            }
            else
            {
                wrongAttempts++;
                UpdateWrongLabel();
                txt.ForeColor = UITheme.Danger;
                txt.BackColor = Color.FromArgb(70, 20, 25);

                lblStatus.Text = wrongAttempts >= MaxWrongAttempts
                    ? "Bạn đã sai 5 lần. Ván chơi kết thúc."
                    : $"Sai đáp án. Còn {MaxWrongAttempts - wrongAttempts} lần sai.";
                lblStatus.ForeColor = UITheme.Danger;

                suppressCellEvents = true;
                txt.Text = string.Empty;
                suppressCellEvents = false;

                if (wrongAttempts >= MaxWrongAttempts)
                    await FinishSinglePlayerGameAsync(false, "Bạn nhập sai 5 lần nên thua trận.");
            }
        }

        private void HighlightGroup(int row, int col, bool on)
        {
            if (gameFinished) return;

            Color highlight = on ? Color.FromArgb(30, 50, 80) : UITheme.BgElevated;
            Color givenHighlight = on ? Color.FromArgb(25, 30, 48) : UITheme.BgDeep;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    bool same = r == row || c == col || (r / 3 == row / 3 && c / 3 == col / 3);
                    if (!same || cells[r, c] == null) continue;

                    if (cells[r, c].ReadOnly)
                    {
                        if (cells[r, c].ForeColor == UITheme.Success)
                            cells[r, c].BackColor = on ? Color.FromArgb(20, 70, 45) : Color.FromArgb(15, 55, 35);
                        else
                            cells[r, c].BackColor = on ? givenHighlight : UITheme.BgDeep;
                    }
                    else
                    {
                        cells[r, c].BackColor = on ? highlight : UITheme.BgElevated;
                    }
                }
            }
        }

        private void LoadNewPuzzleByDifficulty()
        {
            currentDifficulty = GetSelectedDifficulty();
            startedAt = DateTime.Now;
            wrongAttempts = 0;
            gameStarted = false;
            gameFinished = false;

            gameTimer.Stop();

            var generated = sudokuGenerator.GeneratePuzzleWithSolution(currentDifficulty);
            if (generated.Puzzle == null || generated.Solution == null)
            {
                MessageBox.Show("Lỗi tạo bảng Sudoku. Hãy thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentPuzzle = generated.Puzzle;
            currentSolution = generated.Solution;
            LoadPuzzleToBoard(currentPuzzle);

            totalLimitSeconds = currentDifficulty.GetTimeLimitSeconds();
            remainingSeconds = totalLimitSeconds;
            lblTimer.Text = FormatTime(remainingSeconds);
            lblTimer.ForeColor = UITheme.Success;
            pbTime.Value = 100;

            lblDiffLabel.Text = $"Độ khó: {currentDifficulty.ToVietnamese()}  |  Giới hạn: {totalLimitSeconds / 60} phút";
            UpdateWrongLabel();

            lblStatus.Text = "Bảng mới đã được tạo. Nhập số để bắt đầu tính giờ.";
            lblStatus.ForeColor = UITheme.Success;
        }

        private void LoadPuzzleToBoard(int[,] puzzle)
        {
            suppressCellEvents = true;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    int value = puzzle[r, c];
                    var cell = cells[r, c];
                    cell.TabStop = true;

                    if (value != 0)
                    {
                        cell.ReadOnly = true;
                        cell.Text = value.ToString();
                        cell.BackColor = UITheme.BgDeep;
                        cell.ForeColor = Color.FromArgb(190, 205, 235);
                    }
                    else
                    {
                        cell.ReadOnly = false;
                        cell.Text = string.Empty;
                        cell.BackColor = UITheme.BgElevated;
                        cell.ForeColor = UITheme.TextPrimary;
                    }
                }
            }

            suppressCellEvents = false;
        }

        private bool IsBoardCorrectAndFull()
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (!int.TryParse(cells[r, c].Text, out int value)) return false;
                    if (value != currentSolution[r, c]) return false;
                }
            }
            return true;
        }

        private async void BtnSolve_Click(object? sender, EventArgs e)
        {
            if (gameFinished) return;

            var confirm = MessageBox.Show(
                "Hiện đáp án sẽ tính là thua trận. Bạn có chắc không?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            suppressCellEvents = true;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (!cells[r, c].ReadOnly)
                    {
                        cells[r, c].Text = currentSolution[r, c].ToString();
                        cells[r, c].ForeColor = UITheme.TextMuted;
                        cells[r, c].BackColor = Color.FromArgb(25, 40, 60);
                    }
                    cells[r, c].ReadOnly = true;
                }
            }
            suppressCellEvents = false;

            await FinishSinglePlayerGameAsync(false, "Người chơi chọn hiện đáp án nên ván đấu tính là thua.");
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (gameFinished) return;

            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                lblTimer.Text = FormatTime(remainingSeconds);

                int pct = totalLimitSeconds > 0 ? remainingSeconds * 100 / totalLimitSeconds : 0;
                pbTime.Value = Math.Max(0, Math.Min(100, pct));

                if (remainingSeconds <= 60) lblTimer.ForeColor = UITheme.Danger;
                else if (remainingSeconds <= 120) lblTimer.ForeColor = UITheme.Warning;
                else lblTimer.ForeColor = UITheme.Success;
            }
            else
            {
                _ = FinishSinglePlayerGameAsync(false, "Hết thời gian làm bài.");
            }
        }

        private async Task FinishSinglePlayerGameAsync(bool isWin, string reason)
        {
            if (gameFinished) return;
            gameFinished = true;
            gameTimer.Stop();
            LockBoard();

            int timeSeconds = Math.Max(1, totalLimitSeconds - remainingSeconds);
            int score = isWin ? CalculateScore(timeSeconds) : 0;
            string result = isWin ? "Win" : "Lose";

            try
            {
                var request = new SaveMatchResultPacket
                {
                    PacketType = "SAVE_MATCH_RESULT",
                    Opponent = "Single Player",
                    Result = result,
                    Difficulty = currentDifficulty.ToString(),
                    Score = score,
                    TimeSeconds = timeSeconds
                };

                SaveMatchResultPacket? response = await AppSession.SendAndWaitAsync<SaveMatchResultPacket>(request, "SAVE_MATCH_RESULT");
                if (response == null || !response.Success)
                {
                    MessageBox.Show(response?.Message ?? "Server không trả về kết quả lưu trận.",
                        "Không thể lưu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu kết quả qua Server: " + ex.Message,
                    "Lỗi Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lblStatus.Text = reason;
            lblStatus.ForeColor = isWin ? UITheme.Success : UITheme.Danger;

            MessageBox.Show(
                isWin
                    ? $"Hoàn thành Sudoku!\nĐộ khó: {currentDifficulty.ToVietnamese()}\nĐiểm: {score}\nThời gian: {FormatPlainTime(timeSeconds)}"
                    : $"Thua trận!\nLý do: {reason}\nThời gian: {FormatPlainTime(timeSeconds)}",
                isWin ? "Chiến thắng" : "Kết thúc ván chơi",
                MessageBoxButtons.OK,
                isWin ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void LockBoard()
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    cells[r, c].ReadOnly = true;
        }

        private int CalculateScore(int timeSeconds)
        {
            int baseScore = currentDifficulty switch
            {
                Difficulty.Easy => 1000,
                Difficulty.Medium => 1500,
                Difficulty.Hard => 2000,
                _ => 1500
            };

            int score = baseScore - timeSeconds * 2 - wrongAttempts * 80;
            return Math.Max(100, score);
        }

        private void UpdateWrongLabel()
        {
            lblWrongLabel.Text = $"Sai: {wrongAttempts}/{MaxWrongAttempts}";
            lblWrongLabel.ForeColor = wrongAttempts switch
            {
                >= 4 => UITheme.Danger,
                >= 3 => UITheme.Warning,
                _ => UITheme.Success
            };
        }

        private static string FormatTime(int totalSeconds)
        {
            int m = totalSeconds / 60;
            int s = totalSeconds % 60;
            return $"{m:D2}:{s:D2}";
        }

        private static string FormatPlainTime(int totalSeconds)
        {
            int m = totalSeconds / 60;
            int s = totalSeconds % 60;
            return $"{m:D2}:{s:D2}";
        }
    }
}
