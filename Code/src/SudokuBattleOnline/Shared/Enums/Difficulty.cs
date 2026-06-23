using Shared.Constants;

namespace Shared.Enums
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public static class DifficultyExtensions
    {
        public static int GetClueCount(this Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => GameConstants.EASY_CLUES,
                Difficulty.Medium => GameConstants.MEDIUM_CLUES,
                Difficulty.Hard => GameConstants.HARD_CLUES,
                _ => GameConstants.MEDIUM_CLUES
            };
        }

        public static int GetHiddenCount(this Difficulty difficulty)
        {
            return GameConstants.BOARD_SIZE * GameConstants.BOARD_SIZE
                   - difficulty.GetClueCount();
        }

        public static int GetTimeLimitSeconds(this Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => GameConstants.EASY_TIME_LIMIT,
                Difficulty.Medium => GameConstants.MEDIUM_TIME_LIMIT,
                Difficulty.Hard => GameConstants.HARD_TIME_LIMIT,
                _ => GameConstants.MEDIUM_TIME_LIMIT
            };
        }

        public static int GetEloWin(this Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => GameConstants.EASY_ELO_WIN,
                Difficulty.Medium => GameConstants.MEDIUM_ELO_WIN,
                Difficulty.Hard => GameConstants.HARD_ELO_WIN,
                _ => GameConstants.MEDIUM_ELO_WIN
            };
        }

        public static int GetEloLose(this Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => GameConstants.EASY_ELO_LOSE,
                Difficulty.Medium => GameConstants.MEDIUM_ELO_LOSE,
                Difficulty.Hard => GameConstants.HARD_ELO_LOSE,
                _ => GameConstants.MEDIUM_ELO_LOSE
            };
        }

        public static int GetEloChange(this Difficulty difficulty, bool isWin)
        {
            return isWin ? difficulty.GetEloWin() : difficulty.GetEloLose();
        }

        public static string ToVietnamese(this Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => "Dễ",
                Difficulty.Medium => "Trung bình",
                Difficulty.Hard => "Khó",
                _ => "Trung bình"
            };
        }

        public static Difficulty FromString(string value)
        {
            return value.Trim().ToLower() switch
            {
                "easy" => Difficulty.Easy,
                "dễ" => Difficulty.Easy,

                "medium" => Difficulty.Medium,
                "trung bình" => Difficulty.Medium,

                "hard" => Difficulty.Hard,
                "khó" => Difficulty.Hard,

                _ => Difficulty.Medium
            };
        }
    }
}