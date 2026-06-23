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
            return _rooms.TryRemove(roomId, out _);
        }

        public Room[] GetAllRooms() => _rooms.Values.ToArray();
    }
}