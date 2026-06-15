using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Cell
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public int Value { get; set; }

        public bool IsFixed { get; set; }
    }
}
