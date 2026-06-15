using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class GameState
    {
        public int[,] Board { get; set; }

        public int Player1Progress { get; set; }

        public int Player2Progress { get; set; }

        public bool IsFinished { get; set; }

        public GameState()
        {
            Board = new int[9, 9];
        }
    }
}