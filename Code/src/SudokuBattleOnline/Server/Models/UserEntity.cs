using System;

namespace Server.Models
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int Elo { get; set; } = 1000;
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
