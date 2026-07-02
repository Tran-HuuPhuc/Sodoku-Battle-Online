using SudokuBattleOnline.Client;
using SudokuBattleOnline.Shared.Packets;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace SudokuBattleOnline.Forms
{
    public class LobbyForm : Form
    {
        private ListBox lstRooms;
        private System.Windows.Forms.Timer refreshTimer;
        private Button btnCreate;
        private Button btnJoin;
        private Label lblStatus;
        private Button btnRefresh;

        // Lưu danh sách phòng để lấy RoomId khi click chọn
        private System.Collections.Generic.List<RoomInfoData> activeRooms = new();

        public LobbyForm()
        {
            Text = "Sudoku Battle – Lobby";
            Size = new Size(860, 560);
            BackColor = UITheme.BgMain;
            FormBorderStyle = FormBorderStyle.None;

            BuildUI();

            btnCreate.Click += BtnCreate_Click;
            btnJoin.Click += BtnJoin_Click;
            btnRefresh.Click += async (s, e) => await LoadRoomsFromServerAsync();

            // Khi double-click vào phòng → tham gia luôn
            lstRooms.DoubleClick += BtnJoin_Click;

            // Timer tự động làm mới danh sách phòng mỗi 3 giây
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 3000;
            refreshTimer.Tick += async (s, e) => await LoadRoomsFromServerAsync();

            this.Load += async (s, e) =>
            {
                await LoadRoomsFromServerAsync();
                refreshTimer.Start();
            };

            this.FormClosed += (s, e) =>
            {
                refreshTimer.Stop();
                refreshTimer.Dispose();
            };
        }

        private void BuildUI()
        {
            // ── Header ──────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = UITheme.BgDeep,
                Padding = new Padding(UITheme.PaddingMd, 0, UITheme.PaddingMd, 0)
            };

            var lblTitle = new Label
            {
                Text = "🎮  Lobby – Phòng chờ",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.Accent,
                AutoSize = true,
                Location = new Point(24, 22)
            };

            var lblSeparatorBottom = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = UITheme.Border
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSeparatorBottom);

            // ── Body ─────────────────────────────────────────────────
            var pnlBody = new Panel
            {
                Location = new Point(0, 72),
                Size = new Size(860, 488),
                BackColor = UITheme.BgMain,
                Padding = new Padding(UITheme.PaddingMd)
            };

            // ── Hai nút lớn ─────────────────────────────────────────
            btnCreate = UITheme.MakeSuccessButton("＋  Tạo phòng mới", 380, 55);
            btnCreate.Location = new Point(24, 20);
            btnCreate.Font = UITheme.FontSubtitle;

            btnJoin = UITheme.MakeButton("🔍  Tham gia phòng", 380, 55);
            btnJoin.Location = new Point(432, 20);
            btnJoin.Font = UITheme.FontSubtitle;

            // ── Separator ────────────────────────────────────────────
            var sep = UITheme.MakeSeparator(812);
            sep.Location = new Point(24, 92);

            // ── Row header danh sách phòng ──────────────────────────
            var lblListHeader = new Label
            {
                Text = "Phòng đang chờ:",
                Font = UITheme.FontBodyBold,
                ForeColor = UITheme.TextPrimary,
                AutoSize = true,
                Location = new Point(24, 108)
            };

            btnRefresh = UITheme.MakeOutlineButton("↻ Làm mới", 110, 30);
            btnRefresh.Location = new Point(702, 102);
            btnRefresh.Font = UITheme.FontSmall;

            // ── ListBox phòng ────────────────────────────────────────
            lstRooms = new ListBox
            {
                Location = new Point(24, 140),
                Size = new Size(812, 270),
                Font = UITheme.FontBody,
                BackColor = UITheme.BgCard,
                ForeColor = UITheme.TextPrimary,
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 36,
                SelectionMode = SelectionMode.One
            };
            lstRooms.DrawItem += LstRooms_DrawItem;

            // ── Status bar ───────────────────────────────────────────
            lblStatus = new Label
            {
                Text = "Đang tải danh sách phòng...",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                AutoSize = true,
                Location = new Point(24, 420)
            };

            pnlBody.Controls.Add(btnCreate);
            pnlBody.Controls.Add(btnJoin);
            pnlBody.Controls.Add(sep);
            pnlBody.Controls.Add(lblListHeader);
            pnlBody.Controls.Add(btnRefresh);
            pnlBody.Controls.Add(lstRooms);
            pnlBody.Controls.Add(lblStatus);

            Controls.Add(pnlHeader);
            Controls.Add(pnlBody);
        }

        /// <summary>Custom draw mỗi item trong ListBox phòng.</summary>
        private void LstRooms_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool selected = (e.State & DrawItemState.Selected) != 0;
            Color bg = selected ? UITheme.BgHover : (e.Index % 2 == 0 ? UITheme.BgCard : UITheme.BgElevated);
            e.Graphics.FillRectangle(new SolidBrush(bg), e.Bounds);

            string text = lstRooms.Items[e.Index]?.ToString() ?? "";
            bool isWaiting = text.Contains("Đang chờ");
            Color statusColor = isWaiting ? UITheme.Success : UITheme.Warning;
            Color textColor = selected ? UITheme.TextPrimary : UITheme.TextSecondary;

            // Vẽ chấm tròn trạng thái
            int dotSize = 10;
            int dotY = e.Bounds.Y + (e.Bounds.Height - dotSize) / 2;
            e.Graphics.FillEllipse(new SolidBrush(statusColor), new Rectangle(e.Bounds.X + 12, dotY, dotSize, dotSize));

            using var font = UITheme.FontBody;
            e.Graphics.DrawString(text, font, new SolidBrush(textColor), new RectangleF(e.Bounds.X + 32, e.Bounds.Y + 8, e.Bounds.Width - 36, e.Bounds.Height));

            // Gạch dưới mỏng
            e.Graphics.DrawLine(new Pen(UITheme.BorderSub), e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private async System.Threading.Tasks.Task LoadRoomsFromServerAsync()
        {
            if (!AppSession.IsConnected) return;

            try
            {
                var request = new GetRoomsPacket();
                var response = await AppSession.SendAndWaitAsync<GetRoomsPacket>(request, "GET_ROOMS", 4000);

                if (response != null && response.Success)
                {
                    activeRooms = response.Rooms;
                    lstRooms.Items.Clear();

                    foreach (var room in activeRooms)
                    {
                        string status = room.IsStarted ? "Đang chơi" : "Đang chờ";
                        lstRooms.Items.Add($"{room.RoomName}  [{room.PlayerCount}/{room.MaxPlayers}]  –  {status}");
                    }

                    lblStatus.Text = $"Cập nhật lúc: {DateTime.Now:HH:mm:ss}  ·  Tìm thấy {activeRooms.Count} phòng";
                }
                else
                {
                    lblStatus.Text = "Không nhận được danh sách phòng hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi cập nhật phòng: " + ex.Message;
            }
        }

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            btnCreate.Enabled = false;
            try
            {
                var request = new CreateRoomPacket
                {
                    RoomName = $"Phòng của {AppSession.CurrentUsername}"
                };

                var response = await AppSession.SendAndWaitAsync<CreateRoomPacket>(request, "CREATE_ROOM_RESULT", 6000);
                if (response != null && response.Success)
                {
                    refreshTimer.Stop();
                    var roomForm = new RoomForm(response.RoomId);
                    MainMenuForm.Instance?.ShowFormInPanel(roomForm);
                }
                else
                {
                    MessageBox.Show(response?.Message ?? "Tạo phòng thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối tạo phòng: " + ex.Message, "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCreate.Enabled = true;
            }
        }

        private async void BtnJoin_Click(object? sender, EventArgs e)
        {
            int selectedIndex = lstRooms.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= activeRooms.Count)
            {
                MessageBox.Show("Vui lòng chọn phòng từ danh sách để tham gia.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRoom = activeRooms[selectedIndex];
            if (selectedRoom.IsStarted)
            {
                MessageBox.Show("Trận đấu trong phòng này đã bắt đầu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedRoom.PlayerCount >= selectedRoom.MaxPlayers)
            {
                MessageBox.Show("Phòng đấu đã đầy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnJoin.Enabled = false;
            try
            {
                var request = new JoinRoomPacket
                {
                    RoomId = selectedRoom.RoomId
                };

                var response = await AppSession.SendAndWaitAsync<JoinRoomPacket>(request, "JOIN_ROOM_RESULT", 6000);
                if (response != null && response.Success)
                {
                    refreshTimer.Stop();
                    var roomForm = new RoomForm(response.RoomId);
                    MainMenuForm.Instance?.ShowFormInPanel(roomForm);
                }
                else
                {
                    MessageBox.Show(response?.Message ?? "Không thể vào phòng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối vào phòng: " + ex.Message, "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnJoin.Enabled = true;
            }
        }
    }
}