using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HaaToChess
{
    interface MoveI
    {
        int PrimaryPieceID { get; set; }       
        int SecondaryPieceID { get; set; }
        ulong PrimaryNewPosition { get; set; }
        ulong SecondaryNewPosition { get; set; }
        int KindOfMove { get; set; }             //0 = Normal move, 1 = Rokade, 3 Ampasand, 4 Kill, 5 Pawn initial two square advance, 6 Pawn transformation
        int Score {get;set;}
    }

    class Move
    {
        public int PrimaryPieceID { get; set; }
        public int SecondaryPieceID { get; set; }
        public ulong PrimaryNewPosition { get; set; }
        public ulong SecondaryNewPosition { get; set; }
        public int KindOfMove { get; set; }
        public int Score {get;set;}

        private int[] scoreValues = new int[2];

        public Move()
        {
            PrimaryPieceID = -1;
            SecondaryPieceID = -1;
            PrimaryNewPosition = 0;
            SecondaryNewPosition = 0;
            KindOfMove = 0;
            Score = 0;
        }
        public Move(int pieceID, ulong position, int type)
        {
            PrimaryPieceID = pieceID;
            SecondaryPieceID = -1;
            PrimaryNewPosition = position;
            KindOfMove = type;
            SecondaryNewPosition = 0;
            Score = 0;
        }
        // Copy constructor
        public Move(Move m)
        {
            PrimaryNewPosition = m.PrimaryNewPosition;
            PrimaryPieceID = m.PrimaryPieceID;
            SecondaryNewPosition = m.SecondaryNewPosition;
            SecondaryPieceID = m.SecondaryPieceID;
            KindOfMove = m.KindOfMove;
            Score = m.Score;
        }
    }
}
