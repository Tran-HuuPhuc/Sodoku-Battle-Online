namespace SudokuBattleOnline.Shared.Packets
{
    public class CreateRoomPacket : BasePacket
    {
        public string RoomName { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;

        public CreateRoomPacket()
        {
            PacketType = "CREATE_ROOM";
        }
    }
}
