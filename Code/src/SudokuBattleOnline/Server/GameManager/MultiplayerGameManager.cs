using Shared.Models;

namespace Server.GameManager
{
    public class MultiplayerGameManager
    {
        private const int MaxMistakes = 5;
        private readonly GameRoom _room;
        private int _emptyCellsCount = 40; // Số ô trống ban đầu để tính % tiến độ

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

            _room.GameState.Board = (int[,])puzzleBoard.Clone();

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

            // Đếm số ô trống ban đầu để làm cơ sở tính tiến trình %
            _emptyCellsCount = 0;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (puzzleBoard[r, c] == 0)
                        _emptyCellsCount++;
                }
            }
            if (_emptyCellsCount == 0) _emptyCellsCount = 81;
        }

        /// <summary>
        /// Người chơi nhập số vào ô. Server kiểm tra đúng/sai với SolutionBoard.
        /// Client KHÔNG BAO GIỜ biết đáp án – chỉ gửi giá trị, nhận phản hồi đúng/sai từ Server.
        /// </summary>
        public bool SubmitMove(
            int playerNumber,
            int row,
            int col,
            int value)
        {
            if (_room.GameState.IsFinished) return false;

            // Không cho sửa ô gốc (ô đề bài đã có số)
            if (_room.PuzzleBoard[row, col] != 0)
                return false;

            // Server là nơi DUY NHẤT kiểm tra đáp án
            bool isCorrect = _room.SolutionBoard[row, col] == value;

            if (isCorrect)
            {
                _room.GameState.Board[row, col] = value;
                AddCorrectMove(playerNumber);
                CheckAndFinishGame();
                return true;
            }

            AddMistake(playerNumber);
            CheckAndFinishGame();
            return false;
        }

        private void AddCorrectMove(int playerNumber)
        {
            if (playerNumber == 1)
            {
                _room.Player1.Score++;
                _room.GameState.Player1Progress = CalculateProgress(_room.Player1.Score);
            }
            else
            {
                _room.Player2.Score++;
                _room.GameState.Player2Progress = CalculateProgress(_room.Player2.Score);
            }
        }

        private void AddMistake(int playerNumber)
        {
            if (playerNumber == 1)
                _room.Player1.Mistakes++;
            else
                _room.Player2.Mistakes++;
        }

        private int CalculateProgress(int score)
        {
            return Math.Min(100, (score * 100) / _emptyCellsCount);
        }

        public bool IsGameFinished()
        {
            return _room.GameState.IsFinished;
        }

        /// <summary>
        /// Kiểm tra điều kiện kết thúc trận sau mỗi nước đi.
        /// Thứ tự ưu tiên:
        ///   1. Perfect Win – một bên đạt 100% → kết thúc ngay lập tức
        ///   2. Max Mistakes – một bên chạm 5 lỗi → thua cuộc
        /// </summary>
        private void CheckAndFinishGame()
        {
            bool p1Perfect = _room.Player1.Score >= _emptyCellsCount;
            bool p2Perfect = _room.Player2.Score >= _emptyCellsCount;
            bool p1MaxMistakes = _room.Player1.Mistakes >= MaxMistakes;
            bool p2MaxMistakes = _room.Player2.Mistakes >= MaxMistakes;

            if (p1Perfect || p2Perfect || p1MaxMistakes || p2MaxMistakes)
            {
                EndGame();
            }
        }

        /// <summary>
        /// Phân định kết quả trận đấu theo thứ tự ưu tiên:
        ///   1. Perfect Win (100% hoàn thành bảng) → thắng tuyệt đối, không xét thêm
        ///   2. Max Mistakes (5 lỗi) → thua ngay
        ///   3. So sánh tiến trình % (Score) → ai cao hơn thắng
        ///   4. Nếu bằng % → ai ít lỗi hơn thắng
        ///   5. Nếu tất cả bằng → HÒA
        /// </summary>
        public void EndGame()
        {
            _room.GameState.IsFinished = true;

            bool p1Perfect = _room.Player1.Score >= _emptyCellsCount;
            bool p2Perfect = _room.Player2.Score >= _emptyCellsCount;

            // ── Ưu tiên 1: Perfect Win ──────────────────────────────────────
            if (p1Perfect && !p2Perfect)
            {
                _room.Player1.IsWinner = true;
                _room.Player2.IsWinner = false;
                return;
            }
            if (p2Perfect && !p1Perfect)
            {
                _room.Player1.IsWinner = false;
                _room.Player2.IsWinner = true;
                return;
            }
            // Cả hai cùng Perfect (rất hiếm) → tiếp tục xét lỗi bên dưới

            // ── Ưu tiên 2: Max Mistakes (5 lỗi) ───────────────────────────
            if (_room.Player1.Mistakes >= MaxMistakes && _room.Player2.Mistakes < MaxMistakes)
            {
                _room.Player1.IsWinner = false;
                _room.Player2.IsWinner = true;
                return;
            }
            if (_room.Player2.Mistakes >= MaxMistakes && _room.Player1.Mistakes < MaxMistakes)
            {
                _room.Player1.IsWinner = true;
                _room.Player2.IsWinner = false;
                return;
            }

            // ── Ưu tiên 3 & 4: So sánh Score rồi Mistakes ──────────────────
            Player? winner = ResultCalculator.DetermineWinner(_room.Player1, _room.Player2);

            if (winner == null) // HÒA
            {
                _room.Player1.IsWinner = false;
                _room.Player2.IsWinner = false;
                return;
            }

            _room.Player1.IsWinner = winner == _room.Player1;
            _room.Player2.IsWinner = winner == _room.Player2;
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
