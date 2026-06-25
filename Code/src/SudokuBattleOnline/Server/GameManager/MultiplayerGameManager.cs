using System.Collections.Generic;

namespace SudokuBattleOnline.Server.GameManager
{
    public class MultiplayerGameManager
    {
        private readonly Dictionary<string, GameRoom> rooms =
            new Dictionary<string, GameRoom>();

        public void CreateRoom(string roomId)
        {
            GameRoom room = new GameRoom(roomId);

            room.TimeUp += () =>
            {
                EndMatch(roomId);
            };

            rooms.Add(roomId, room);

            room.StartTimer();
        }

        public GameRoom? GetRoom(string roomId)
        {
            rooms.TryGetValue(roomId, out GameRoom room);

            return room;
        }

        public void EndMatch(string roomId)
        {
            if (!rooms.ContainsKey(roomId))
                return;

            rooms[roomId].StopTimer();

            
        }
    }
}