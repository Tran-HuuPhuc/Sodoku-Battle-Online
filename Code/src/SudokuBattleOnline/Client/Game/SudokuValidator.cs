using System.Collections.Generic;

namespace Client.Game
{
    /// <summary>
    /// </summary>
    public static class SudokuValidator
    {
        public static bool IsValid(int[,] board)
        {
            if (board == null || board.GetLength(0) != 9 || board.GetLength(1) != 9) 
                return false;

            for (int i = 0; i < 9; i++)
            {
                if (!IsValidSet(board, i, i, 0, 8)) return false; // Kiểm tra dòng i
                if (!IsValidSet(board, 0, 8, i, i)) return false; // Kiểm tra cột i
            }

            // Kiểm tra 9 khối 3x3
            for (int r = 0; r < 9; r += 3)
            {
                for (int c = 0; c < 9; c += 3)
                {
                    if (!IsValidSet(board, r, r + 2, c, c + 2)) return false;
                }
            }

            return true;
        }

        public static bool IsSolved(int[,] board)
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (board[r, c] == 0) return false; // còn ô => chưa win
                }
            }
            // thắng nếu điền kín, không sai
            return IsValid(board);
        }

        private static bool IsValidSet(int[,] board, int rowStart, int rowEnd, int colStart, int colEnd)
        {
            HashSet<int> seen = new HashSet<int>();
            for (int r = rowStart; r <= rowEnd; r++)
            {
                for (int c = colStart; c <= colEnd; c++)
                {
                    int val = board[r, c];
                    if (val != 0) 
                    {
                        if (seen.Contains(val)) return false; 
                        seen.Add(val);
                    }
                }
            }
            return true;
        }
    }
}
