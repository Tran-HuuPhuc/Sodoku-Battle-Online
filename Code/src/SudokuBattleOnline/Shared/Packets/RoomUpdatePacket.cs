using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuBattleOnline.Shared.Packets
{
    public class RoomUpdatePacket : BasePacket
    {
        public string RoomId { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;

        public List<string> Members { get; set; } = new();

        public string HostUsername { get; set; } = string.Empty;

        public int MaxPlayers { get; set; }

        public bool IsGameStarted { get; set; }

        public RoomUpdatePacket()
        {
            PacketType = "ROOM_UPDATE";
        }
    }
}
