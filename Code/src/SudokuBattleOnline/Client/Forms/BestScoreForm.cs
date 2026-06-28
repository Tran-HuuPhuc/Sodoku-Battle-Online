using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;

namespace SudokuBattleOnline.Forms
{
    public class BestScoreForm : Form
    {
        private readonly ListView listView;
        private readonly Label    lblStatus;

        public BestScoreForm()
        {
            // ── Form setup ──────────────────────────────
            Text            = "Bảng Thành Tích Cá Nhân";
            Size            = new Size(760, 520);
            MinimumSize     = new Size(760, 520);
            BackColor       = UITheme.BgMain;
            ForeColor       = UITheme.TextPrimary;
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // ── Trophy icon + title ──────────────────────
            var lblIcon = new Label
            {
                Text      = "🏆",
                Font      = new Font("Segoe UI Emoji", 28, FontStyle.Regular),
                ForeColor = UITheme.Gold,
                AutoSize  = true,
                Location  = new Point(30, 18),
            };

            var lblTitle = new Label
            {
                Text      = "Bảng Thành Tích Cá Nhân",
                Font      = UITheme.FontTitle,
                ForeColor = UITheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(80, 22),
            };

            // ── Separator ───────────────────────────────
            var sep = UITheme.MakeSeparator(700);
            sep.Location = new Point(30, 75);

            // ── Status label ────────────────────────────
            lblStatus = new Label
            {
                Text      = "⏳ Đang tải dữ liệu…",
                Font      = UITheme.FontBody,
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(30, 86),
            };

            // ── ListView ────────────────────────────────
            listView = new ListView
            {
                Location       = new Point(30, 116),
                Size           = new Size(700, 330),
                View           = View.Details,
                FullRowSelect  = true,
                GridLines      = false,
                HeaderStyle    = ColumnHeaderStyle.Nonclickable,
                OwnerDraw      = false,
            };
            UITheme.ApplyListViewTheme(listView);

            listView.Columns.Add("Rank",        70,  HorizontalAlignment.Center);
            listView.Columns.Add("Username",    140, HorizontalAlignment.Left);
            listView.Columns.Add("Difficulty",  110, HorizontalAlignment.Center);
            listView.Columns.Add("Best Score",  110, HorizontalAlignment.Center);
            listView.Columns.Add("Best Time",   110, HorizontalAlignment.Center);
            listView.Columns.Add("Achieved At", 155, HorizontalAlignment.Left);

            // ── Close button ────────────────────────────
            var btnClose = UITheme.MakeOutlineButton("Đóng", 120, 38);
            btnClose.Location = new Point(610, 460);
            btnClose.Click += (s, e) => Close();

            // ── Add controls ────────────────────────────
            Controls.AddRange(new Control[]
            {
                lblIcon, lblTitle, sep,
                lblStatus, listView,
                btnClose,
            });

            Load += BestScoreForm_Load;
        }

        // ────────────────────────────────────────────────
        //  Server call (preserved + medal coloring added)
        // ────────────────────────────────────────────────
        private async void BestScoreForm_Load(object sender, EventArgs e)
        {
            try
            {
                var request = new BestScorePacket { PacketType = "BEST_SCORE_REQUEST" };

                BestScorePacket? response =
                    await AppSession.SendAndWaitAsync<BestScorePacket>(request, "BEST_SCORE_RESULT");

                listView.Items.Clear();

                if (response == null)
                {
                    lblStatus.Text      = "⚠ Không nhận được phản hồi từ Server.";
                    lblStatus.ForeColor = UITheme.Warning;
                    return;
                }

                if (!response.Success)
                {
                    lblStatus.Text      = "⚠ " + response.Message;
                    lblStatus.ForeColor = UITheme.Warning;
                    return;
                }

                if (response.Scores.Count == 0)
                {
                    lblStatus.Text      = "ℹ Chưa có dữ liệu Best Score.";
                    lblStatus.ForeColor = UITheme.TextSecondary;
                    return;
                }

                lblStatus.Text      = $"✔ Tìm thấy {response.Scores.Count} kết quả.";
                lblStatus.ForeColor = UITheme.Success;

                foreach (var score in response.Scores)
                {
                    // Medal prefix for top-3
                    string rankLabel = score.Rank switch
                    {
                        1 => "🥇 1",
                        2 => "🥈 2",
                        3 => "🥉 3",
                        _ => score.Rank.ToString()
                    };

                    var item = new ListViewItem(rankLabel);
                    item.SubItems.Add(score.Username);
                    item.SubItems.Add(score.Difficulty);
                    item.SubItems.Add(score.BestScore.ToString());
                    item.SubItems.Add(score.BestTimeSeconds + "s");
                    item.SubItems.Add(score.AchievedAt);

                    // Medal row colors
                    (item.BackColor, item.ForeColor) = score.Rank switch
                    {
                        1 => (Color.FromArgb(60, 52, 0),   UITheme.Gold),
                        2 => (Color.FromArgb(40, 42, 48),  UITheme.Silver),
                        3 => (Color.FromArgb(50, 32, 8),   UITheme.Bronze),
                        _ => (UITheme.BgCard,              UITheme.TextPrimary)
                    };
                    item.UseItemStyleForSubItems = true;

                    listView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text      = "✖ Lỗi tải Best Score: " + ex.Message;
                lblStatus.ForeColor = UITheme.Danger;
            }
        }
    }
}