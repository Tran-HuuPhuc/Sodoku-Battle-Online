using Client.Network;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using Shared.Constants;

namespace SudokuBattleOnline.Client
{
    /// <summary>
    /// Lưu phiên làm việc chung của Client: kết nối Server và user đang đăng nhập.
    /// </summary>
    public static class AppSession
    {
        public static string ServerIp { get; set; } = NetworkConstants.SERVER_IP;
        public static int ServerPort { get; set; } = NetworkConstants.SERVER_PORT;
        public static string CurrentUsername { get; set; } = string.Empty;

        public static ClientConnection Connection { get; } = new ClientConnection();

        public static bool IsConnected => Connection.IsConnected;
        public static bool IsLoggedIn => !string.IsNullOrWhiteSpace(CurrentUsername);

        public static event Action<BasePacket, string>? PacketReceived;

        static AppSession()
        {
            Connection.OnMessageReceived += HandleRawMessage;
            Connection.OnDisconnected += () =>
            {
                CurrentUsername = string.Empty;
            };
        }

        public static async Task EnsureConnectedAsync()
        {
            if (!Connection.IsConnected)
                await Connection.ConnectAsync(ServerIp, ServerPort);
        }

        public static async Task SendPacketAsync(BasePacket packet)
        {
            await EnsureConnectedAsync();
            await Connection.SendPacketAsync(packet);
        }

        public static async Task<T?> SendAndWaitAsync<T>(BasePacket packet, string expectedPacketType, int timeoutMs = 8000)
            where T : BasePacket
        {
            await EnsureConnectedAsync();

            var tcs = new TaskCompletionSource<T?>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Handler(BasePacket basePacket, string rawJson)
            {
                if (!string.Equals(basePacket.PacketType, expectedPacketType, StringComparison.OrdinalIgnoreCase))
                    return;

                try
                {
                    T? result = JsonSerializer.Deserialize<T>(rawJson);
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            PacketReceived += Handler;
            try
            {
                await Connection.SendPacketAsync(packet);

                Task completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
                if (completed != tcs.Task)
                    throw new TimeoutException("Server không phản hồi đúng thời gian cho phép.");

                return await tcs.Task;
            }
            finally
            {
                PacketReceived -= Handler;
            }
        }

        private static void HandleRawMessage(string rawJson)
        {
            try
            {
                BasePacket? basePacket = JsonSerializer.Deserialize<BasePacket>(rawJson);
                if (basePacket == null || string.IsNullOrWhiteSpace(basePacket.PacketType))
                    return;

                PacketReceived?.Invoke(basePacket, rawJson);
            }
            catch
            {
                // Bỏ qua gói tin không hợp lệ.
            }
        }
    }

    public static class UITheme
    {
        // ═══════════════════════════════════════════
        //  MÀU SẮC – Palette tối hiện đại
        // ═══════════════════════════════════════════

        // Nền
        public static readonly Color BgDeep    = Color.FromArgb(13,  17,  28);   // #0D111C – Nền sâu nhất
        public static readonly Color BgMain    = Color.FromArgb(20,  24,  36);   // #141824 – Nền chính
        public static readonly Color BgCard    = Color.FromArgb(28,  33,  48);   // #1C2130 – Nền card
        public static readonly Color BgElevated= Color.FromArgb(36,  42,  60);   // #242A3C – Nền nổi (input, panel)
        public static readonly Color BgHover   = Color.FromArgb(48,  55,  78);   // #30374E – Hover state

        // Viền
        public static readonly Color Border    = Color.FromArgb(55,  65,  95);   // #37415F
        public static readonly Color BorderSub = Color.FromArgb(40,  48,  70);   // #283048

        // Text
        public static readonly Color TextPrimary   = Color.FromArgb(230, 235, 255); // Trắng xanh sáng
        public static readonly Color TextSecondary = Color.FromArgb(140, 155, 190); // Xám xanh
        public static readonly Color TextMuted     = Color.FromArgb(80,  95,  135); // Mờ

        // Accent – Xanh cyan chủ đạo
        public static readonly Color Accent     = Color.FromArgb(64,  180, 255);  // #40B4FF
        public static readonly Color AccentDark = Color.FromArgb(30,  120, 200);  // Đậm hơn
        public static readonly Color AccentGlow = Color.FromArgb(30,  80,  150);  // Phát sáng

        // Màu trạng thái
        public static readonly Color Success  = Color.FromArgb(46,  213, 115);   // Xanh lá neon
        public static readonly Color Warning  = Color.FromArgb(255, 165,  50);   // Cam
        public static readonly Color Danger   = Color.FromArgb(255,  75,  90);   // Đỏ
        public static readonly Color Gold     = Color.FromArgb(255, 215,   0);   // Vàng rank
        public static readonly Color Silver   = Color.FromArgb(192, 192, 192);   // Bạc rank
        public static readonly Color Bronze   = Color.FromArgb(205, 127,  50);   // Đồng rank

        // ═══════════════════════════════════════════
        //  FONT
        // ═══════════════════════════════════════════
        public const string FontFamily = "Segoe UI";

        public static Font FontTitle    => new(FontFamily, 22, FontStyle.Bold);
        public static Font FontSubtitle => new(FontFamily, 14, FontStyle.Bold);
        public static Font FontBody     => new(FontFamily, 10, FontStyle.Regular);
        public static Font FontBodyBold => new(FontFamily, 10, FontStyle.Bold);
        public static Font FontSmall    => new(FontFamily, 8,  FontStyle.Regular);
        public static Font FontMono     => new("Consolas",  12, FontStyle.Regular);
        public static Font FontNumber   => new("Segoe UI",  18, FontStyle.Bold);  // Số trong bàn cờ

        // ═══════════════════════════════════════════
        //  SPACING & SIZING
        // ═══════════════════════════════════════════
        public const int RadiusCard   = 12;
        public const int RadiusButton = 8;
        public const int PaddingMd    = 16;
        public const int PaddingSm    = 8;

        // ═══════════════════════════════════════════
        //  HELPER – Tạo nút bấm chuẩn
        // ═══════════════════════════════════════════

        /// <summary>Nút primary – màu accent xanh.</summary>
        public static Button MakeButton(string text, int w = 180, int h = 44)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(w, h),
                Font = FontBodyBold,
                BackColor = Accent,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = AccentDark;
            btn.MouseLeave += (s, e) => btn.BackColor = Accent;
            return btn;
        }

        /// <summary>Nút secondary – viền mỏng không nền.</summary>
        public static Button MakeOutlineButton(string text, int w = 180, int h = 44)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(w, h),
                Font = FontBodyBold,
                BackColor = Color.Transparent,
                ForeColor = Accent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = Accent;
            btn.MouseEnter += (s, e) => { btn.BackColor = AccentGlow; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.Transparent; };
            return btn;
        }

