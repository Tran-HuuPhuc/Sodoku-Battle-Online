using SudokuBattle.Shared.Enums;
using System;

namespace Server.Models
{
    public class RoomEntity
    {
        public string RoomId { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Guest { get; set; } = string.Empty;
        public RoomStatus Status { get; set; } = RoomStatus.Waiting;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
