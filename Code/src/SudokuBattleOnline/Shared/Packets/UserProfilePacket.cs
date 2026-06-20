namespace SudokuBattleOnline.Shared.Packets
{
    /// <summary>
    /// Client gửi PROFILE để yêu cầu hồ sơ người đang đăng nhập.
    /// Server trả PROFILE_RESULT với dữ liệu thật từ SQLite Server.
    /// </summary>
    public class UserProfilePacket : BasePacket
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Elo { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public string CreatedAt { get; set; } = string.Empty;

        public UserProfilePacket()
        {
            PacketType = "PROFILE";
        }
    }
}

