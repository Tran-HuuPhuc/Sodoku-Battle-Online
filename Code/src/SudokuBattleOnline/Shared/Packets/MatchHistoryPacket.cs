using System.Collections.Generic;

namespace SudokuBattleOnline.Shared.Packets
{
    /// <summary>
    /// Client gửi MATCH_HISTORY để lấy lịch sử đấu của user đang đăng nhập.
    /// Server trả MATCH_HISTORY_RESULT.
    /// </summary>
    public class MatchHistoryPacket : BasePacket
    {
        public List<MatchHistoryEntry> History { get; set; } = new List<MatchHistoryEntry>();

        public MatchHistoryPacket()
        {
            PacketType = "MATCH_HISTORY";
        }
    }

    public class MatchHistoryEntry
    {
        public int Id { get; set; }
        public string Player1 { get; set; } = string.Empty;
        public string Player2 { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public int EloChangeP1 { get; set; }
        public int EloChangeP2 { get; set; }
        public string PlayedAt { get; set; } = string.Empty;
    }
}
