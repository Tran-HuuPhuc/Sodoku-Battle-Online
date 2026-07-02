using System;
using System.Drawing;
using System.Windows.Forms;
using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;

namespace SudokuBattleOnline.Forms
{
    public class BestScoreForm : Form
    {
        private readonly DataGridView dgv;
        private readonly Label lblStatus;

        public BestScoreForm()
        {
            Text = "Thành Tích Cá Nhân";
            Size = new Size(900, 560);
            MinimumSize = new Size(820, 520);
            BackColor = UITheme.BgMain;
            ForeColor = UITheme.TextPrimary;
            StartPosition = FormStartPosition.CenterParent;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 108,
                BackColor = UITheme.BgMain
            };

            var lblTitle = new Label
            {
                Text = "THÀNH TÍCH CÁ NHÂN",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Location = new Point(28, 18),
                Size = new Size(520, 40)
            };

            lblStatus = new Label
            {
                Text = "Đang tải dữ liệu thành tích...",
                Font = UITheme.FontBody,
                ForeColor = UITheme.TextSecondary,
                Location = new Point(30, 66),
                Size = new Size(760, 24)
            };

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblStatus);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowTemplate = { Height = 38 }
            };
            UITheme.ApplyDgvTheme(dgv);
            dgv.ColumnHeadersHeight = 42;

            dgv.Columns.Add("Rank", "Hạng");
            dgv.Columns.Add("Username", "Người chơi");
            dgv.Columns.Add("Difficulty", "Độ khó");
            dgv.Columns.Add("BestScore", "Điểm tốt nhất");
            dgv.Columns.Add("BestTime", "Thời gian tốt nhất");
            dgv.Columns.Add("AchievedAt", "Ngày đạt");

            dgv.Columns["Rank"].FillWeight = 10;
            dgv.Columns["Username"].FillWeight = 20;
            dgv.Columns["Difficulty"].FillWeight = 15;
            dgv.Columns["BestScore"].FillWeight = 17;
            dgv.Columns["BestTime"].FillWeight = 18;
            dgv.Columns["AchievedAt"].FillWeight = 24;

            dgv.CellFormatting += Dgv_CellFormatting;

            Controls.Add(dgv);
            Controls.Add(header);
            Load += BestScoreForm_Load;
        }

        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name == "Rank" && e.Value != null)
            {
                string value = e.Value.ToString() ?? string.Empty;
                if (value == "1") e.CellStyle.ForeColor = UITheme.Gold;
                else if (value == "2") e.CellStyle.ForeColor = UITheme.Silver;
                else if (value == "3") e.CellStyle.ForeColor = UITheme.Bronze;
                e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        private async void BestScoreForm_Load(object? sender, EventArgs e)
        {
            dgv.Rows.Clear();
            try
            {
                var request = new BestScorePacket { PacketType = "BEST_SCORE_REQUEST" };
                BestScorePacket? response =
                    await AppSession.SendAndWaitAsync<BestScorePacket>(request, "BEST_SCORE_RESULT");

                if (response == null)
                {
                    lblStatus.Text = "Không nhận được phản hồi từ Server.";
                    lblStatus.ForeColor = UITheme.Warning;
                    return;
                }

                if (!response.Success)
                {
                    lblStatus.Text = response.Message;
                    lblStatus.ForeColor = UITheme.Warning;
                    return;
                }

                if (response.Scores.Count == 0)
                {
                    lblStatus.Text = "Chưa có dữ liệu Best Score.";
                    lblStatus.ForeColor = UITheme.TextSecondary;
                    return;
                }

                foreach (var score in response.Scores)
                {
                    dgv.Rows.Add(
                        score.Rank,
                        score.Username,
                        score.Difficulty,
                        score.BestScore,
                        FormatTime(score.BestTimeSeconds),
                        score.AchievedAt);
                }

                lblStatus.Text = $"Đã tải {response.Scores.Count} thành tích.";
                lblStatus.ForeColor = UITheme.Success;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải Best Score: " + ex.Message;
                lblStatus.ForeColor = UITheme.Danger;
            }
        }

        private static string FormatTime(int seconds)
        {
            if (seconds <= 0) return "-";
            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m:00}:{s:00}";
        }
    }
}
