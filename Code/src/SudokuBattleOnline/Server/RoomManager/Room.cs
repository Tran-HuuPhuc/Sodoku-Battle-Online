using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using SudokuBattle.Server.Network;
using SudokuBattleOnline.Shared.Packets;

namespace SudokuBattle.Server.Rooms
{
    public class Room
    {
        public string Id { get; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; } = 2; // Mặc định game Sudoku là 2 người đấu với nhau
        public bool IsGameStarted { get; set; } = false;

        // Lưu trữ danh sách kết nối thực tế của các Client trong phòng
        private readonly ConcurrentDictionary<string, ClientSession> _members = new();
        private readonly object _sync = new();

        // Trả về mảng danh sách người chơi hiện tại
        public ClientSession[] Members => _members.Values.ToArray();

        // Xác định ai là chủ phòng (lấy người đầu tiên tham gia)
        public ClientSession Host { get; private set; }

        public Room(string id, string name, int maxPlayers = 2)
        {
            Id = id;
            Name = name;
            MaxPlayers = maxPlayers;
        }

        public (bool, string) AddMember(ClientSession session)
        {
            if (session == null) return (false, "Session is null.");
            if (!session.IsConnected) return (false, "Kết nối không còn hoạt động.");
            if (!session.IsAuthenticated) return (false, "Bạn cần đăng nhập trước khi tham gia phòng.");

            lock (_sync)
            {
                if (IsGameStarted) return (false, "Trận đấu đã bắt đầu.");
                if (_members.Count >= MaxPlayers) return (false, "Phòng đã đầy.");
                if (_members.ContainsKey(session.SessionId)) return (false, "Bạn đã ở trong phòng này.");

                if (_members.TryAdd(session.SessionId, session))
                {
                    session.CurrentRoomId = Id;

                    // Đăng ký event để tự remove khi disconnect
                    session.OnDisconnected += OnSessionDisconnected;

                    if (Host == null)
                        Host = session;

                    Console.WriteLine($"[ROOM {Id}] {session} đã tham gia. Thành viên: {_members.Count}/{MaxPlayers}");
                    return (true, "Tham gia phòng thành công.");
                }

                return (false, "Không thể thêm vào phòng.");
            }
        }

        public (bool, string) RemoveMember(ClientSession session)
        {
            if (session == null) return (false, "Session is null.");

            lock (_sync)
            {
                if (_members.TryRemove(session.SessionId, out _))
                {
                    // Unsubscribe để tránh giữ reference
                    session.OnDisconnected -= OnSessionDisconnected;

                    session.CurrentRoomId = null;

                    if (Host?.SessionId == session.SessionId)
                        Host = _members.Values.FirstOrDefault();

                    if (_members.IsEmpty)
                        Host = null;

                    Console.WriteLine($"[ROOM {Id}] {session} đã rời. Thành viên: {_members.Count}/{MaxPlayers}");
                    return (true, "Rời phòng thành công.");
                }

                return (false, "Không thấy thành viên trong phòng.");
            }
        }

        private void OnSessionDisconnected(ClientSession session)
        {
            try
            {
                RemoveMember(session);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ROOM {Id}] Lỗi khi xử lý disconnect: {ex.Message}");
            }
        }

        // Gửi gói tin tới tất cả thành viên trong phòng
        public async Task BroadcastAsync(BasePacket packet)
        {
            var targets = Members;
            foreach (var s in targets)
            {
                if (s == null) continue;
                if (!s.IsConnected) continue;

                try
                {
                    await s.SendPacketAsync(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ROOM {Id}] Lỗi gửi packet tới {s}: {ex.Message}");
                }
            }
        }
        public async Task BroadcastExceptAsync(BasePacket packet, string exceptSessionId)
        {
            var targets = Members;
            foreach (var s in targets)
            {
                if (s == null || s.SessionId == exceptSessionId) continue;
                if (!s.IsConnected) continue;

                try
                {
                    await s.SendPacketAsync(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ROOM {Id}] Lỗi gửi packet tới {s}: {ex.Message}");
                }
            }
        }

        public ClientSession? FindMemberByUsername(string username)
        {
            return _members.Values.FirstOrDefault(s => string.Equals(s.Username, username, StringComparison.OrdinalIgnoreCase));
        }
    }

}
