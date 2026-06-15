using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Board
    {
        public Cell[,] Cells = new Cell[9, 9];

        public Board()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cells[i, j] = new Cell();
                }
            }
        }

    }
}