using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class MultiplayerGameForm : Form
    {
        private Panel board;
        private TextBox[,] cells = new TextBox[9, 9];
        private System.Windows.Forms.Timer gameTimer = null!;
        private Label lblTimer = null!;
        private Label lblPlayer = null!;
        private Label lblOpponent = null!;
        
        private int remainingSeconds;
        private GameStartPacket _gameData;
        private bool isUpdatingFromServer = false;

        public MultiplayerGameForm(GameStartPacket startPacket)
        {
            _gameData = startPacket;
            Text = "Multiplayer Game";
            Size = new Size(800, 600);
            
            lblPlayer = new Label();
            lblPlayer.Text = $"Bạn ({AppSession.CurrentUsername}): 0%";
            lblPlayer.Location = new Point(30, 20);
            lblPlayer.AutoSize = true;
            lblPlayer.Font = new Font("Arial", 11, FontStyle.Bold);

            lblOpponent = new Label();
            lblOpponent.Text = $"Đối thủ ({_gameData.OpponentUsername}): 0%";
            lblOpponent.Location = new Point(250, 20);
            lblOpponent.AutoSize = true;
            lblOpponent.Font = new Font("Arial", 11, FontStyle.Bold);

            lblTimer = new Label();
            remainingSeconds = _gameData.TimeLimitSeconds;
            int m = remainingSeconds / 60;
            int s = remainingSeconds % 60;
            lblTimer.Text = $"Còn lại: {m:D2}:{s:D2}";
            lblTimer.Location = new Point(500, 20);
            lblTimer.AutoSize = true;
            lblTimer.Font = new Font("Arial", 12, FontStyle.Bold);

            board = new Panel();
            board.Location = new Point(30, 60);
            board.Size = new Size(390, 390);
            board.BorderStyle = BorderStyle.None;
            board.Paint += (s, e) =>
            {
                using Pen pen = new Pen(Color.Black, 6);
                e.Graphics.DrawRectangle(pen, 0, 0, board.Width - 1, board.Height - 1);
            };

            TextBox txtChat = new TextBox();
            txtChat.Location = new Point(450, 60);
            txtChat.Size = new Size(300, 390);
            txtChat.Multiline = true;
            txtChat.ReadOnly = true;
            txtChat.Text = "Hệ thống: Trận đấu bắt đầu!\r\n";

            Controls.Add(lblPlayer);
            Controls.Add(lblOpponent);
            Controls.Add(lblTimer);
            Controls.Add(board);
            Controls.Add(txtChat);

            CreateBoard();
            LoadBoardData(_gameData.Board);

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();
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

                    int offset = 11;
                    int x = offset + c * size + (c / 3) * 4;
                    int y = offset + r * size + (r / 3) * 4;
                    txt.Location = new Point(x, y);
                    txt.Size = new Size(size, size);

                    int captureR = r;
                    int captureC = c;

                    txt.KeyPress += (s, e) =>
                    {
                        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
                        if (e.KeyChar == '0') e.Handled = true;
                    };

                    txt.TextChanged += async (s, e) =>
                    {
                        if (txt.ReadOnly || isUpdatingFromServer) return;
                        if (string.IsNullOrEmpty(txt.Text))
                        {
                            txt.ForeColor = Color.Black;
                            return;
                        }
                        if (int.TryParse(txt.Text, out int value))
                        {
                            txt.ForeColor = Color.Blue;
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
            isUpdatingFromServer = false;
        }

        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (remainingSeconds > 0)
            {
                remainingSeconds--;
                lblTimer.Text = $"Còn lại: {remainingSeconds / 60:D2}:{remainingSeconds % 60:D2}";
                if (remainingSeconds <= 60) lblTimer.ForeColor = Color.Red;
            }
            else
            {
                gameTimer.Stop();
                for (int r = 0; r < 9; r++)
                    for (int c = 0; c < 9; c++)
                        cells[r, c].ReadOnly = true;
                MessageBox.Show("Hết giờ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}