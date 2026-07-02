using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuBattleOnline.Forms
{
    public partial class RoomForm : Form
    {
        private string roomId;

        // Header
        private Label lblRoomCode;

        // Left panel controls
        private ListBox lstPlayers;
        private Button btnReady;
        private Button btnStart;
        private Button btnLeave;

        // Right panel controls
        private TextBox txtChat;
        private TextBox txtInputChat;
        private Button btnSend;

        private bool isHost = false;
        private bool isReady = false;

        // Layout constants
        private const int LeftPanelW = 280;
        private const int PanelTop = 80;

        public RoomForm(string roomId)
        {
            this.roomId = roomId;
            InitializeForm();

            AppSession.PacketReceived += AppSession_PacketReceived;

            this.Load += async (s, e) => await Task.CompletedTask;

            this.FormClosed += (s, e) =>
            {
                AppSession.PacketReceived -= AppSession_PacketReceived;
            };
        }

        private void InitializeForm()
        {
            Text = "Phòng Chờ Đấu";
            Size = new Size(860, 580);
            BackColor = UITheme.BgMain;
            FormBorderStyle = FormBorderStyle.None;

            BuildHeader();
            BuildLeftPanel();
            BuildRightPanel();
        }

        // ════════════════════════════════════════════════════════
        //  HEADER
        // ════════════════════════════════════════════════════════
        private void BuildHeader()
        {
            var pnlHeader = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(860, 72),
                BackColor = UITheme.BgDeep
            };

            var lblTitle = new Label
            {
                Text = "🏠  Phòng chờ",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 22)
            };

            lblRoomCode = new Label
            {
                Text = $"#{roomId}",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.Accent,
                AutoSize = true,
                Location = new Point(186, 22)
            };

            var sep = new Label
            {
                Location = new Point(0, 71),
                Size = new Size(860, 1),
                BackColor = UITheme.Border
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblRoomCode);
            pnlHeader.Controls.Add(sep);
            Controls.Add(pnlHeader);
        }

        // ════════════════════════════════════════════════════════
        //  LEFT PANEL – Người chơi + Buttons
        // ════════════════════════════════════════════════════════
        private void BuildLeftPanel()
        {
            var pnlLeft = new Panel
            {
                Location = new Point(0, 72),
                Size = new Size(LeftPanelW, 508),
                BackColor = UITheme.BgCard,
                Padding = new Padding(UITheme.PaddingMd)
            };

            // Gạch dọc phân cách phải
            var sepRight = new Label
            {
                Location = new Point(LeftPanelW - 1, 72),
                Size = new Size(1, 508),
                BackColor = UITheme.Border
            };

            var lblPlayersTitle = new Label
            {
                Text = "Người chơi",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.Accent,
                AutoSize = true,
                Location = new Point(UITheme.PaddingMd, 16)
            };

            var sepLine = UITheme.MakeSeparator(248);
            sepLine.Location = new Point(UITheme.PaddingMd, 42);

            lstPlayers = new ListBox
            {
                Location = new Point(UITheme.PaddingMd, 52),
                Size = new Size(248, 260),
                Font = UITheme.FontBody,
                BackColor = UITheme.BgCard,
                ForeColor = UITheme.TextPrimary,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 40,
                SelectionMode = SelectionMode.None
            };
            lstPlayers.DrawItem += LstPlayers_DrawItem;

            // ── Buttons ──────────────────────────────────────────────
            int btnY = 328;

            btnReady = UITheme.MakeSuccessButton("✓  Sẵn sàng", 248, 42);
            btnReady.Location = new Point(UITheme.PaddingMd, btnY);
            btnReady.Click += async (s, e) => await ToggleReadyAsync();

            btnStart = UITheme.MakeSuccessButton("▶  Bắt đầu trận đấu", 248, 42);
            btnStart.Location = new Point(UITheme.PaddingMd, btnY + 52);
            btnStart.Enabled = false;
            btnStart.Visible = false;
            btnStart.Click += BtnStart_Click;

            btnLeave = UITheme.MakeDangerButton("🚪  Rời phòng", 248, 42);
            btnLeave.Location = new Point(UITheme.PaddingMd, btnY + 104);
            btnLeave.Click += BtnLeave_Click;

            pnlLeft.Controls.Add(lblPlayersTitle);
            pnlLeft.Controls.Add(sepLine);
            pnlLeft.Controls.Add(lstPlayers);
            pnlLeft.Controls.Add(btnReady);
            pnlLeft.Controls.Add(btnStart);
            pnlLeft.Controls.Add(btnLeave);

            Controls.Add(sepRight);
            Controls.Add(pnlLeft);
        }

        /// <summary>Custom draw người chơi trong ListBox.</summary>
        private void LstPlayers_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            Color bg = e.Index % 2 == 0 ? UITheme.BgCard : UITheme.BgElevated;
            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);

            string text = lstPlayers.Items[e.Index]?.ToString() ?? "";
            bool ready = text.Contains("[Sẵn sàng]");
            Color dotColor = ready ? UITheme.Success : UITheme.Warning;

            int dotSize = 10;
            int dotY = e.Bounds.Y + (e.Bounds.Height - dotSize) / 2;
            e.Graphics.FillEllipse(new SolidBrush(dotColor), new Rectangle(e.Bounds.X + 8, dotY, dotSize, dotSize));

            using var font = UITheme.FontBody;
            e.Graphics.DrawString(text, font, new SolidBrush(UITheme.TextPrimary),
                new RectangleF(e.Bounds.X + 26, e.Bounds.Y + 10, e.Bounds.Width - 30, e.Bounds.Height));

            e.Graphics.DrawLine(new Pen(UITheme.BorderSub), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        // ════════════════════════════════════════════════════════
        //  RIGHT PANEL – Chat
        // ════════════════════════════════════════════════════════
        private void BuildRightPanel()
        {
            int rightX = LeftPanelW;
            int rightW = 860 - LeftPanelW;

            var pnlRight = new Panel
            {
                Location = new Point(rightX, 72),
                Size = new Size(rightW, 508),
                BackColor = UITheme.BgMain,
                Padding = new Padding(UITheme.PaddingMd)
            };

            var lblChatTitle = new Label
            {
                Text = "💬  Trò chuyện",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.Accent,
                AutoSize = true,
                Location = new Point(UITheme.PaddingMd, 16)
            };

            var sepLine = UITheme.MakeSeparator(rightW - 2 * UITheme.PaddingMd);
            sepLine.Location = new Point(UITheme.PaddingMd, 42);

            // TextBox chat
            txtChat = new TextBox
            {
                Location = new Point(UITheme.PaddingMd, 52),
                Size = new Size(rightW - 2 * UITheme.PaddingMd, 358),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = UITheme.BgCard,
                ForeColor = UITheme.TextSecondary,
                Font = UITheme.FontBody,
                BorderStyle = BorderStyle.None,
                Text = "Hệ thống: Đã vào phòng chờ.\r\n"
            };

            // Input + nút gửi
            int inputY = 422;
            txtInputChat = new TextBox
            {
                Location = new Point(UITheme.PaddingMd, inputY),
                Size = new Size(rightW - 2 * UITheme.PaddingMd - 90 - 8, 38),
                Font = UITheme.FontBody,
                BackColor = UITheme.BgElevated,
                ForeColor = UITheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtInputChat.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    BtnSend_Click(this, EventArgs.Empty);
                }
            };

            btnSend = UITheme.MakeButton("Gửi ➤", 86, 38);
            btnSend.Location = new Point(txtInputChat.Right + 8, inputY);
            btnSend.Click += BtnSend_Click;

            pnlRight.Controls.Add(lblChatTitle);
            pnlRight.Controls.Add(sepLine);
            pnlRight.Controls.Add(txtChat);
            pnlRight.Controls.Add(txtInputChat);
            pnlRight.Controls.Add(btnSend);

            Controls.Add(pnlRight);
        }

        // ════════════════════════════════════════════════════════
        //  PACKET HANDLER
        // ════════════════════════════════════════════════════════
        private void AppSession_PacketReceived(BasePacket basePacket, string rawJson)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                if (basePacket.PacketType == "ROOM_UPDATE")
                {
                    var update = System.Text.Json.JsonSerializer.Deserialize<RoomUpdatePacket>(rawJson);
                    if (update != null)
                    {
                        lstPlayers.Items.Clear();
                        foreach (var player in update.Players)
                        {
                            string role = (player == update.HostUsername) ? " 👑" : "";
                            bool playerReady = update.ReadyStates.GetValueOrDefault(player, false);
                            string readyText = playerReady ? " [Sẵn sàng]" : " [Chưa]";
                            lstPlayers.Items.Add(player + role + readyText);
                        }

                        isHost = (update.HostUsername == AppSession.CurrentUsername);
                        btnStart.Visible = isHost;

                        bool amIReady = update.ReadyStates.GetValueOrDefault(AppSession.CurrentUsername, false);
                        isReady = amIReady;
                        if (amIReady)
                        {
                            btnReady.Text = "✗  Hủy sẵn sàng";
                            // Đổi sang outline style
                            btnReady.BackColor = UITheme.BgElevated;
                            btnReady.ForeColor = UITheme.Accent;
                            btnReady.FlatAppearance.BorderSize = 1;
                            btnReady.FlatAppearance.BorderColor = UITheme.Accent;
                        }
                        else
                        {
                            btnReady.Text = "✓  Sẵn sàng";
                            btnReady.BackColor = UITheme.Success;
                            btnReady.ForeColor = Color.White;
                            btnReady.FlatAppearance.BorderSize = 0;
                        }

                        if (isHost)
                        {
                            bool allReady = update.Players.All(p => update.ReadyStates.GetValueOrDefault(p, false));
                            btnStart.Enabled = (update.Players.Count >= 2 && allReady);
                            btnStart.BackColor = btnStart.Enabled ? UITheme.Success : UITheme.BgElevated;
                            btnStart.ForeColor = btnStart.Enabled ? Color.White : UITheme.TextMuted;
                        }
                    }
                }
                else if (basePacket.PacketType == "CHAT" || basePacket.PacketType == "ROOM_SYSTEM")
                {
                    var chat = System.Text.Json.JsonSerializer.Deserialize<ChatPacket>(rawJson);
                    if (chat != null)
                    {
                        string sender = basePacket.PacketType == "ROOM_SYSTEM" ? "🔔 Hệ thống" : chat.Sender;
                        string timestamp = string.IsNullOrEmpty(chat.Timestamp) ? DateTime.Now.ToString("HH:mm:ss") : chat.Timestamp;
                        txtChat.AppendText($"[{timestamp}] {sender}: {chat.Content}\r\n");
                    }
                }
                else if (basePacket.PacketType == "GAME_START")
                {
                    var startPacket = System.Text.Json.JsonSerializer.Deserialize<GameStartPacket>(rawJson);
                    if (startPacket != null)
                    {
                        AppSession.PacketReceived -= AppSession_PacketReceived;
                        MainMenuForm.Instance?.ShowFormInPanel(new Forms.MultiplayerGameForm(startPacket));
                    }
                }
                else if (basePacket.PacketType == "START_GAME_RESULT")
                {
                    if (!basePacket.Success)
                    {
                        MessageBox.Show(basePacket.Message, "Lỗi bắt đầu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            });
        }

        // ════════════════════════════════════════════════════════
        //  BUTTON HANDLERS
        // ════════════════════════════════════════════════════════
        private async Task ToggleReadyAsync()
        {
            btnReady.Enabled = false;
            try
            {
                await AppSession.SendPacketAsync(new BasePacket { PacketType = "TOGGLE_READY" });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnReady.Enabled = true;
            }
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            string content = txtInputChat.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            txtInputChat.Clear();
            try
            {
                var chatPacket = new ChatPacket { Content = content };
                await AppSession.SendPacketAsync(chatPacket);
            }
            catch (Exception ex)
            {
                txtChat.AppendText($"[Lỗi mạng] Không gửi được tin nhắn: {ex.Message}\r\n");
            }
        }

        private async void BtnStart_Click(object? sender, EventArgs e)
        {
            btnStart.Enabled = false;
            try
            {
                var request = new BasePacket { PacketType = "START_GAME" };
                await AppSession.SendPacketAsync(request);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối bắt đầu trận đấu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnStart.Enabled = true;
            }
        }

        private async void BtnLeave_Click(object? sender, EventArgs e)
        {
            btnLeave.Enabled = false;
            try
            {
                var request = new LeaveRoomPacket { RoomId = roomId };
                await AppSession.SendPacketAsync(request);

                AppSession.PacketReceived -= AppSession_PacketReceived;
                MainMenuForm.Instance?.ShowFormInPanel(new Forms.LobbyForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi rời phòng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLeave.Enabled = true;
            }
        }
    }
}
