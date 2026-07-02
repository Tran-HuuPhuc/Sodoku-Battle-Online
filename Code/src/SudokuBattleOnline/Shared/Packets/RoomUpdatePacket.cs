using System.Collections.Generic;

namespace SudokuBattleOnline.Shared.Packets
{
    public class RoomUpdatePacket : BasePacket
    {
        public string RoomId { get; set; } = string.Empty;
        public string HostUsername { get; set; } = string.Empty;
        public string GuestUsername { get; set; } = string.Empty;
        public List<string> Players { get; set; } = new();
        public Dictionary<string, bool> ReadyStates { get; set; } = new();

        public RoomUpdatePacket()
        {
            PacketType = "ROOM_UPDATE";
        }
    }
}
