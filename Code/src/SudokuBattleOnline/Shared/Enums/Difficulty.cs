
using Shared.Constants;

namespace Shared.Enums
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    /// <summary>
    /// Các tiện ích mở rộng cho enum Difficulty.
    /// </summary>
    public static class DifficultyExtensions
    {
        /// <summary>Số ô đã cho (clues) để lại trên bảng.</summary>
        public static int GetClueCount(this Difficulty d) => d switch
        {
            Difficulty.Easy => GameConstants.EASY_CLUES,
            Difficulty.Medium => GameConstants.MEDIUM_CLUES,
            Difficulty.Hard => GameConstants.HARD_CLUES,
            _ => GameConstants.MEDIUM_CLUES
        };

        /// <summary>Số ô ẩn đi (= 81 - clues).</summary>
        public static int GetHiddenCount(this Difficulty d) =>
            GameConstants.BOARD_SIZE * GameConstants.BOARD_SIZE - d.GetClueCount();

        /// <summary>Thời gian giới hạn tính bằng giây.</summary>
        public static int GetTimeLimitSeconds(this Difficulty d) => d switch
        {
            Difficulty.Easy => GameConstants.EASY_TIME_LIMIT,
            Difficulty.Medium => GameConstants.MEDIUM_TIME_LIMIT,
            Difficulty.Hard => GameConstants.HARD_TIME_LIMIT,
            _ => GameConstants.MEDIUM_TIME_LIMIT
        };

        /// <summary>ELO cộng khi thắng.</summary>
        public static int GetEloWin(this Difficulty d) => d switch
        {
            Difficulty.Easy => GameConstants.EASY_ELO_WIN,
            Difficulty.Medium => GameConstants.MEDIUM_ELO_WIN,
            Difficulty.Hard => GameConstants.HARD_ELO_WIN,
            _ => GameConstants.MEDIUM_ELO_WIN
        };

        /// <summary>ELO trừ khi thua (giá trị âm).</summary>
        public static int GetEloLose(this Difficulty d) => d switch
        {
            Difficulty.Easy => GameConstants.EASY_ELO_LOSE,
            Difficulty.Medium => GameConstants.MEDIUM_ELO_LOSE,
            Difficulty.Hard => GameConstants.HARD_ELO_LOSE,
            _ => GameConstants.MEDIUM_ELO_LOSE
        };

        /// <summary>Tên hiển thị tiếng Việt.</summary>
        public static string ToVietnamese(this Difficulty d) => d switch
        {
            Difficulty.Easy => "Dễ",
            Difficulty.Medium => "Trung Bình",
            Difficulty.Hard => "Khó",
            _ => "Trung Bình"
        };
    }
}
