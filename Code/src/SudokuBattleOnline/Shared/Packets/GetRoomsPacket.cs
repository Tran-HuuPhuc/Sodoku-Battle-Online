using System.Collections.Generic;

namespace SudokuBattleOnline.Shared.Packets
{
    public class GetRoomsPacket : BasePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<RoomInfoData> Rooms { get; set; } = new();

        public GetRoomsPacket()
        {
            PacketType = "GET_ROOMS";
        }
    }

    public class RoomInfoData
    {
        public string RoomId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsStarted { get; set; }
    }
}
