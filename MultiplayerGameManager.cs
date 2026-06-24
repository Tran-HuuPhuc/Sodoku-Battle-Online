using Shared.Models;

namespace Server.GameManager
{
    public class MultiplayerGameManager
    {
        private readonly GameRoom _room;

        public MultiplayerGameManager(GameRoom room)
        {
            _room = room;
        }

        public void StartGame(int[,] initialBoard)
        {
            _room.GameState.Board = initialBoard;
            _room.GameState.Player1Progress = 0;
            _room.GameState.Player2Progress = 0;
            _room.GameState.IsFinished = false;

            _room.Player1.Score = 0;
            _room.Player2.Score = 0;

            _room.Player1.Mistakes = 0;
            _room.Player2.Mistakes = 0;

            _room.Player1.IsWinner = false;
            _room.Player2.IsWinner = false;

            _room.IsStarted = true;
        }

        public void UpdateCell(
            int row,
            int col,
            int value)
        {
            _room.GameState.Board[row, col] = value;
        }

        public void UpdatePlayerProgress(
            int playerNumber)
        {
            int filledCells = CountFilledCells();

            if (playerNumber == 1)
            {
                _room.GameState.Player1Progress = filledCells;
            }
            else
            {
                _room.GameState.Player2Progress = filledCells;
            }
        }

        public int CalculateProgressPercent(
            int playerNumber)
        {
            int progress =
                playerNumber == 1
                ? _room.GameState.Player1Progress
                : _room.GameState.Player2Progress;

            return (progress * 100) / 81;
        }

        public bool CheckGameFinished()
        {
            return _room.GameState.Player1Progress >= 81
                || _room.GameState.Player2Progress >= 81;
        }

        public void EndGame()
        {
            _room.GameState.IsFinished = true;

            DetermineWinner();
        }

        public string DetermineWinner()
        {
            int p1 = _room.GameState.Player1Progress;
            int p2 = _room.GameState.Player2Progress;

            if (p1 > p2)
            {
                _room.Player1.IsWinner = true;
                _room.Player2.IsWinner = false;

                return _room.Player1.Username;
            }

            if (p2 > p1)
            {
                _room.Player2.IsWinner = true;
                _room.Player1.IsWinner = false;

                return _room.Player2.Username;
            }

            _room.Player1.IsWinner = false;
            _room.Player2.IsWinner = false;

            return "Draw";
        }

        public void AddCorrectMove(
            int playerNumber)
        {
            if (playerNumber == 1)
            {
                _room.Player1.Score++;
                UpdatePlayerProgress(1);
            }
            else
            {
                _room.Player2.Score++;
                UpdatePlayerProgress(2);
            }
        }

        public void AddMistake(
            int playerNumber)
        {
            if (playerNumber == 1)
            {
                _room.Player1.Mistakes++;
            }
            else
            {
                _room.Player2.Mistakes++;
            }
        }

        private int CountFilledCells()
        {
            int count = 0;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (_room.GameState.Board[i, j] != 0)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}