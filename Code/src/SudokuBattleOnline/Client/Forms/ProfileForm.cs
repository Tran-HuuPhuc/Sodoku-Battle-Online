using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SudokuBattleOnline.Forms
{
    public class ProfileForm : Form
    {
        // ── Header ──────────────────────────────────────
        private readonly Label  lblAvatar;
        private readonly Label  lblUsername;
        private readonly Label  lblSubtitle;

        // ── Stat cards ──────────────────────────────────
        private readonly Panel  cardElo;
        private readonly Label  lblEloValue;
        private readonly Label  lblEloCaption;

        private readonly Panel  cardWin;
        private readonly Label  lblWinValue;
        private readonly Label  lblWinCaption;

        private readonly Panel  cardLoss;
        private readonly Label  lblLossValue;
        private readonly Label  lblLossCaption;

        // ── Footer info ─────────────────────────────────
        private readonly Label  lblCreatedAt;
        private readonly Label  lblStatus;

        // ── Separator ───────────────────────────────────
        private readonly Label  sep;

        public ProfileForm()
        {
            // ── Form setup ──────────────────────────────
            Text            = "Hồ Sơ Người Chơi";
            Size            = new Size(520, 420);
            MinimumSize     = new Size(520, 420);
            MaximumSize     = new Size(520, 420);
            BackColor       = UITheme.BgMain;
            ForeColor       = UITheme.TextPrimary;
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // ── Avatar emoji ────────────────────────────
            lblAvatar = new Label
            {
                Text      = "👤",
                Font      = new Font("Segoe UI Emoji", 32, FontStyle.Regular),
                ForeColor = UITheme.Accent,
                BackColor = UITheme.BgCard,
                Size      = new Size(80, 80),
                Location  = new Point(30, 24),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            // Rounded look via Paint
            lblAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(UITheme.Border, 2);
                e.Graphics.DrawEllipse(pen, 1, 1, lblAvatar.Width - 3, lblAvatar.Height - 3);
            };

            // ── Username ────────────────────────────────
            lblUsername = new Label
            {
                Text      = AppSession.IsLoggedIn ? AppSession.CurrentUsername : "—",
                Font      = UITheme.FontTitle,
                ForeColor = UITheme.TextPrimary,
                AutoSize  = true,
                Location  = new Point(124, 30),
            };

            lblSubtitle = new Label
            {
                Text      = "Hồ sơ người chơi",
                Font      = UITheme.FontBody,
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(124, 70),
            };

            // ── Separator ───────────────────────────────
            sep = UITheme.MakeSeparator(460);
            sep.Location = new Point(30, 120);

            // ── Stat Cards ──────────────────────────────
            const int cardW = 130, cardH = 100, cardY = 140;

            cardElo = MakeStatCard(cardW, cardH, new Point(30, cardY));
            lblEloValue = MakeStatValueLabel("—", UITheme.Gold);
            lblEloCaption = MakeStatCaptionLabel("ELO Rating");
            PositionStatLabels(cardElo, lblEloValue, lblEloCaption);

            cardWin = MakeStatCard(cardW, cardH, new Point(190, cardY));
            lblWinValue = MakeStatValueLabel("—", UITheme.Success);
            lblWinCaption = MakeStatCaptionLabel("Thắng");
            PositionStatLabels(cardWin, lblWinValue, lblWinCaption);

            cardLoss = MakeStatCard(cardW, cardH, new Point(350, cardY));
            lblLossValue = MakeStatValueLabel("—", UITheme.Danger);
            lblLossCaption = MakeStatCaptionLabel("Thua");
            PositionStatLabels(cardLoss, lblLossValue, lblLossCaption);

            // ── Created-at label ────────────────────────
            lblCreatedAt = new Label
            {
                Text      = "Ngày tạo tài khoản: —",
                Font      = UITheme.FontBody,
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                Location  = new Point(30, 262),
            };

            // ── Status label ────────────────────────────
            lblStatus = new Label
            {
                Text      = "⏳ Đang lấy dữ liệu từ Server…",
                Font      = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(30, 295),
            };

            // ── Close button ────────────────────────────
            var btnClose = UITheme.MakeOutlineButton("Đóng", 120, 38);
            btnClose.Location = new Point(370, 340);
            btnClose.Click += (s, e) => Close();

            // ── Add controls ────────────────────────────
            Controls.AddRange(new Control[]
            {
                lblAvatar, lblUsername, lblSubtitle,
                sep,
                cardElo, cardWin, cardLoss,
                lblCreatedAt, lblStatus,
                btnClose,
            });

            // ── Load ────────────────────────────────────
            Shown += async (s, e) => await LoadProfileFromServerAsync();
        }

        // ────────────────────────────────────────────────
        //  Helpers – stat card construction
        // ────────────────────────────────────────────────
        private static Panel MakeStatCard(int w, int h, Point location)
        {
            var p = new Panel
            {
                Size      = new Size(w, h),
                Location  = location,
                BackColor = UITheme.BgCard,
            };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(UITheme.Border, 1);
                var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                e.Graphics.DrawRectangle(pen, rect);
            };
            return p;
        }

        private static Label MakeStatValueLabel(string text, Color color)
            => new()
            {
                Text      = text,
                Font      = new Font(UITheme.FontFamily, 26, FontStyle.Bold),
                ForeColor = color,
                AutoSize  = true,
                TextAlign = ContentAlignment.MiddleCenter,
            };

        private static Label MakeStatCaptionLabel(string text)
            => new()
            {
                Text      = text,
                Font      = UITheme.FontSmall,
                ForeColor = UITheme.TextSecondary,
                AutoSize  = true,
                TextAlign = ContentAlignment.MiddleCenter,
            };

        private static void PositionStatLabels(Panel card, Label value, Label caption)
        {
            // Value will be centred after load; caption below
            value.Location   = new Point(0, 18);
            caption.Location = new Point(0, 65);
            card.Controls.Add(value);
            card.Controls.Add(caption);

            // Centre horizontally after the card is shown
            card.Layout += (s, e) =>
            {
                value.Left   = (card.Width - value.Width)   / 2;
                caption.Left = (card.Width - caption.Width) / 2;
            };
        }

        // ────────────────────────────────────────────────
        //  Server call (preserved from original)
        // ────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoadProfileFromServerAsync()
        {
            if (!AppSession.IsLoggedIn)
            {
                lblStatus.Text = "⚠ Chưa đăng nhập.";
                return;
            }

            try
            {
                var request = new UserProfilePacket { PacketType = "PROFILE" };

                UserProfilePacket? response =
                    await AppSession.SendAndWaitAsync<UserProfilePacket>(request, "PROFILE_RESULT");

                if (response == null)
                {
                    lblStatus.Text = "⚠ Server không trả về dữ liệu hồ sơ.";
                    return;
                }

                if (!response.Success)
                {
                    lblStatus.Text = "⚠ " + response.Message;
                    return;
                }

                // Update username header
                lblUsername.Text = response.Username ?? AppSession.CurrentUsername;

                // Update stat cards
                lblEloValue.Text   = response.Elo.ToString();
                lblWinValue.Text   = response.TotalWins.ToString();
                lblLossValue.Text  = response.TotalLosses.ToString();

                // Force re-centre
                cardElo.PerformLayout();
                cardWin.PerformLayout();
                cardLoss.PerformLayout();

                // Created at
                lblCreatedAt.Text = "📅 Ngày tạo tài khoản: " + response.CreatedAt;
                lblStatus.Text    = "✔ Dữ liệu được lấy từ server.";
                lblStatus.ForeColor = UITheme.Success;
            }
            catch (Exception ex)
            {
                lblStatus.Text    = "✖ Không lấy được hồ sơ từ Server: " + ex.Message;
                lblStatus.ForeColor = UITheme.Danger;
            }
        }
    }
}
