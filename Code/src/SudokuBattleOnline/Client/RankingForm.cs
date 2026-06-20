using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class RankingForm : Form
    {
        private readonly DataGridView dgv;

        public RankingForm()
        {
            Text = "Bảng Xếp Hạng";
            Size = new Size(750, 450);
            StartPosition = FormStartPosition.CenterParent;

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgv.Columns.Add("Rank", "Hạng");
            dgv.Columns.Add("Username", "Người chơi");
            dgv.Columns.Add("Elo", "ELO");
            dgv.Columns.Add("Wins", "Thắng");
            dgv.Columns.Add("Matches", "Tổng trận");

            Controls.Add(dgv);
            Shown += async (s, e) => await LoadRankingFromServerAsync();
        }

        private async System.Threading.Tasks.Task LoadRankingFromServerAsync()
        {
            dgv.Rows.Clear();
            try
            {
                RankingPacket? response = await AppSession.SendAndWaitAsync<RankingPacket>(
                    new RankingPacket { PacketType = "RANKING" },
                    "RANKING");

                if (response == null || !response.Success)
                {
                    dgv.Rows.Add("", response?.Message ?? "Không lấy được bảng xếp hạng", "", "", "");
                    return;
                }

                foreach (var item in response.Rankings)
                {
                    dgv.Rows.Add(item.Rank, item.Username, item.RankPoint, item.WinCount, item.MatchCount);
                }

                if (response.Rankings.Count == 0)
                {
                    dgv.Rows.Add("", "Chưa có dữ liệu người chơi trên Server", "", "", "");
                }
            }
            catch (Exception ex)
            {
                dgv.Rows.Add("", "Không kết nối được Server: " + ex.Message, "", "", "");
            }
        }
    }
}
