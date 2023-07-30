using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
// ...

namespace HaaToChess
{
    interface IGameState{
        void AddPiece(Piece newPiece);
        void ExecuteMove(Move move, int perspective);
        string StateAsString();
        Piece GetPieceAtSquare(int index);
        Move LastMove { get; set; }

    }
    class GameState : IGameState
    {
        // Properties
        public Move LastMove { get; set; }
        public string Name { get; set; }
        public int AIPlayer { get; set; }
        
        // Fields
        private Piece[] whitePieces = new Piece[24];
        private Piece[] blackPieces = new Piece[24];
        private ulong whiteBlockers;                                    // All squares occupied by  white pieces
        private ulong blackBlockers;                                    // All squares occupied by  black pieces
        private ulong allBlockers;                                      // All squares occupied by any piece
        //private MoveGenerator mg; 
        
        //Public methodes
        public void ClearBoard()
        {
            //mg = new MoveGenerator();
            whiteBlockers = 0;
            blackBlockers = 0;
            for (int i = 0; i <= 23; i++) { whitePieces[i] = null; blackPieces[i] = null; }
            LastMove = null;
        }
        public void AddPiece(Piece newPiece)                            // Add new piece to chess board
        {
            if (!(newPiece is null))
            {
                if (newPiece.Color == 1)
                {
                    this.whitePieces[newPiece.pieceID] = newPiece;
                    whiteBlockers |= newPiece.Position;
                }
                if (newPiece.Color == 0)
                {
                    this.blackPieces[newPiece.pieceID] = newPiece;
                    blackBlockers |= newPiece.Position;
                }
            allBlockers |= newPiece.Position;
            }
        }
        public void RemovePiece(int PieceID,int perspective)
        {
            if (perspective == 0) { this.blackPieces[PieceID] = null; }
            else { this.whitePieces[PieceID] = null; }
        }
        public void ExecuteMove(Move move, int perspective)             // Execute move                                                               
        {

            LastMove = new Move(move);                                                                  // Save last move
            Piece primaryPiece = this.GetPiece(move.PrimaryPieceID, perspective); 
            
            //Piece to move is black
            if (primaryPiece.Color == 0) {
                blackBlockers ^= primaryPiece.Position | move.PrimaryNewPosition;      // Delete old position and add new position
                primaryPiece.InitialPosition = false;                                  // Set "not in intial position" flag
                primaryPiece.Position = move.PrimaryNewPosition;                       // Update piece position

                for (int i = 0; i <= 23; i++) 
                { if (!(whitePieces[i] is null)) 
                    { if (move.PrimaryNewPosition == whitePieces[i].Position)               // Kill move -> remove white piece
                        {
                            whiteBlockers &= ~whitePieces[i].Position;
                            whitePieces[i] = null; 
                        } 
                    } 
                }
            }
            //Piece to move is white
            if (primaryPiece.Color == 1) {
                whiteBlockers ^= primaryPiece.Position | move.PrimaryNewPosition;      // Delete old position and add new position
                primaryPiece.InitialPosition = false;                                  // Set "not in intial position" flag
                primaryPiece.Position = move.PrimaryNewPosition;                       // Update piece position
                for (int i = 0; i <= 23; i++) 
                { if (!(blackPieces[i] is null)) 
                    { if (move.PrimaryNewPosition == blackPieces[i].Position) 
                        {
                            blackBlockers &= ~blackPieces[i].Position;
                            blackPieces[i] = null; 
                        } 
                    } 
                }
            }   
            // Move include a secondary move (Ampasand or rokade)
            if (move.SecondaryPieceID != -1)                                                 
            {
                Piece secondaryPiece = this.GetPiece(move.SecondaryPieceID, perspective);             // MÅ ENDRES PERSPECTIVE
                //Piece to move is black
                if (secondaryPiece.Color == 0)
                {
                    blackBlockers ^= secondaryPiece.Position | move.SecondaryNewPosition;      // Delete old position and add new position
                    secondaryPiece.InitialPosition = false;                                    // Set "not in intial position" flag
                    secondaryPiece.Position = move.SecondaryNewPosition;                       // Update piece position
                }
                //Piece to move is white
                if (primaryPiece.Color == 1)
                {
                    whiteBlockers ^= secondaryPiece.Position | move.SecondaryNewPosition;      // Delete old position and add new position
                    secondaryPiece.InitialPosition = false;                                    // Set "not in intial position" flag
                    secondaryPiece.Position = move.SecondaryNewPosition;                       // Update piece position
                }
            }
            // Pawn transformation
            if (move.KindOfMove == 6)     
            {
                int pieceId = move.PrimaryPieceID;
                int pawnPosIndex = (int)Math.Log(move.PrimaryNewPosition, 2);
                this.RemovePiece(pieceId, perspective);
                this.AddPiece(new Queen(pieceId + 8, perspective, pawnPosIndex));
            }

            allBlockers = whiteBlockers | blackBlockers;                                            // Update all blockers
         
        }
        public Piece GetPiece(int i, int perspective)                   // Return piece nr. i from black or white player
        {
            if (perspective == 0) { return this.blackPieces[i]; }
            else { return this.whitePieces[i]; }
        }
        public Piece GetPieceAtSquare(int index) {

            for (int i = 0; i <= 23; i++)
            {
                if (whitePieces[i] != null && whitePieces[i].Index == index) { return whitePieces[i]; }
                if (blackPieces[i] != null && blackPieces[i].Index == index) { return blackPieces[i]; }
            }
            return null;
        }
        public ulong BlockedSquares(int perspective) {
            if (perspective == 0) { return blackBlockers; }
            if (perspective == 1) { return whiteBlockers; }
            else { return allBlockers; }
        }
        public string StateAsString()                                        // Convert state to string
        {
            string[] piecesBlack = { "k", "q", "b", "b", "n", "n", "r", "r", "p", "p", "p", "p", "p", "p", "p", "p" };
            string[] piecesWhite = { "K", "Q", "B", "B", "N", "N", "R", "R", "P", "P", "P", "P", "P", "P", "P", "P" };

            string[,] board= new string[8, 8] { { ".", ".", ".", ".", ".", ".", ".", "." },
                                                { ".", ".", ".", ".", ".", ".", ".", "." },
                                                { ".", ".", ".", ".", ".", ".", ".", "." },
                                                { ".", ".", ".", ".", ".", ".", ".", "." },
                                                { ".", ".", ".", ".", ".", ".", ".", "." },
                                                { ".", ".", ".", ".", ".", ".", ".", "." },
                                                { ".", ".", ".", ".", ".", ".", ".", "." },
                                                { ".", ".", ".", ".", ".", ".", ".", "." }};
            
            for (int i = 0; i <= 23; i++){
                if (!(blackPieces[i] is null)){
                    int row = (int)Math.Log(blackPieces[i].Position, 2) / 8;
                    int col = (row + 1) * 8 - (int)Math.Log(blackPieces[i].Position, 2)-1;
                    board[row, col] = piecesBlack[blackPieces[i].pieceID];
                }
                if (!(whitePieces[i] is null))
                {
                    int row = (int)Math.Log(whitePieces[i].Position, 2) / 8;
                    int col = (row + 1) * 8 - (int)Math.Log(whitePieces[i].Position, 2) - 1;
                    board[row, col] = piecesWhite[whitePieces[i].pieceID];
                }
            }
            string value="";
            for (int row=7;row>=0;row--){
                for (int col=0; col<8; col++){ value += String.Format("{0,5}", board[row,col]); }
                value += Environment.NewLine + Environment.NewLine;
            }
            return value;
        }
        
