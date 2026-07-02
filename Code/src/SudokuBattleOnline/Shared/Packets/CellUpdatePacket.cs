namespace SudokuBattleOnline.Shared.Packets
{
    public class CellUpdatePacket : BasePacket
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public int Value { get; set; }

        // Các trường bổ sung để đồng bộ hoá tiến trình và trạng thái ô cờ
        public string Username { get; set; } = string.Empty;
        public bool Success { get; set; }
        public bool IsCorrect { get; set; }
        public int Player1Progress { get; set; }
        public int Player2Progress { get; set; }
        public int Player1Mistakes { get; set; }
        public int Player2Mistakes { get; set; }

        public CellUpdatePacket()
        {
            PacketType = "CELL_UPDATE";
        }
    }
}
