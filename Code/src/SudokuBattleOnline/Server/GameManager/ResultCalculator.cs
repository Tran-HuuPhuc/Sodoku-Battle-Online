using Shared.Models;

namespace Server.GameManager
{
    public static class ResultCalculator
    {
        public static Player DetermineWinner(
            Player player1,
            Player player2)
        {
            // Ưu tiên điểm số

            if (player1.Score > player2.Score)
                return player1;

            if (player2.Score > player1.Score)
                return player2;

            // Nếu bằng điểm thì ít lỗi hơn thắng

            if (player1.Mistakes < player2.Mistakes)
                return player1;

            if (player2.Mistakes < player1.Mistakes)
                return player2;

            return null;
        }
    }
}