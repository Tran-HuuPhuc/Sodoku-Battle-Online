using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SudokuBattle.Server.Rooms
{
    public class RoomManager
    {
        private readonly ConcurrentDictionary<string, Room> _rooms = new();

        public Room CreateRoom(string name, int maxPlayers = 2)
        {
            var id = Guid.NewGuid().ToString("N")[..8];
            var room = new Room(id, name, maxPlayers);
            room.OnRoomEmpty += r =>
            {
                if (_rooms.TryRemove(r.Id, out _))
                    Console.WriteLine($"[ROOMMANAGER] Phòng {r.Id} '{r.Name}' đã trống, đã xóa.");
            };
            _rooms.TryAdd(id, room);
            Console.WriteLine($"[ROOMMANAGER] Tạo phòng {id} '{name}'");
            return room;
        }

        public bool TryGetRoom(string roomId, out Room? room)
        {
            return _rooms.TryGetValue(roomId, out room);
        }

        public bool RemoveRoom(string roomId)
        {
            if (_rooms.TryRemove(roomId, out var room))
            {
                Console.WriteLine($"[ROOMMANAGER] Xóa phòng {roomId} '{room?.Name}'.");
                return true;
            }
            return false;
        }

        public Room[] GetAllRooms() => _rooms.Values.ToArray();
    }
}