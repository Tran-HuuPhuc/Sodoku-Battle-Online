using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class MatchHistoryForm : Form
    {
        private readonly DataGridView dgv;
        private readonly Label        lblStatus;

        public MatchHistoryForm()
        {
            // ── Form setup ──────────────────────────────
            Text            = "Lịch Sử Trận Đấu";
            Size            = new Size(980, 500);
            MinimumSize     = new Size(980, 500);
            BackColor       = UITheme.BgMain;
            ForeColor       = UITheme.TextPrimary;
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // ── Header ──────────────────────────────────
            var lblIcon = new Label
            {
                Text      = "📋",
                Font      = new Font("Segoe UI Emoji", 26, FontStyle.Regular),
                ForeColor = UITheme.Accent,
                AutoSize  = true,
                Location  = new Point(28, 18),
            };

            var lblTitle = new Label
            {
                Text      = "Lịch Sử Trận Đấu",
                Font      = UITheme.FontTitle,
                ForeColor = UITheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(74, 22),
            };

            // ── Separator ───────────────────────────────
            var sep = UITheme.MakeSeparator(920);
            sep.Location = new Point(28, 70);

            // ── Status label ────────────────────────────
            lblStatus = new Label
            {
                Text      = "⏳ Đang tải lịch sử trận đấu…",
                Font      = UITheme.FontBody,
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(28, 80),
            };

            // ── DataGridView ─────────────────────────────
            dgv = new DataGridView
            {
                Location               = new Point(28, 110),
                Size                   = new Size(924, 330),
                ReadOnly               = true,
                AllowUserToAddRows     = false,
                AllowUserToDeleteRows  = false,
                AutoSizeColumnsMode    = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode          = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect            = false,
            };
            UITheme.ApplyDgvTheme(dgv);

            // Columns
            dgv.Columns.Add("Date",       "Ngày chơi");
            dgv.Columns.Add("Player1",    "Người chơi");
            dgv.Columns.Add("Player2",    "Đối thủ");
            dgv.Columns.Add("Winner",     "Người thắng");
            dgv.Columns.Add("Difficulty", "Độ khó");
            dgv.Columns.Add("Time",       "Thời gian");
            dgv.Columns.Add("Elo",        "ELO đổi");

            // Proportional column widths
            dgv.Columns["Date"].FillWeight       = 18;
            dgv.Columns["Player1"].FillWeight    = 16;
            dgv.Columns["Player2"].FillWeight    = 16;
            dgv.Columns["Winner"].FillWeight     = 16;
            dgv.Columns["Difficulty"].FillWeight = 12;
            dgv.Columns["Time"].FillWeight       = 10;
            dgv.Columns["Elo"].FillWeight        = 12;

            // Row painting – colour Winner cell
            dgv.CellFormatting += Dgv_CellFormatting;

            // ── Refresh button ──────────────────────────
            var btnRefresh = UITheme.MakeButton("↻ Làm mới", 130, 38);
            btnRefresh.Location = new Point(28, 455);
            btnRefresh.Click += async (s, e) => await LoadHistoryFromServerAsync();

            // ── Close button ────────────────────────────
            var btnClose = UITheme.MakeOutlineButton("Đóng", 120, 38);
            btnClose.Location = new Point(832, 455);
            btnClose.Click += (s, e) => Close();

            // ── Add controls ────────────────────────────
            Controls.AddRange(new Control[]
            {
                lblIcon, lblTitle, sep,
                lblStatus, dgv,
                btnRefresh, btnClose,
            });

            Shown += async (s, e) => await LoadHistoryFromServerAsync();
        }

        // ────────────────────────────────────────────────
        //  Winner cell colouring
        // ────────────────────────────────────────────────
        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].Name != "Winner") return;
            if (e.Value == null) return;

            string winner = e.Value.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(winner) || winner == "-")
            {
                // Draw (Hòa)
                e.CellStyle!.ForeColor  = UITheme.Warning;
                e.CellStyle.BackColor   = Color.FromArgb(50, 40, 10);
            }
            else if (winner.Equals(AppSession.CurrentUsername, StringComparison.OrdinalIgnoreCase))
            {
                // Win
                e.CellStyle!.ForeColor  = UITheme.Success;
                e.CellStyle.BackColor   = Color.FromArgb(10, 50, 25);
            }
            else
            {
                // Loss
                e.CellStyle!.ForeColor  = UITheme.Danger;
                e.CellStyle.BackColor   = Color.FromArgb(50, 10, 15);
            }
        }

        // ────────────────────────────────────────────────
        //  Server call (preserved from original)
        // ────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadHistoryFromServerAsync()
        {
            dgv.Rows.Clear();
            lblStatus.Text      = "⏳ Đang tải lịch sử trận đấu…";
            lblStatus.ForeColor = UITheme.TextSecondary;

            try
            {
                MatchHistoryPacket? response = await AppSession.SendAndWaitAsync<MatchHistoryPacket>(
                    new MatchHistoryPacket { PacketType = "MATCH_HISTORY" },
                    "MATCH_HISTORY_RESULT");

                if (response == null || !response.Success)
                {
                    string msg = response?.Message ?? "Không lấy được lịch sử đấu";
                    dgv.Rows.Add("", "", "", msg, "", "", "");
                    lblStatus.Text      = "⚠ " + msg;
                    lblStatus.ForeColor = UITheme.Warning;
                    return;
                }

                if (response.History.Count == 0)
                {
                    dgv.Rows.Add("", AppSession.CurrentUsername, "", "Chưa có lịch sử đấu trên Server", "", "", "");
                    lblStatus.Text      = "ℹ Chưa có lịch sử đấu nào.";
                    lblStatus.ForeColor = UITheme.TextSecondary;
                    return;
                }

                foreach (var item in response.History)
                {
                    dgv.Rows.Add(
                        item.PlayedAt,
                        item.Player1,
                        item.Player2,
                        string.IsNullOrWhiteSpace(item.Winner) ? "-" : item.Winner,
                        item.Difficulty,
                        FormatTime(item.DurationSeconds),
                        $"P1:{item.EloChangeP1}, P2:{item.EloChangeP2}");
                }

                lblStatus.Text      = $"✔ Tìm thấy {response.History.Count} trận đấu.";
                lblStatus.ForeColor = UITheme.Success;
            }
            catch (Exception ex)
            {
                dgv.Rows.Add("", "", "", "Không kết nối được Server: " + ex.Message, "", "", "");
                lblStatus.Text      = "✖ Lỗi kết nối Server: " + ex.Message;
                lblStatus.ForeColor = UITheme.Danger;
            }
        }

        // ────────────────────────────────────────────────
        //  Helpers
        // ────────────────────────────────────────────────
        private static string FormatTime(int seconds)
        {
            if (seconds <= 0) return "-";
            int minutes      = seconds / 60;
            int remainSeconds = seconds % 60;
            return $"{minutes:00}:{remainSeconds:00}";
        }
    }
}
