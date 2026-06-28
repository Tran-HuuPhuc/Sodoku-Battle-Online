using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;
using Shared.Enums;
using SudokuBattleOnline.Client.Game;
namespace SudokuBattleOnline.Forms
{
    public class SinglePlayerForm : Form
    {
        private Panel board;
        private TextBox[,] cells = new TextBox[9, 9];
        private Random random = new Random();
        private DateTime startedAt = DateTime.Now;
        private Difficulty currentDifficulty = Difficulty.Medium;
        private ComboBox cmbDifficulty = null!;

        private System.Windows.Forms.Timer gameTimer = null!;
        private Label lblTimer = null!;
        private int remainingSeconds;
        private int totalLimitSeconds;
        private bool gameStarted = false;
        private int checkCount = 0;          // tổng check
        private const int MaxChecks = 3;
        private readonly SudokuGenerator sudokuGenerator = new();

        private int[,] currentPuzzle = new int[9, 9];
        private int[,] currentSolution = new int[9, 9];
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
            Text = "Sudoku Battle Online";
            Size = new Size(700, 550);
            StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label();
            lblTitle.Text = "SUDOKU";
            lblTitle.Font = new Font("Arial", 18, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(250, 20);

            Controls.Add(lblTitle);
            Label lblDifficulty = new Label();
            lblDifficulty.Text = "Độ khó:";
            lblDifficulty.Location = new Point(500, 55);
            lblDifficulty.Size = new Size(120, 25);

            cmbDifficulty = new ComboBox();
            cmbDifficulty.Location = new Point(500, 75);
            cmbDifficulty.Size = new Size(120, 30);
            cmbDifficulty.DropDownStyle = ComboBoxStyle.DropDownList;

            cmbDifficulty.Items.Add(Difficulty.Easy.ToVietnamese());
            cmbDifficulty.Items.Add(Difficulty.Medium.ToVietnamese());
            cmbDifficulty.Items.Add(Difficulty.Hard.ToVietnamese());

            cmbDifficulty.SelectedIndex = 1; // mặc định Trung Bình

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

            Controls.Add(lblDifficulty);
            Controls.Add(cmbDifficulty);
            board = new Panel();
            board.Location = new Point(20, 70);
            board.Size = new Size(390, 390);
            board.BorderStyle = BorderStyle.None;

            board.Paint += (s, e) =>
            {
            using Pen pen = new Pen(Color.Black, 6);
            e.Graphics.DrawRectangle(
            pen,
            0,
            0,
            board.Width - 1,
            board.Height - 1
            );
            };

            Controls.Add(board);

            CreateBoard();

            // Khởi tạo Nhãn đếm ngược
            lblTimer = new Label();
            lblTimer.Text = "Còn lại: 00:00";
            lblTimer.Font = new Font("Arial", 12, FontStyle.Bold);
            lblTimer.Location = new Point(500, 370);
            lblTimer.AutoSize = true;
            Controls.Add(lblTimer);

            // Khởi tạo Timer
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;

            Button btnNew = new Button();
            btnNew.Text = "New";
            btnNew.Location = new Point(500, 100);
            btnNew.Size = new Size(120, 40);

            btnNew.Click += (s, e) =>
            {
                LoadNewPuzzleByDifficulty();
            };

            Button btnCheck = new Button();
            btnCheck.Text = $"Check ({MaxChecks - checkCount} lần)";
            btnCheck.Location = new Point(500, 170);
            btnCheck.Size = new Size(120, 40);

            btnCheck.Click += async (s, e) =>
            {
                if (checkCount >= MaxChecks)
                {
                    MessageBox.Show("Bạn đã hết lượt Check!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                checkCount++;
                int remaining = MaxChecks - checkCount;
                btnCheck.Text = remaining > 0 ? $"Check ({remaining} lần)" : "Check (hết)";
                if (remaining == 0) btnCheck.Enabled = false;

                gameTimer.Stop(); 

                int emptyCells = 0;
                int wrongCells = 0;

                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (string.IsNullOrWhiteSpace(cells[r, c].Text))
                        {
                            emptyCells++;
                        }
                        else if (int.TryParse(cells[r, c].Text, out int val))
                        {
                            if (val != currentSolution[r, c])
                            {
                                wrongCells++;
                                cells[r, c].ForeColor = Color.Red;
                            }
                        }
                    }
                }

                if (emptyCells > 0 || wrongCells > 0)
                {
                    string msg = $"Bảng Sudoku chưa hoàn thành.\n- Ô trống: {emptyCells}\n- Ô sai: {wrongCells}";
                    MessageBox.Show(msg, "Kết quả kiểm tra", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    gameTimer.Start(); 
                    return; 
                }

                // Người chơi thắng cuộc
                int timeSeconds = Math.Max(1, totalLimitSeconds - remainingSeconds);
                int score = Math.Max(0, 1000 - timeSeconds * 2);
                string result = "Win";

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
                        return;
                    }

                    MessageBox.Show($"Chúc mừng! Bạn đã hoàn thành xuất sắc!\nĐã lưu lịch sử chơi vào Server.\nKết quả: {result}\nĐiểm: {score}\nThời gian: {timeSeconds} giây",
                        "Chiến thắng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể lưu lịch sử chơi qua Server: " + ex.Message,
                        "Lỗi Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            Button btnSolve = new Button();
            btnSolve.Text = "Solve";
            btnSolve.Location = new Point(500, 240);
            btnSolve.Size = new Size(120, 40);

            btnSolve.Click += (s, e) =>
            {
                gameTimer.Stop(); // Dừng bộ đếm thời gian khi giải tự động
                if (currentSolution != null && currentSolution[0, 0] != 0)
                {
                    for (int r = 0; r < 9; r++)
                    {
                        for (int c = 0; c < 9; c++)
                        {
                            if (!cells[r, c].ReadOnly)
                            {
                                cells[r, c].Text = currentSolution[r, c].ToString();
                                cells[r, c].ForeColor = Color.Blue;
                            }
                        }
                    }
                    MessageBox.Show("Đã giải xong bảng Sudoku!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            Button btnBack = new Button();
            btnBack.Text = "Quay lại";
            btnBack.Location = new Point(500, 310);
            btnBack.Size = new Size(120, 40);

            btnBack.Click += (s, e) =>
            {
                Close();
            };

            
            Controls.Add(btnNew);
            Controls.Add(btnCheck);
            Controls.Add(btnSolve);
            Controls.Add(btnBack);

            //LoadRandomPuzzle();
            LoadNewPuzzleByDifficulty();
        }
        private void LoadNewPuzzleByDifficulty()
        {
            if (cmbDifficulty == null)
            {
                MessageBox.Show("ComboBox độ khó chưa được khởi tạo.");
                return;
            }

            if (cells[0, 0] == null)
            {
                MessageBox.Show("Bảng Sudoku chưa được tạo. Kiểm tra lại CreateBoard().");
                return;
            }

            currentDifficulty = cmbDifficulty.SelectedIndex switch
            {
                0 => Difficulty.Easy,
                1 => Difficulty.Medium,
                2 => Difficulty.Hard,
                _ => Difficulty.Medium
            };

            startedAt = DateTime.Now;

            var generated = sudokuGenerator.GeneratePuzzleWithSolution(currentDifficulty);

            if (generated.Puzzle == null || generated.Solution == null)
            {
                MessageBox.Show("SudokuGenerator trả về dữ liệu rỗng. Kiểm tra GeneratePuzzleWithSolution().");
                return;
            }

            currentPuzzle = generated.Puzzle;
            currentSolution = generated.Solution;

            LoadPuzzleToBoard(currentPuzzle);

            // Cấu hình bộ đếm thời gian
            totalLimitSeconds = currentDifficulty.GetTimeLimitSeconds();
            remainingSeconds = totalLimitSeconds;
            lblTimer.Text = $"Còn lại: {remainingSeconds / 60:D2}:{remainingSeconds % 60:D2}";
            lblTimer.ForeColor = Color.Black;
            gameStarted = false;
            gameTimer.Stop();
        }
        private void LoadPuzzleToBoard(int[,] puzzle)
        {
            if (puzzle == null)
            {
                MessageBox.Show("Puzzle bị null.");
                return;
            }

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (cells[r, c] == null)
                    {
                        MessageBox.Show($"Ô cells[{r},{c}] chưa được tạo.");
                        return;
                    }

                    int value = puzzle[r, c];

                    if (value != 0)
                    {
                        cells[r, c].ReadOnly = true;
                        cells[r, c].Text = value.ToString();
                        cells[r, c].BackColor = Color.LightGray;
                        cells[r, c].ForeColor = Color.Black;
                    }
                    else
                    {
                        cells[r, c].ReadOnly = false;
                        cells[r, c].Text = "";
                        cells[r, c].BackColor = Color.White;
                        cells[r, c].ForeColor = Color.Blue;
                    }
                }
            }
        }
        private void CreateBoard()
        {
            int size = 40;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox txt = new TextBox();

                    txt.BorderStyle = BorderStyle.FixedSingle;
                    txt.Font = new Font("Arial", 16, FontStyle.Bold);
                    txt.TextAlign = HorizontalAlignment.Center;
                    txt.MaxLength = 1;

                    int boardRealSize = 368;
                    int offset = 11;

                    int x = offset + c * size + (c / 3) * 4;
                    int y = offset + r * size + (r / 3) * 4;
                    txt.Location = new Point(x, y);
                    txt.Size = new Size(size, size);

                    int captureR = r;
                    int captureC = c;

                    txt.KeyPress += (s, e) =>
                    {
                        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                        {
                            e.Handled = true;
                        }
                        if (e.KeyChar == '0')
                        {
                            e.Handled = true;
                        }
                    };

                    txt.TextChanged += (s, e) =>
                    {
                        if (txt.ReadOnly) return;

                        if (string.IsNullOrEmpty(txt.Text))
                        {
                            txt.ForeColor = Color.Black;
                            txt.BackColor = Color.White;
                            // Tái kiểm tra các ô liên quan khi xóa
                            RevalidateRelated(captureR, captureC);
                            return;
                        }

                        if (!gameStarted)
                        {
                            gameStarted = true;
                            gameTimer.Start();
                        }

                        if (int.TryParse(txt.Text, out int value))
                        {
                            // Kiểm tra xung đột: trùng hàng / cột / ô 3x3
                            bool conflict = HasConflict(captureR, captureC, value);
                            if (conflict)
                            {
                                txt.ForeColor = Color.DarkRed;
                                txt.BackColor = Color.FromArgb(255, 220, 220);
                            }
                            else
                            {
                                txt.ForeColor = Color.Black;
                                txt.BackColor = Color.White;
                            }
                            // Tái kiểm tra các ô liên quan (vì thay đổi có thể giải xung đột)
                            RevalidateRelated(captureR, captureC);
                        }
                    };

                    cells[r, c] = txt;
                    board.Controls.Add(txt);
                }
            }
        }

        private void LoadRandomPuzzle()
        {
            startedAt = DateTime.Now;
            int index = random.Next(puzzles.Length);
            string[,] puzzle = puzzles[index];

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (puzzle[r, c] != "")
                    {
                        cells[r, c].ReadOnly = true;
                        cells[r, c].Text = puzzle[r, c];
                        cells[r, c].BackColor = Color.LightGray;
                        cells[r, c].ForeColor = Color.Black;
                    }
                    else
                    {
                        cells[r, c].ReadOnly = false;
                        cells[r, c].Text = "";
                        cells[r, c].BackColor = Color.White;
                        cells[r, c].ForeColor = Color.Blue;
                    }
                }
            }
        }