        /// <summary>Nút Danger – màu đỏ.</summary>
        public static Button MakeDangerButton(string text, int w = 180, int h = 44)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(w, h),
                Font = FontBodyBold,
                BackColor = Danger,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(200, 50, 65);
            btn.MouseLeave += (s, e) => btn.BackColor = Danger;
            return btn;
        }

        /// <summary>Nút Success – màu xanh lá.</summary>
        public static Button MakeSuccessButton(string text, int w = 180, int h = 44)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(w, h),
                Font = FontBodyBold,
                BackColor = Success,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(30, 170, 90);
            btn.MouseLeave += (s, e) => btn.BackColor = Success;
            return btn;
        }

        /// <summary>Label tiêu đề trang.</summary>
        public static Label MakeTitle(string text)
            => new() { Text = text, Font = FontTitle, ForeColor = TextPrimary, AutoSize = true };

        /// <summary>Label mô tả nhỏ.</summary>
        public static Label MakeSubLabel(string text)
            => new() { Text = text, Font = FontBody, ForeColor = TextSecondary, AutoSize = true };

        /// <summary>TextBox chuẩn – dark style.</summary>
        public static TextBox MakeInput(int w = 280, int h = 36, bool password = false)
        {
            var txt = new TextBox
            {
                Size = new Size(w, h),
                Font = FontBody,
                BackColor = BgElevated,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
            };
            if (password) txt.PasswordChar = '*';
            return txt;
        }

        /// <summary>Panel card với nền nổi.</summary>
        public static Panel MakeCard(int w, int h)
            => new() { Size = new Size(w, h), BackColor = BgCard };

        /// <summary>Separator ngang (Label rỗng với border dưới).</summary>
        public static Label MakeSeparator(int w)
            => new() { Size = new Size(w, 1), BackColor = Border };

        /// <summary>Gradient string for labels – màu accent</summary>
        public static Color[] AccentGradient => new[] { Accent, Color.FromArgb(130, 80, 255) };

        // ═══════════════════════════════════════════
        //  HELPER – Apply theme cho toàn bộ Form
        // ═══════════════════════════════════════════
        public static void ApplyFormTheme(Form form)
        {
            form.BackColor = BgMain;
            form.ForeColor = TextPrimary;
        }

        /// <summary>Style cho DataGridView dark theme.</summary>
        public static void ApplyDgvTheme(DataGridView dgv)
        {
            dgv.BackgroundColor = BgCard;
            dgv.ForeColor = TextPrimary;
            dgv.GridColor = Border;
            dgv.BorderStyle = BorderStyle.None;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = BgElevated;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Accent;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontBodyBold;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.DefaultCellStyle.BackColor = BgCard;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = BgHover;
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = BgElevated;
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowHeadersVisible = false;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgv.RowTemplate.Height = 34;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        }

        /// <summary>Style cho ListView dark theme.</summary>
        public static void ApplyListViewTheme(ListView lv)
        {
            lv.BackColor = BgCard;
            lv.ForeColor = TextPrimary;
            lv.BorderStyle = BorderStyle.None;
        }
    }
}
