namespace SudokuBattleOnline.Server.GameManager
{
    public class ResultCalculator
    {
        public string CalculateWinner(int player1Score, int player2Score)
        {
            if (player1Score > player2Score)
                return "Player 1 Wins";

            if (player2Score > player1Score)
                return "Player 2 Wins";

            return "Draw";
        }
    }
}