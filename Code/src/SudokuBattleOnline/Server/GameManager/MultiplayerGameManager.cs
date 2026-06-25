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

        public void StartGame(
            int[,] puzzleBoard,
            int[,] solutionBoard)
        {
            _room.PuzzleBoard = puzzleBoard;
            _room.SolutionBoard = solutionBoard;

            _room.GameState.Board =
                (int[,])puzzleBoard.Clone();

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

        /// <summary>
        /// Người chơi nhập số
        /// Tự động kiểm tra đúng/sai
        /// </summary>
        public bool SubmitMove(
            int playerNumber,
            int row,
            int col,
            int value)
        {
            // Không cho sửa ô gốc

            if (_room.PuzzleBoard[row, col] != 0)
                return false;

            bool isCorrect =
                _room.SolutionBoard[row, col] == value;

            if (isCorrect)
            {
                _room.GameState.Board[row, col] = value;

                AddCorrectMove(playerNumber);

                CheckAndFinishGame();

                return true;
            }

            AddMistake(playerNumber);

            return false;
        }

        private void AddCorrectMove(int playerNumber)
        {
            if (playerNumber == 1)
            {
                _room.Player1.Score++;
                _room.GameState.Player1Progress =
                    CalculateProgress(_room.Player1.Score);
            }
            else
            {
                _room.Player2.Score++;
                _room.GameState.Player2Progress =
                    CalculateProgress(_room.Player2.Score);
            }
        }

        private void AddMistake(int playerNumber)
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

        private int CalculateProgress(int score)
        {
            return (score * 100) / 81;
        }

        public bool IsGameFinished()
        {
            return _room.GameState.IsFinished;
        }

        private void CheckAndFinishGame()
        {
            if (_room.Player1.Score >= 81
                || _room.Player2.Score >= 81)
            {
                EndGame();
            }
        }

        public void EndGame()
        {
            _room.GameState.IsFinished = true;

            Player winner =
                ResultCalculator.DetermineWinner(
                    _room.Player1,
                    _room.Player2);

            if (winner == null)
            {
                _room.Player1.IsWinner = false;
                _room.Player2.IsWinner = false;
                return;
            }

            _room.Player1.IsWinner =
                winner == _room.Player1;

            _room.Player2.IsWinner =
                winner == _room.Player2;
        }

        public string GetWinnerName()
        {
            if (_room.Player1.IsWinner)
                return _room.Player1.Username;

            if (_room.Player2.IsWinner)
                return _room.Player2.Username;

            return "Draw";
        }
    }
}
