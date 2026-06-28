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
        public int MaxPlayers { get; set; } = 2;
        public bool IsGameStarted { get; set; } = false;

        private readonly ConcurrentDictionary<string, ClientSession> _members = new();
        private readonly object _sync = new();

        public ClientSession[] Members => _members.Values.ToArray();

        public ClientSession Host { get; private set; }

        public event Action<Room>? OnRoomEmpty;


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
                var(removed, _) = RemoveMember(session);
                if (!removed) return;

                if (_members.IsEmpty)
                {
                    OnRoomEmpty?.Invoke(this);
                }
                else
                {
                    Task.Run(() => BroadcastRoomUpdateAsync());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ROOM {Id}] Lỗi khi xử lý disconnect: {ex.Message}");
            }
        }

        public async Task BroadcastRoomUpdateAsync()
        {
            var packet = new RoomUpdatePacket
            {
                RoomId = Id,
                RoomName = Name,
                Members = Members.Select(m => m.Username ?? m.SessionId).ToList(),
                HostUsername = Host?.Username ?? string.Empty,
                MaxPlayers = MaxPlayers,
                IsGameStarted = IsGameStarted
            };

            await BroadcastAsync(packet);
        }

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
