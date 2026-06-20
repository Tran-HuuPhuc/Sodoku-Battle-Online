namespace Shared.Constants
{
    public static class GameConstants
    {
        public const int BOARD_SIZE = 9;
        public const int SUBGRID_SIZE = 3;
        public const int EMPTY_CELL = 0;

        // Số ô bị ẩn theo từng mức độ
        // Sudoku 9x9 có 81 ô; số ô đã cho (clues) tối thiểu để có đúng 1 nghiệm ~ 17
        public const int EASY_CLUES = 45;   // 36 ô ẩn – người chơi mới
        public const int MEDIUM_CLUES = 32;   // 49 ô ẩn – trung bình
        public const int HARD_CLUES = 26;   // 55 ô ẩn – thách thức

        // Thời gian giới hạn (giây)
        public const int EASY_TIME_LIMIT = 600;   // 10 phút
        public const int MEDIUM_TIME_LIMIT = 480;   //  8 phút
        public const int HARD_TIME_LIMIT = 360;   //  6 phút

        // ELO điểm thưởng / phạt theo độ khó
        public const int EASY_ELO_WIN = 8;
        public const int EASY_ELO_LOSE = -5;
        public const int MEDIUM_ELO_WIN = 15;
        public const int MEDIUM_ELO_LOSE = -10;
        public const int HARD_ELO_WIN = 25;
        public const int HARD_ELO_LOSE = -12;
    }
}
