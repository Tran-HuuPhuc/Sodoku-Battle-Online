using Shared.Models;

namespace Server.GameManager
{
    public class GameRoom
    {
        public string RoomId { get; set; }

        public Player Player1 { get; set; }

        public Player Player2 { get; set; }

        public GameState GameState { get; set; }

        public bool IsStarted { get; set; }

        public GameRoom()
        {
            GameState = new GameState();
        }
    }
}