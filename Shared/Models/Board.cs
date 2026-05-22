using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Board
    {
        public Cell[,] Cells { get; set; }
        public Board()
        {
            Cells = new Cell[9, 9];
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    Cells[row, col] = new Cell();
                }
            }
        }
    }
}
