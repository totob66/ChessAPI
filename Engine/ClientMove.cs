using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaaToChess
{
    public class ClientMove
    {
        public string from { get; set; }
        public string to { get; set; }
        public string color { get; set; }

        public ClientMove(string from, string to, int color)
        {
            this.from = from;
            this.to = to;
            this.color = color==0 ? "b" : "w" ;
        }
    }
}
