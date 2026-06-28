using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SudokuBattle.Shared.Enums;

namespace SudokuBattle.Shared.Models
{
    public class RoomInfo
    {
        public string RoomId { get; set; }

        public string RoomName { get; set; }

        public string Host { get; set; }

        public string Guest { get; set; }

        public RoomStatus Status { get; set; }

        public int CurrentPlayers { get; set; }      
        
        public int MaxPlayers { get; set; }

        public bool IsGameStarted { get; set; }
    }
}