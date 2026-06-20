using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class SinglePlayerForm : Form
    {
        private Panel board;
        private TextBox[,] cells = new TextBox[9, 9];
        private Random random = new Random();
        private DateTime startedAt = DateTime.Now;

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

            board = new Panel();
            board.Location = new Point(20, 70);
            board.Size = new Size(390, 390);
            board.BorderStyle = BorderStyle.FixedSingle;

            CreateBoard();

            Button btnNew = new Button();
            btnNew.Text = "New";
            btnNew.Location = new Point(500, 100);
            btnNew.Size = new Size(120, 40);

            btnNew.Click += (s, e) =>
            {
                LoadRandomPuzzle();
            };

            Button btnCheck = new Button();
            btnCheck.Text = "Check";
            btnCheck.Location = new Point(500, 170);
            btnCheck.Size = new Size(120, 40);

            btnCheck.Click += async (s, e) =>
            {
                int timeSeconds = Math.Max(1, (int)(DateTime.Now - startedAt).TotalSeconds);
                int emptyCells = CountEmptyCells();
                int score = Math.Max(0, 1000 - timeSeconds * 2 - emptyCells * 10);
                string result = emptyCells == 0 ? "Win" : "Completed";

                try
                {
                    var request = new SaveMatchResultPacket
                    {
                        PacketType = "SAVE_MATCH_RESULT",
                        Opponent = "Single Player",
                        Result = result,
                        Difficulty = "Medium",
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

                    MessageBox.Show($"Đã lưu lịch sử chơi vào SQLite phía Server.\nKết quả: {result}\nĐiểm: {score}\nThời gian: {timeSeconds} giây",
                        "Đã lưu", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("Chức năng Solve đang phát triển.");
            };

            Button btnBack = new Button();
            btnBack.Text = "Quay lại";
            btnBack.Location = new Point(500, 310);
            btnBack.Size = new Size(120, 40);

            btnBack.Click += (s, e) =>
            {
                Close();
            };

            Controls.Add(board);
            Controls.Add(btnNew);
            Controls.Add(btnCheck);
            Controls.Add(btnSolve);
            Controls.Add(btnBack);

            LoadRandomPuzzle();
        }

        private void CreateBoard()
        {
            int size = 40;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox txt = new TextBox();

                    txt.Font = new Font("Arial", 16, FontStyle.Bold);
                    txt.TextAlign = HorizontalAlignment.Center;

                    int x = c * size + (c / 3) * 4;
                    int y = r * size + (r / 3) * 4;

                    txt.Location = new Point(x, y);
                    txt.Size = new Size(size, size);

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
                    cells[r, c].Text = puzzle[r, c];

                    if (puzzle[r, c] != "")
                    {
                        cells[r, c].ReadOnly = true;
                        cells[r, c].BackColor = Color.LightGray;
                    }
                    else
                    {
                        cells[r, c].ReadOnly = false;
                        cells[r, c].BackColor = Color.White;
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
    }
}
