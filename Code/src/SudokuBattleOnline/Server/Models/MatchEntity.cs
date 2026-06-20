using Shared.Enums;
using System;

namespace Server.Models
{
    public class MatchEntity
    {
        public int Id { get; set; }
        public string Player1 { get; set; } = string.Empty;
        public string Player2 { get; set; } = string.Empty;
        public string Winner { get; set; } = string.Empty;
        public Difficulty Difficulty { get; set; } = Difficulty.Medium;
        public int DurationSeconds { get; set; }
        public int EloChangeP1 { get; set; }
        public int EloChangeP2 { get; set; }
        public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    }
}
