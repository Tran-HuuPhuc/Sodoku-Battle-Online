using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Player
    {
        public string Username { get; set; }

        public int Score { get; set; }

        public int Mistakes { get; set; }

        public bool IsReady { get; set; }

        public bool IsWinner { get; set; }
    }
}
