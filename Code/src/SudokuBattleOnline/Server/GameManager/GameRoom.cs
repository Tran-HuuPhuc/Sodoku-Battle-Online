using System;
using System.Timers;

namespace SudokuBattleOnline.Server.GameManager
{
public class GameRoom
{
public string RoomId { get; set; }

    public int RemainingTime { get; private set; }

    public bool IsGameEnded { get; private set; }

    private System.Timers.Timer gameTimer = new();

    public event Action<int>? TimeUpdated;
    public event Action? TimeUp;

    public GameRoom(string roomId, int matchTime = 600)
    {
        RoomId = roomId;
        RemainingTime = matchTime;

        gameTimer.Interval = 1000;
        gameTimer.Elapsed += OnTimerTick;
    }

    public void StartTimer()
    {
        gameTimer.Start();
    }

    public void StopTimer()
    {
        gameTimer.Stop();
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        RemainingTime--;

        TimeUpdated?.Invoke(RemainingTime);

        if (RemainingTime <= 0)
        {
            RemainingTime = 0;
            IsGameEnded = true;

            gameTimer.Stop();

            TimeUp?.Invoke();
        }
    }
}

}