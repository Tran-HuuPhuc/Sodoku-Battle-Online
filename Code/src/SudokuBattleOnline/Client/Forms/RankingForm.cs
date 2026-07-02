using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public partial class RankingForm : Form
    {
        private readonly DataGridView dgv;
        private readonly Label lblStatus;

        public RankingForm()
        {
            Text = "Bảng Xếp Hạng";
            Size = new Size(900, 560);
            MinimumSize = new Size(820, 520);
            BackColor = UITheme.BgMain;
            ForeColor = UITheme.TextPrimary;
            StartPosition = FormStartPosition.CenterParent;

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 108,
                BackColor = UITheme.BgMain,
                Padding = new Padding(28, 18, 28, 0)
            };

            var lblTitle = new Label
            {
                Text = "BẢNG XẾP HẠNG",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Location = new Point(28, 18),
                Size = new Size(420, 40)
            };

            lblStatus = new Label
            {
                Text = "Đang tải bảng xếp hạng từ Server...",
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
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dgv.Columns.Add("Rank", "Hạng");
            dgv.Columns.Add("Username", "Người chơi");
            dgv.Columns.Add("Elo", "ELO");
            dgv.Columns.Add("Wins", "Thắng");
            dgv.Columns.Add("Matches", "Tổng trận");
            dgv.Columns.Add("WinRate", "Tỉ lệ thắng");

            dgv.Columns["Rank"].FillWeight = 12;
            dgv.Columns["Username"].FillWeight = 30;
            dgv.Columns["Elo"].FillWeight = 16;
            dgv.Columns["Wins"].FillWeight = 14;
            dgv.Columns["Matches"].FillWeight = 14;
            dgv.Columns["WinRate"].FillWeight = 18;

            dgv.CellFormatting += Dgv_CellFormatting;

            Controls.Add(dgv);
            Controls.Add(header);
            Shown += async (s, e) => await LoadRankingFromServerAsync();
        }

        private void Dgv_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name == "Rank" && e.Value != null)
            {
                string value = e.Value.ToString() ?? string.Empty;
                if (value.StartsWith("1")) e.CellStyle.ForeColor = UITheme.Gold;
                else if (value.StartsWith("2")) e.CellStyle.ForeColor = UITheme.Silver;
                else if (value.StartsWith("3")) e.CellStyle.ForeColor = UITheme.Bronze;
                e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }

            if (dgv.Columns[e.ColumnIndex].Name == "Elo")
            {
                e.CellStyle.ForeColor = UITheme.Accent;
                e.CellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        private async System.Threading.Tasks.Task LoadRankingFromServerAsync()
        {
            dgv.Rows.Clear();
            lblStatus.Text = "Đang tải bảng xếp hạng từ Server...";
            lblStatus.ForeColor = UITheme.TextSecondary;

            try
            {
                RankingPacket? response = await AppSession.SendAndWaitAsync<RankingPacket>(
                    new RankingPacket { PacketType = "RANKING" },
                    "RANKING");

                if (response == null || !response.Success)
                {
                    lblStatus.Text = response?.Message ?? "Không lấy được bảng xếp hạng.";
                    lblStatus.ForeColor = UITheme.Warning;
                    return;
                }

                if (response.Rankings.Count == 0)
                {
                    lblStatus.Text = "Chưa có dữ liệu người chơi trên Server.";
                    lblStatus.ForeColor = UITheme.TextSecondary;
                    return;
                }

                foreach (var item in response.Rankings)
                {
                    string rank = item.Rank switch
                    {
                        1 => "1 - Top 1",
                        2 => "2 - Top 2",
                        3 => "3 - Top 3",
                        _ => item.Rank.ToString()
                    };

                    string winRate = item.MatchCount <= 0
                        ? "0%"
                        : $"{item.WinCount * 100.0 / item.MatchCount:0.#}%";

                    dgv.Rows.Add(rank, item.Username, item.RankPoint, item.WinCount, item.MatchCount, winRate);
                }

                lblStatus.Text = $"Đã tải {response.Rankings.Count} người chơi.";
                lblStatus.ForeColor = UITheme.Success;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Không kết nối được Server: " + ex.Message;
                lblStatus.ForeColor = UITheme.Danger;
            }
        }
    }
}
