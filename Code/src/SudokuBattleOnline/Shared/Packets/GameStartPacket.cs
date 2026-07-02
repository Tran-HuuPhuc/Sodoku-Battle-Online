namespace SudokuBattleOnline.Shared.Packets
{
    /// <summary>
    /// Gói tin Server gửi cho cả 2 Client khi trận đấu bắt đầu.
    /// Chứa đề bài Sudoku và thông tin cấu hình trận đấu.
    /// </summary>
    public class GameStartPacket : BasePacket
    {
        /// <summary>Mã phòng đấu.</summary>
        public string RoomId { get; set; } = string.Empty;

        /// <summary>
        /// Bảng Sudoku đề bài (mảng 1 chiều 81 phần tử, ô trống = 0).
        /// Dùng mảng 1 chiều để dễ serialize JSON hơn mảng 2 chiều.
        /// </summary>
        public int[] Board { get; set; } = new int[81];

        /// <summary>Thời gian giới hạn trận đấu (tính bằng giây).</summary>
        public int TimeLimitSeconds { get; set; } = 300;

        /// <summary>Tên đăng nhập đối thủ.</summary>
        public string OpponentUsername { get; set; } = string.Empty;

        /// <summary>Tên đăng nhập của Player 1 (chủ phòng / người thứ nhất).</summary>
        public string Player1Username { get; set; } = string.Empty;

        public GameStartPacket()
        {
            PacketType = "GAME_START";
        }
    }
}
