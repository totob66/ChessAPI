using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace HaaToChess
{
    interface IPiece {

        ulong Position { get; set; }
        int pieceID { get; set; }
        int Color { get; }
        int Value { get; set; }
        Boolean InitialPosition { get; set; }
        int Index { get;  }
}
    class Piece:IPiece
    {
        // Private Fields
        // Properties
        public ulong Position{ get; set; }
        public int pieceID{ get; set; }
        public int Color{ get; set; }
        public int Value{ get; set; }
        //public Image PieceImage { get; set; }
        public Boolean InitialPosition { get; set; }
        public int Index { get { return (int)Math.Log(Position, 2); } }

        // Public Methodes
        // Private methodes      
        // Public methodes

        // Conctructors
        public Piece() { }
        //Constructor
        public Piece(int newID, int newColor, int squareIndex)
        {
            Position = (ulong)Math.Pow(2, squareIndex);
            pieceID = newID;
            Color = newColor;
            Value = 0;
            InitialPosition = true;
            //if (newColor == PieceType.White) { PieceImage = ToTo.Properties.Resources.WhitePiece; }
            //else { PieceImage = ToTo.Properties.Resources.BlackPiece; }
        }
        // Copy constructor
        public Piece(Piece ps)
        {
            Position = ps.Position;
            pieceID = ps.pieceID;
            Color = ps.Color;
            Value = ps.Value;
            InitialPosition = ps.InitialPosition;
            //PieceImage = ps.PieceImage;
        }
    }

    class Pawn : Piece
    {      
        public Pawn(int newID, int newColor, int squareIndex)
        {
            Value = 1;
            Color = newColor;
            pieceID = newID;
            Position = (ulong)Math.Pow(2, squareIndex);
            InitialPosition = true;
            //if (newColor == PieceType.White) { PieceImage = ToTo.Properties.Resources.WhitePawn; }
            //else { PieceImage = ToTo.Properties.Resources.BlackPawn; }
        }
    }
    class King : Piece
    {
        public King(int newID, int newColor, int squareIndex)
        {
            Value = 100;
            Color = newColor;
            pieceID = newID;
            Position = (ulong)Math.Pow(2, squareIndex);
            InitialPosition = true;
            //if (newColor == PieceType.White) { PieceImage = ToTo.Properties.Resources.WhiteKing; }
            //else { PieceImage = ToTo.Properties.Resources.BlackKing; }
        }
    }
    class Queen : Piece
    {
        public Queen(int newID, int newColor, int squareIndex)
        {
            Value = 10;
            Color = newColor;
            pieceID = newID;
            Position = (ulong)Math.Pow(2, squareIndex);
            InitialPosition = true;
            //if (newColor == PieceType.White) { PieceImage = ToTo.Properties.Resources.WhiteQueen; }
            //else { PieceImage = ToTo.Properties.Resources.BlackQueen; }
        }
    }
    class Bishop : Piece
    {
        public Bishop(int newID, int newColor, int squareIndex)
        {
            Value = 3;
            Color = newColor;
            pieceID = newID;
            Position = (ulong)Math.Pow(2, squareIndex);
            InitialPosition = true;
            //if (newColor == PieceType.White) { PieceImage = ToTo.Properties.Resources.WhiteBishop; }
            //else { PieceImage = ToTo.Properties.Resources.BlackBishop; }
        }
    }
    class Knight : Piece
    {
        public Knight(int newID, int newColor, int squareIndex)
        {
            Value = 3;
            Color = newColor;
            pieceID = newID;
            Position = (ulong)Math.Pow(2, squareIndex);
            InitialPosition = true;
            //if (newColor == PieceType.White) { PieceImage = ToTo.Properties.Resources.WhiteKnight; }
            //else { PieceImage = ToTo.Properties.Resources.BlackKnight; }
        }
    }
    class Rook : Piece
    {
        public Rook(int newID, int newColor, int squareIndex)
        {
            Value = 5;
            Color = newColor;
            pieceID = newID;
            Position = (ulong)Math.Pow(2, squareIndex);
            InitialPosition = true;
            //if (newColor == PieceType.White) { PieceImage = ToTo.Properties.Resources.WhiteRook; }
            //else { PieceImage = ToTo.Properties.Resources.BlackRook; }
        }
    }
}
