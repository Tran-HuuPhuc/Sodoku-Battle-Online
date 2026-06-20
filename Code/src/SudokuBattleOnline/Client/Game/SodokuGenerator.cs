using Shared.Enums;
using SudokuBattle.Shared.Enums;

namespace SudokuBattle.Client.Game
{
    public class SudokuGenerator
    {
        private const int Size = 9;
        private readonly Random random = new();

        public int[,] GenerateBoard(Difficulty difficulty)
        {
            int[,] board = new int[Size, Size];

            GenerateFullBoard(board);

            RemoveCells(board, difficulty);

            return board;
        }

        private void GenerateFullBoard(int[,] board)
        {
            FillBoard(board, 0, 0);
        }

        private bool FillBoard(int[,] board, int row, int col)
        {
            if (row == Size)
                return true;

            int nextRow = (col == Size - 1) ? row + 1 : row;
            int nextCol = (col == Size - 1) ? 0 : col + 1;

            List<int> numbers = ShuffleNumbers();

            foreach (int number in numbers)
            {
                if (IsSafe(board, row, col, number))
                {
                    board[row, col] = number;

                    if (FillBoard(board, nextRow, nextCol))
                        return true;

                    board[row, col] = 0;
                }
            }

            return false;
        }

        private bool IsSafe(int[,] board, int row, int col, int value)
        {
            for (int i = 0; i < Size; i++)
            {
                if (board[row, i] == value)
                    return false;

                if (board[i, col] == value)
                    return false;
            }

            int startRow = row / 3 * 3;
            int startCol = col / 3 * 3;

            for (int r = startRow; r < startRow + 3; r++)
            {
                for (int c = startCol; c < startCol + 3; c++)
                {
                    if (board[r, c] == value)
                        return false;
                }
            }

            return true;
        }

        private void RemoveCells(int[,] board, Difficulty difficulty)
        {
            int cellsToRemove = CountCellsToRemove(difficulty);

            while (cellsToRemove > 0)
            {
                int row = random.Next(Size);
                int col = random.Next(Size);

                if (board[row, col] != 0)
                {
                    board[row, col] = 0;
                    cellsToRemove--;
                }
            }
        }

        private int CountCellsToRemove(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => 35,
                Difficulty.Medium => 45,
                Difficulty.Hard => 55,
                _ => 40
            };
        }

        private List<int> ShuffleNumbers()
        {
            List<int> numbers = Enumerable.Range(1, 9).ToList();

            for (int i = numbers.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);

                (numbers[i], numbers[j]) = (numbers[j], numbers[i]);
            }

            return numbers;
        }
    }
}
