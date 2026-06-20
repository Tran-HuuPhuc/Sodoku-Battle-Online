namespace SudokuBattleOnline.Shared.Packets
{
    /// <summary>
    /// Client gửi kết quả trận/chơi đơn lên Server để Server lưu vào SQLite.
    /// </summary>
    public class SaveMatchResultPacket : BasePacket
    {
        public string Opponent { get; set; } = "Single Player";
        public string Result { get; set; } = "Completed";
        public string Difficulty { get; set; } = "Medium";
        public int Score { get; set; }
        public int TimeSeconds { get; set; }

        public SaveMatchResultPacket()
        {
            PacketType = "SAVE_MATCH_RESULT";
        }
    }
}