        private int CountEmptyCells()
        {
            int count = 0;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (string.IsNullOrWhiteSpace(cells[r, c].Text))
                        count++;
                }
            }

            return count;
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                int minutes = remainingSeconds / 60;
                int seconds = remainingSeconds % 60;
                lblTimer.Text = $"Còn lại: {minutes:D2}:{seconds:D2}";

                if (remainingSeconds <= 60)
                {
                    lblTimer.ForeColor = Color.Red;
                }
                else
                {
                    lblTimer.ForeColor = Color.Black;
                }
            }
            else
            {
                gameTimer.Stop();

                // Khóa bảng
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        cells[r, c].ReadOnly = true;
                    }
                }

                MessageBox.Show("Hết giờ! Bạn đã thất bại trong việc giải bảng Sudoku.", "Kết thúc game", MessageBoxButtons.OK, MessageBoxIcon.Stop);

                // Lưu kết quả thua lên Server
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

                SaveMatchResultPacket? response = await AppSession.SendAndWaitAsync<SaveMatchResultPacket>(request, "SAVE_MATCH_RESULT");
                if (response != null && response.Success)
                {
                    Console.WriteLine("[DB LOG] Kết quả hết giờ đã được lưu.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DB LOG ERROR] Không thể lưu kết quả hết giờ: " + ex.Message);
            }
        }

        /// <summary>
        /// Kiểm tra xem số <paramref name="value"/> có bị trùng lặp
        /// trong cùng hàng, cột, hoặc ô 3×3 với ô (row, col) không.
        /// Bỏ qua chính ô đó khi so sánh.
        /// </summary>
        private bool HasConflict(int row, int col, int value)
        {
            for (int i = 0; i < 9; i++)
            {
                // Cùng hàng (bỏ qua chính ô)
                if (i != col && int.TryParse(cells[row, i].Text, out int rv) && rv == value)
                    return true;

                // Cùng cột (bỏ qua chính ô)
                if (i != row && int.TryParse(cells[i, col].Text, out int cv) && cv == value)
                    return true;
            }

            // Cùng ô 3×3
            int startRow = row / 3 * 3;
            int startCol = col / 3 * 3;
            for (int r = startRow; r < startRow + 3; r++)
            {
                for (int c = startCol; c < startCol + 3; c++)
                {
                    if (r == row && c == col) continue;
                    if (int.TryParse(cells[r, c].Text, out int bv) && bv == value)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sau khi ô (row, col) thay đổi, tái kiểm tra màu của tất cả
        /// các ô liên quan (cùng hàng, cột, ô 3×3) để cập nhật đúng.
        /// </summary>
        private void RevalidateRelated(int row, int col)
        {
            var toCheck = new System.Collections.Generic.HashSet<(int, int)>();

            for (int i = 0; i < 9; i++)
            {
                toCheck.Add((row, i));   // Cùng hàng
                toCheck.Add((i, col));   // Cùng cột
            }

            int startRow = row / 3 * 3;
            int startCol = col / 3 * 3;
            for (int r = startRow; r < startRow + 3; r++)
                for (int c = startCol; c < startCol + 3; c++)
                    toCheck.Add((r, c));

            foreach (var (r, c) in toCheck)
            {
                if (r == row && c == col) continue;
                var cell = cells[r, c];
                if (cell.ReadOnly || string.IsNullOrEmpty(cell.Text)) continue;
                if (!int.TryParse(cell.Text, out int v)) continue;

                bool conflict = HasConflict(r, c, v);
                cell.ForeColor = conflict ? Color.DarkRed : Color.Black;
                cell.BackColor = conflict ? Color.FromArgb(255, 220, 220) : Color.White;
            }
        }
    }
}
