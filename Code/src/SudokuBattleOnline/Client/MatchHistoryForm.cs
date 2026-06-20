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

        public MatchHistoryForm()
        {
            Text = "Lịch Sử Đấu";
            Width = 950;
            Height = 450;
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

            dgv.Columns.Add("Date", "Ngày chơi");
            dgv.Columns.Add("Player1", "Người chơi");
            dgv.Columns.Add("Player2", "Đối thủ");
            dgv.Columns.Add("Winner", "Người thắng");
            dgv.Columns.Add("Difficulty", "Độ khó");
            dgv.Columns.Add("Time", "Thời gian");
            dgv.Columns.Add("Elo", "ELO đổi");

            Controls.Add(dgv);
            Shown += async (s, e) => await LoadHistoryFromServerAsync();
        }

        private async System.Threading.Tasks.Task LoadHistoryFromServerAsync()
        {
            dgv.Rows.Clear();
            try
            {
                MatchHistoryPacket? response = await AppSession.SendAndWaitAsync<MatchHistoryPacket>(
                    new MatchHistoryPacket { PacketType = "MATCH_HISTORY" },
                    "MATCH_HISTORY_RESULT");

                if (response == null || !response.Success)
                {
                    dgv.Rows.Add("", "", "", response?.Message ?? "Không lấy được lịch sử đấu", "", "", "");
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

                if (response.History.Count == 0)
                {
                    dgv.Rows.Add("", AppSession.CurrentUsername, "", "Chưa có lịch sử đấu trên Server", "", "", "");
                }
            }
            catch (Exception ex)
            {
                dgv.Rows.Add("", "", "", "Không kết nối được Server: " + ex.Message, "", "", "");
            }
        }

        private static string FormatTime(int seconds)
        {
            if (seconds <= 0)
                return "-";

            int minutes = seconds / 60;
            int remainSeconds = seconds % 60;
            return $"{minutes:00}:{remainSeconds:00}";
        }
    }
}
