using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaaToChess
{
    public class ClientMove
    {
        public string fromSq { get; set; }
        public string toSq { get; set; }
        public string aiColor { get; set; }

        public ClientMove(string from, string to, string color)
        {
            fromSq = from;
            toSq = to;
            aiColor = color;
        }
    }
}
