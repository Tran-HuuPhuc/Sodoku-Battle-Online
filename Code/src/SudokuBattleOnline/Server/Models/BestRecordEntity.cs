using Shared.Enums;
using System;

namespace Server.Models
{
    public class BestRecordEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; } = Difficulty.Medium;
        public int BestTimeSeconds { get; set; }
        public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
    }
}