        // Constructor
        public GameState() {
            //mg = new MoveGenerator();
            whiteBlockers = 0;
            blackBlockers = 0;
            for (int i = 0; i <= 23; i++) { whitePieces[i] = null; blackPieces[i] = null; }
            LastMove = null;
            AIPlayer = 0;
        }

        // Copy constructor
        public GameState(GameState gs)
        {
            for (int i = 0; i <= 23; i++){

                if (!(gs.GetPiece(i, 0) is null)) { this.AddPiece(new Piece(gs.GetPiece(i, 0))); } 
                if (!(gs.GetPiece(i, 1) is null)) { this.AddPiece(new Piece(gs.GetPiece(i, 1))); }        
            }
            AIPlayer = gs.AIPlayer;
            if (!(gs.LastMove is null)) { LastMove = new Move(gs.LastMove); } // To handle no existing last move in a start position with AI player performin firstr move
        }

        public GameState(String fen)
        {
            String[] subFEN = fen.Split(' ');                                                           //Split FEN into state ....

            String currentState = subFEN[0];                                                            // Current game state
            if (!(subFEN[1] is null) & subFEN[1] == "w") {this.AIPlayer = 1; }                          //AI color
            //CastlinRights
            //En Passent Targets
            //Half move clock
            //Full move number

            String[] ranks = currentState.Split('/');                                                   // Split inti rank states

            int nCount = 0; int bCount = 0; int pCount = 0; int NCount = 0; int BCount = 0; int PCount = 0; int rCount = 0; int RCount = 0;

            for (int r = 0; r < ranks.Length; r++)                                                      //Process one rank at each iteration
            {
                string rank = ranks[r];
                
                for (int i = 0; i < rank.Length; i++)                                                   //Process one letter at each iteration
                {
                    if (char.IsDigit(rank[i]))
                    {
                        string str = "";
                        string emptySquares = str.PadLeft((int)Char.GetNumericValue(rank[i]), '.');     //Make string with dots representing empty squares    
                        rank = rank.Substring(0, i) + emptySquares + rank.Substring(i + 1);
                    }
                }
                Debug.WriteLine(rank);
                for (int i = 0; i < rank.Length; i++)
                {
                    char val = rank[i];
                    int pos = (8 - r) * 8 - 1 - i;
                    switch (val)
                    {
                        case 'r':
                            if (rCount == 0) { this.AddPiece(new Rook(PieceType.Rook1, PieceType.Black, pos)); }
                            if (rCount == 1) { this.AddPiece(new Rook(PieceType.Rook2, PieceType.Black, pos)); }
                            rCount++;
                            break;
                        case 'n':
                            if (nCount == 0) { this.AddPiece(new Knight(PieceType.Knight1, PieceType.Black, pos)); }
                            if (nCount == 1) { this.AddPiece(new Knight(PieceType.Knight2, PieceType.Black, pos)); }
                            nCount++;
                            break;
                        case 'b':
                            if (bCount == 0) { this.AddPiece(new Bishop(PieceType.Bishop1, PieceType.Black, pos)); }
                            if (bCount == 1) { this.AddPiece(new Knight(PieceType.Bishop2, PieceType.Black, pos)); }
                            bCount++;
                            break;
                        case 'k':
                            this.AddPiece(new King(PieceType.King, PieceType.Black, pos));
                            break;
                        case 'q':
                            this.AddPiece(new Queen(PieceType.Queen, PieceType.Black, pos));
                            break;
                        case 'p':
                            if (pCount == 0) { this.AddPiece(new Pawn(PieceType.Pawn1, PieceType.Black, pos)); }
                            if (pCount == 1) { this.AddPiece(new Pawn(PieceType.Pawn2, PieceType.Black, pos)); }
                            if (pCount == 2) { this.AddPiece(new Pawn(PieceType.Pawn3, PieceType.Black, pos)); }
                            if (pCount == 3) { this.AddPiece(new Pawn(PieceType.Pawn4, PieceType.Black, pos)); }
                            if (pCount == 4) { this.AddPiece(new Pawn(PieceType.Pawn5, PieceType.Black, pos)); }
                            if (pCount == 5) { this.AddPiece(new Pawn(PieceType.Pawn6, PieceType.Black, pos)); }
                            if (pCount == 6) { this.AddPiece(new Pawn(PieceType.Pawn7, PieceType.Black, pos)); }
                            if (pCount == 7) { this.AddPiece(new Pawn(PieceType.Pawn8, PieceType.Black, pos)); }
                            pCount++;
                            break;
                        case 'R':
                            if (RCount == 0) { this.AddPiece(new Rook(PieceType.Rook1, PieceType.White, pos)); }
                            if (RCount == 1) { this.AddPiece(new Rook(PieceType.Rook2, PieceType.White, pos)); }
                            RCount++;
                            break;
                        case 'N':
                            if (NCount == 0) { this.AddPiece(new Knight(PieceType.Knight1, PieceType.White, pos)); }
                            if (NCount == 1) { this.AddPiece(new Knight(PieceType.Knight2, PieceType.White, pos)); }
                            NCount++;
                            break;
                        case 'B':
                            if (BCount == 0) { this.AddPiece(new Bishop(PieceType.Bishop1, PieceType.White, pos)); }
                            if (BCount == 1) { this.AddPiece(new Knight(PieceType.Bishop2, PieceType.White, pos)); }
                            BCount++;
                            break;
                        case 'K':
                            this.AddPiece(new King(PieceType.King, PieceType.White, pos));
                            break;
                        case 'Q':
                            this.AddPiece(new Queen(PieceType.Queen, PieceType.White, pos));
                            break;
                        case 'P':
                            if (PCount == 0) { this.AddPiece(new Pawn(PieceType.Pawn1, PieceType.White, pos)); }
                            if (PCount == 1) { this.AddPiece(new Pawn(PieceType.Pawn2, PieceType.White, pos)); }
                            if (PCount == 2) { this.AddPiece(new Pawn(PieceType.Pawn3, PieceType.White, pos)); }
                            if (PCount == 3) { this.AddPiece(new Pawn(PieceType.Pawn4, PieceType.White, pos)); }
                            if (PCount == 4) { this.AddPiece(new Pawn(PieceType.Pawn5, PieceType.White, pos)); }
                            if (PCount == 5) { this.AddPiece(new Pawn(PieceType.Pawn6, PieceType.White, pos)); }
                            if (PCount == 6) { this.AddPiece(new Pawn(PieceType.Pawn7, PieceType.White, pos)); }
                            if (PCount == 7) { this.AddPiece(new Pawn(PieceType.Pawn8, PieceType.White, pos)); }
                            PCount++;
                            break;
                    }
                }
            }
        }
    }
}
