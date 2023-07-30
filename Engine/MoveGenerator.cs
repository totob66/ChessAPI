using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;

namespace HaaToChess
{
    // Interface to Class
    interface IMoveGenerator
    {
        ArrayList LeagalMoves(GameState gs, int p);
    }
    //Class definition
    class MoveGenerator : IMoveGenerator
    {
        // Properties

        // Fields
        private ulong[][] ubmp = new ulong[7][];
        private static ulong[] rookMagic = {
            0xa8002c000108020, 0x6c00049b0002001, 0x100200010090040, 0x2480041000800801, 0x280028004000800,
            0x900410008040022, 0x280020001001080, 0x2880002041000080, 0xa000800080400034, 0x4808020004000,
            0x2290802004801000, 0x411000d00100020, 0x402800800040080, 0xb000401004208, 0x2409000100040200,
            0x1002100004082, 0x22878001e24000, 0x1090810021004010, 0x801030040200012, 0x500808008001000,
            0xa08018014000880, 0x8000808004000200, 0x201008080010200, 0x801020000441091, 0x800080204005,
            0x1040200040100048, 0x120200402082, 0xd14880480100080, 0x12040280080080, 0x100040080020080,
            0x9020010080800200, 0x813241200148449, 0x491604001800080, 0x100401000402001, 0x4820010021001040,
            0x400402202000812, 0x209009005000802, 0x810800601800400, 0x4301083214000150, 0x204026458e001401,
            0x40204000808000, 0x8001008040010020, 0x8410820820420010, 0x1003001000090020, 0x804040008008080,
            0x12000810020004, 0x1000100200040208, 0x430000a044020001, 0x280009023410300, 0xe0100040002240,
            0x200100401700, 0x2244100408008080, 0x8000400801980, 0x2000810040200, 0x8010100228810400,
            0x2000009044210200, 0x4080008040102101, 0x40002080411d01, 0x2005524060000901, 0x502001008400422,
            0x489a000810200402, 0x1004400080a13, 0x4000011008020084, 0x26002114058042 };
        private static int[] rookIndexBits = {
            12, 11, 11, 11, 11, 11, 11, 12,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            12, 11, 11, 11, 11, 11, 11, 12};
        private ulong[,] rookMoves = new ulong[64, 4096];
        private static ulong[] bishopMagic = {
            0x89a1121896040240, 0x2004844802002010, 0x2068080051921000, 0x62880a0220200808, 0x4042004000000,
            0x100822020200011, 0xc00444222012000a, 0x28808801216001, 0x400492088408100, 0x201c401040c0084,
            0x840800910a0010, 0x82080240060, 0x2000840504006000, 0x30010c4108405004, 0x1008005410080802,
            0x8144042209100900, 0x208081020014400, 0x4800201208ca00, 0xf18140408012008, 0x1004002802102001,
            0x841000820080811, 0x40200200a42008, 0x800054042000, 0x88010400410c9000, 0x520040470104290,
            0x1004040051500081, 0x2002081833080021, 0x400c00c010142, 0x941408200c002000, 0x658810000806011,
            0x188071040440a00, 0x4800404002011c00, 0x104442040404200, 0x511080202091021, 0x4022401120400,
            0x80c0040400080120, 0x8040010040820802, 0x480810700020090, 0x102008e00040242, 0x809005202050100,
            0x8002024220104080, 0x431008804142000, 0x19001802081400, 0x200014208040080, 0x3308082008200100,
            0x41010500040c020, 0x4012020c04210308, 0x208220a202004080, 0x111040120082000, 0x6803040141280a00,
            0x2101004202410000, 0x8200000041108022, 0x21082088000, 0x2410204010040, 0x40100400809000,
            0x822088220820214, 0x40808090012004, 0x910224040218c9, 0x402814422015008, 0x90014004842410,
            0x1000042304105, 0x10008830412a00, 0x2520081090008908, 0x40102000a0a60140};
        private static int[] bishopIndexBits = {
            6, 5, 5, 5, 5, 5, 5, 6,
            5, 5, 5, 5, 5, 5, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 5, 5, 5, 5, 5, 5,
            6, 5, 5, 5, 5, 5, 5, 6};
        private ulong[,] bishopMoves = new ulong[64, 512];

        //Public methodes       
        public ArrayList LeagalMoves(GameState gs, int p)
        {

            ulong[,] castlingMove = new ulong[,] {              //Possible castling moves
                { 0x200000000000000, 0x2000000000000000 },
                { 0x2, 0x20 }
            };
            ulong[] moveBBs = new ulong[24];
            ArrayList pinnedSquaresList = new ArrayList();
            ArrayList opponentAttackedSquaresList = new ArrayList();
            int nCheck = 0;
            ulong possibleBlockerSquares = 0;
            ulong  unallowedKingMoves = 0;
            Boolean kingIsCheck = false;

            //Move Calculation: King            
            if (!(gs.GetPiece(0, p) is null))                                                                                      // King exists
            {
                unallowedKingMoves = 0;
                opponentAttackedSquaresList = calculateOpponentMoves(gs, p, true);                                                            // Calculate piecewise opponent defended squares
                pinnedSquaresList = calculatePinnedSquares(gs, p);                                                                 // Calculate squares pinned by opponent king threat

                foreach (ulong item in opponentAttackedSquaresList)                                                                           // Assemble all squares attacked/defended by opponent pieces
                {
                    if ((item & gs.GetPiece(0, p).Position) != 0) { nCheck++; kingIsCheck = true; }
                    unallowedKingMoves |= item;
                }
                //WriteBitboard(unallowedKingMoves);
                //Debug.WriteLine(kingIsCheck);

                Boolean[] kingBlocked = RokadeBlocked(gs, p, unallowedKingMoves);                                                    // Check if rokade is bloked (short and long)
                int pos = (int)Math.Log(gs.GetPiece(0, p).Position, 2);                                                              // King position            
                //ulong test1 = gs.BlockedSquares(p);
                //ulong test2 = ubmp[5][pos];
                moveBBs[0] = ubmp[5][pos] & ~(gs.BlockedSquares(p) | unallowedKingMoves);                                            // Calculate leagal king moves
                if (nCheck == 0)                                                                                                     // Not check
                { 
                    if (!(gs.GetPiece(6, p) is null) && gs.GetPiece(6, p).InitialPosition && gs.GetPiece(0, p).InitialPosition && !kingBlocked[0]) // Add possible castling moves short side
                    { moveBBs[0] |= castlingMove[p, 0]; }
                    if (!(gs.GetPiece(7, p) is null) && gs.GetPiece(7, p).InitialPosition && gs.GetPiece(0, p).InitialPosition && !kingBlocked[1]) // Add possible castling moves long side
                    { moveBBs[0] |= castlingMove[p, 1]; }
                }
            }
            //Queen possible moves
            if (!(gs.GetPiece(1, p) is null))                                                                                       // Queen exists
            {
                ulong bm;                                                                                                           // Bishop moves
                ulong rm;                                                                                                           // Rook moves
                int pos = (int)Math.Log(gs.GetPiece(1, p).Position, 2);                                                             // Position of white or black queen depending on player perspective
                ulong bmp = ubmp[1][pos] & gs.BlockedSquares(PieceType.All);                                                        // Rook blocked squares
                ulong index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);                                                  // "Magic" lookup index calculation
                rm = rookMoves[pos, index] & ~gs.BlockedSquares(p);
                bmp = ubmp[0][pos] & gs.BlockedSquares(2);                                                                          // Bishop unblocked rook move pattern
                index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);                                                    // "Magic" lookup index calculation
                bm = bishopMoves[pos, index] & ~gs.BlockedSquares(p);
                moveBBs[1] = (rm | bm);

                // Possible moves to avoid or get out of check state
                foreach (pinnedSquares item in pinnedSquaresList) 
                { 
                                // Is a pinned piece                                                          //Is the only pinned piece
                    if (((gs.GetPiece(1, p).Position & item.pinned)) != 0 && ((gs.BlockedSquares(p) & item.pinned) ^ gs.GetPiece(1, p).Position) == 0) 
                    { 
                        moveBBs[1] &= item.pinned;                                                                                  // Only allowd to move to other pinned squares
                    }
                    if ((item.pinned & gs.BlockedSquares(p ^ 1)) == 0) { possibleBlockerSquares = item.pinned | item.attacker.Position; }
                }
                if (kingIsCheck) 
                { 
                    moveBBs[1] &= possibleBlockerSquares;                                                                           // If check -> Allow only moves blocking the king attack
                    if (nCheck > 1) { moveBBs[1] = 0; }                                                                             // Number of check attacks  > 1 -> impossible to block and king must move)
                } 
                                                                                                                                    // Number of check attacks (nKingAttacks > 1 -> impossible to block and king must move)
            }
            //Transformed pawn possible moves
            for (int i = 16; i <= 23; i++)
            {
                if (!(gs.GetPiece(i, p) is null))                                                                                       // Queen exists
                {
                    ulong bm;                                                                                                           // Bishop moves
                    ulong rm;                                                                                                           // Rook moves
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);                                                             // Position of white or black queen depending on player perspective
                    ulong bmp = ubmp[1][pos] & gs.BlockedSquares(PieceType.All);                                                        // Rook blocked squares
                    ulong index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);                                                  // "Magic" lookup index calculation
                    rm = rookMoves[pos, index] & ~gs.BlockedSquares(p);
                    bmp = ubmp[0][pos] & gs.BlockedSquares(2);                                                                          // Bishop unblocked rook move pattern
                    index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);                                                    // "Magic" lookup index calculation
                    bm = bishopMoves[pos, index] & ~gs.BlockedSquares(p);
                    moveBBs[i] = (rm | bm);

                    // Possible moves to avoid or get out of check state
                    foreach (pinnedSquares item in pinnedSquaresList)
                    {
                        // Is a pinned piece                                                          //Is the only pinned piece
                        if (((gs.GetPiece(i, p).Position & item.pinned)) != 0 && ((gs.BlockedSquares(p) & item.pinned) ^ gs.GetPiece(i, p).Position) == 0)
                        {
                            moveBBs[i] &= item.pinned;                                                                                  // Only allowd to move to other pinned squares
                        }
                        if ((item.pinned & gs.BlockedSquares(p ^ 1)) == 0) { possibleBlockerSquares = item.pinned | item.attacker.Position; }
                    }
                    if (kingIsCheck)
                    {
                        moveBBs[i] &= possibleBlockerSquares;                                                                           // If check -> Allow only moves blocking the king attack
                        if (nCheck > 1) { moveBBs[i] = 0; }                                                                             // Number of check attacks  > 1 -> impossible to block and king must move)
                    }
                    // Number of check attacks (nKingAttacks > 1 -> impossible to block and king must move)
                }
            }
            //Bishop possible moves
            for (int i = 2; i <= 3; i++)
            {
                if (!(gs.GetPiece(i, p) is null))                                                                                   // Bishop exists
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);                                                         // Position of white or black Bishop depending on player perspective
                    ulong bmp = ubmp[0][pos] & gs.BlockedSquares(2);                                                                // Bishop unblocked move pattern
                    ulong index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);                                          // "Magic" lookup index calculation
                    moveBBs[i] = bishopMoves[pos, index] & ~gs.BlockedSquares(p);                                                   // Lookup in precalculated "Magic" bishop lookup table

                    // Possible moves to avoid or get out of check state
                    foreach (pinnedSquares item in pinnedSquaresList) 
                    {
                                    // Is a pinned piece                                                          //Is the only pinned piece
                        if ((gs.GetPiece(i, p).Position & item.pinned) != 0 && ((gs.BlockedSquares(p) & item.pinned) ^ gs.GetPiece(i, p).Position) == 0) // Do not move a single pinned piece so that king is in a state of check
                        { 
                            moveBBs[i] &= item.pinned;               
                        }
                        if ((item.pinned & gs.BlockedSquares(p ^ 1)) == 0) { possibleBlockerSquares = item.pinned | item.attacker.Position; }
                    }
                    if (kingIsCheck)
                    {
                        moveBBs[i] &= possibleBlockerSquares;                                                                           // If check -> Allow only moves blocking the king attack
                        if (nCheck > 1) { moveBBs[i] = 0; }                                                                             // Number of check attacks  > 1 -> impossible to block and king must move)
                    }
                }
            }
            //Knight possible moves
            for (int i = 4; i <= 5; i++)
            {
                if (!(gs.GetPiece(i, p) is null))                                                                                   // Knight exists
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);
                    moveBBs[i] = ubmp[4][pos] & ~gs.BlockedSquares(p);

                    // Possible moves to avoid or get out of check state
                    foreach (pinnedSquares item in pinnedSquaresList) 
                    {
                                      // Is a pinned piece                                                          //Is the only pinned piece    
                        if ((gs.GetPiece(i, p).Position & item.pinned) != 0 && ((gs.BlockedSquares(p) & item.pinned) ^ gs.GetPiece(i, p).Position) == 0) 
                        { 
                            moveBBs[i] &= item.pinned; 
                        }
                        if ((item.pinned & gs.BlockedSquares(p ^ 1)) == 0) { possibleBlockerSquares = item.pinned | item.attacker.Position; }
                    }
                    if (kingIsCheck)
                    {
                        if (nCheck == 1) { moveBBs[i] &= possibleBlockerSquares; }                                                                           // If check -> Allow only moves blocking the king attack
                        if (nCheck  > 1) { moveBBs[i] = 0; }                                                                             // Number of check attacks  > 1 -> impossible to block and king must move)
                    }
                    // Number of check attacks (nKingAttacks > 1 -> impossible to block and king must move)
                }
            }
            //Rook possible moves
            for (int i = 6; i <= 7; i++)
            {
                if (!(gs.GetPiece(i, p) is null))                                                                                    // Rook exists
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);                                                          // Position of white or black rook depending on player perspective
                    ulong bmp = ubmp[1][pos] & gs.BlockedSquares(PieceType.All);                                                     // Rook unblocked move pattern
                    ulong index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);                                               // "Magic" lookup index calculation
                    moveBBs[i] = rookMoves[pos, index] & ~gs.BlockedSquares(p);                                                      // Lookup in precalculated "Magic" rook lookup table

                    // Possible moves to avoid or get out of check state
                    foreach (pinnedSquares item in pinnedSquaresList) 
                    {
                        if ((gs.GetPiece(i, p).Position & item.pinned) != 0 && ((gs.BlockedSquares(p) & item.pinned) ^ gs.GetPiece(i, p).Position) == 0) // Is the only pinned piece
                        { 
                            moveBBs[i] &= item.pinned; 
                        }
                        if ((item.pinned & gs.BlockedSquares(p ^ 1)) == 0) { possibleBlockerSquares = item.pinned | item.attacker.Position; }
                    }
                    if (kingIsCheck)
                    {
                        moveBBs[i] &= possibleBlockerSquares;                                                                           // If check -> Allow only moves blocking the king attack
                        if (nCheck > 1) { moveBBs[i] = 0; }                                                                             // Number of check attacks  > 1 -> impossible to block and king must move)
                    }
                    // Number of check attacks (nKingAttacks > 1 -> impossible to block and king must move)
                }
            }
            //Pawn possible moves
            for (int i = 8; i <= 15; i++)
            {
                if (!(gs.GetPiece(i, p) is null))
                {                                                                                                                                                // Check if pawn exists
                    ulong[] files = new ulong[8]                                                                                                                // File bitboard filter
                    {
                        0x4040404040404040, 0xa0a0a0a0a0a0a0a0, 0x5050505050505050, 0x2828282828282828,0x1414141414141414, 0xa0a0a0a0a0a0a0a,0x505050505050505,0x202020202020202
                    };                                                                                                             // Files bitboard pattern filter                      
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);                                                                                      // Pawn position 
                    int rank = pos / 8; int file = (rank + 1) * 8 - pos - 1;                                                                                     // Calculate current file and rank based on position
                    if (rank == 6 && p == 0)
                    {                                                                                                                                            // Black  player pawn in initial position check
                        if (((gs.GetPiece(i, p).Position >> 8) & gs.BlockedSquares(PieceType.All)) == 0)
                        {
                            moveBBs[i] |= (gs.GetPiece(i, p).Position >> 8);     // Black initial pawn move to rank 5 added
                            if (((gs.GetPiece(i, p).Position >> 16) & gs.BlockedSquares(PieceType.All)) == 0)
                            {
                                moveBBs[i] |= (gs.GetPiece(i, p).Position >> 16);                                                                                // Black initial pawn move to rank 4 added
                            }
                        }
                    }
                    if (rank == 1 && p == 1)
                    {                                                                                                                                            // White  player pawn in initial position check
                        if (((gs.GetPiece(i, p).Position << 8) & gs.BlockedSquares(PieceType.All)) == 0)
                        {
                            moveBBs[i] |= (gs.GetPiece(i, p).Position << 8);     // White initial pawn move to rank 2 added
                            if (((gs.GetPiece(i, p).Position << 16) & gs.BlockedSquares(PieceType.All)) == 0)
                            {
                                moveBBs[i] |= (gs.GetPiece(i, p).Position << 16);                                                                                // White initial pawn move to rank 3 added
                            }
                        }
                    }
                    moveBBs[i] |= (ubmp[3 - p][pos] & files[file]) & gs.BlockedSquares(p ^ 1);                                                                                                     // Pawn sidewise capture                                                                                                                                                             
                    if (p == 0 && (rank < 6 & rank > 0)) { if (((gs.GetPiece(i, p).Position >> 8) & gs.BlockedSquares(PieceType.All)) == 0) { moveBBs[i] |= (gs.GetPiece(i, p).Position >> 8); } } // Black pawn normal forward move
                    if (p == 1 && (rank < 7 & rank > 1)) { if (((gs.GetPiece(i, p).Position << 8) & gs.BlockedSquares(PieceType.All)) == 0) { moveBBs[i] |= (gs.GetPiece(i, p).Position << 8); } } // White pawn normal forward move
                                                                                                                                                                                                   // King is in double check and must move. No other moves allowed
                                                                                                                                                                                                   // Add Ampasand move
                    if (!(gs.LastMove is null)) // Test hvis AI trekker først
                    { 
                        int lastPieceRank = (int)Math.Log(gs.LastMove.PrimaryNewPosition, 2) / 8;
                        if (p == PieceType.Black && rank == 3 && lastPieceRank == 3 && gs.LastMove.PrimaryPieceID > 7 && gs.LastMove.KindOfMove == 5)             //Last move piecetype is black pawn moved with two square advance move
                        {
                            moveBBs[i] |= gs.LastMove.PrimaryNewPosition;                                                                                          // Ampasand move     
                        }
                        if (p == PieceType.White && rank == 4 && lastPieceRank == 4 && gs.LastMove.PrimaryPieceID > 7 && gs.LastMove.KindOfMove == 5)             //Last move piecetype is white pawn moved with two square advance move
                        {
                            moveBBs[i] |= gs.LastMove.PrimaryNewPosition;                                                                                          // Ampasand move                
                        }
                    }

                    // Possible moves to avoid or get out of check state
                    foreach (pinnedSquares item in pinnedSquaresList)
                    {
                        // Is a pinned piece                                                          //Is the only pinned piece
                        if ((gs.GetPiece(i, p).Position & item.pinned) != 0 && ((gs.BlockedSquares(p) & item.pinned) ^ gs.GetPiece(i, p).Position) == 0)
                        {
                            moveBBs[i] &= item.pinned;
                        }
                        if ((item.pinned & gs.BlockedSquares(p ^ 1)) == 0) { possibleBlockerSquares = item.pinned | item.attacker.Position; }
                    }
                    if (kingIsCheck)
                    {
                        moveBBs[i] &= possibleBlockerSquares;                                                                           // If check -> Allow only moves blocking the king attack
                        if (nCheck > 1) { moveBBs[i] = 0; }                                                                             // Number of check attacks  > 1 -> impossible to block and king must move)
                    }
                    // Number of check attacks (nKingAttacks > 1 -> impossible to block and king must move)
                }
            }
            // Serialize moveBB into seperate moves
            ArrayList moves = new ArrayList();
            for (int i = 0; i <= 23; i++)
            {
                ulong moveBB = moveBBs[i];
                while (moveBB != 0)
                {
                    ulong lsb = moveBB & (~moveBB + 1);                                                                            // Calculate Least Significant Bit
                    Move m = new Move(i, lsb, 0);                                                                                  // Normal move

                    // Rokade detected
                    if (i == 0 && gs.GetPiece(0, p).InitialPosition)                                                               // King in initial position
                    {                                                             
                        if (((int)Math.Log(gs.GetPiece(0, p).Position, 2) - (int)Math.Log(lsb, 2) == 2))                           // Short rokade
                        {                         
                            m.SecondaryPieceID = gs.GetPiece(6, p).pieceID;                                                        // Rock on short side
                            m.SecondaryPieceID = 6;
                            m.KindOfMove = 1;
                            if (p == 1) { m.SecondaryNewPosition = 4; }                                                            // Move Rook - Short rokade
                            if (p == 0) { m.SecondaryNewPosition = 288230376151711744; }                                           // Move Rook - Long rokade
                        }
                        if (((int)Math.Log(gs.GetPiece(0, p).Position, 2) - (int)Math.Log(lsb, 2) == -2))                          // Long rokade
                        {                         
                            m.SecondaryPieceID = gs.GetPiece(7, p).pieceID;                                                        // Rock on long side
                            m.SecondaryPieceID = 7;
                            m.KindOfMove = 1;
                            if (p == 1) { m.SecondaryNewPosition = 16; }                                                           // Move Rook - Short rokade
                            if (p == 0) { m.SecondaryNewPosition = 1152921504606846976; }                                          // Move Rook - Long rokade
                        }
                    }     
                    
                    // Pawn two square initial advance move detected
                    if (i>7 && p == PieceType.Black && ((gs.GetPiece(i, p).Position << 16) & lsb) == 0) { m.KindOfMove = 5; }      // Pawn 2 square advance move
                    if (i>7 && p == PieceType.White && ((gs.GetPiece(i, p).Position >> 16) & lsb) == 0) { m.KindOfMove = 5; }      // Pawn 2 square advance move

                    // Pawn transformation detected
                    if ((i > 7 && i <=15)   && p== PieceType.Black && ((int)Math.Log(lsb, 2)/8) == 0){m.KindOfMove = 6; }          // Black Pawn transformation
                    if ((i > 7 && i <= 15)  && p == PieceType.White && ((int)Math.Log(lsb, 2) / 8) == 7) {m.KindOfMove = 6; }      // White Pawn transformation

                    // Ampesand pawn move detected
                    if (i > 7 && i <= 15 && (((gs.GetPiece(i, p).Position << 1) & lsb) != 0 || ((gs.GetPiece(i, p).Position >> 1) & lsb) != 0)) 
                    {
                        if (p == PieceType.Black) {  m.SecondaryPieceID = i; m.SecondaryNewPosition = lsb >> 8; m.KindOfMove = 3; }
                        if (p == PieceType.White) {  m.SecondaryPieceID = i; m.SecondaryNewPosition = lsb << 8; m.KindOfMove = 3; }
                    }

                    moves.Add(m);                                                                                                  // Add move to moves ArrayList
                    moveBB ^= lsb;                                                                                                 // Remove least significant bit
                }
            }

            // Implement sort function based on shallow MiniMax search
            // moves = sortMoves(moves);
            return moves;
        }
        public int SlidingPieceMobility(GameState gs, int p)
        {
            int mobility = 0;
            //Bishop mobility
            for (int i = 2; i <= 3; i++)
            {
                if (!(gs.GetPiece(i, p) is null))  {                                                                                // Bishop exists
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);                                                         // Position of white or black Bishop depending on player perspective
                    ulong bmp = ubmp[0][pos] & gs.BlockedSquares(2);                                                                // Bishop unblocked move pattern
                    ulong index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);                                          // "Magic" lookup index calculation
                    mobility += Bitcount(bishopMoves[pos, index] & ~gs.BlockedSquares(p));                                          // Lookup in precalculated "Magic" bishop lookup table
                }
            }
            //Rook mobility
            for (int i = 6; i <= 7; i++)
            {
                if (!(gs.GetPiece(i, p) is null))  {                                                                                 // Rook exists
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);                                                          // Position of white or black rook depending on player perspective
                    ulong bmp = ubmp[1][pos] & gs.BlockedSquares(PieceType.All);                                                     // Rook unblocked move pattern
                    ulong index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);                                               // "Magic" lookup index calculation
                    mobility += Bitcount(rookMoves[pos, index] & ~gs.BlockedSquares(p));                                             // Lookup in precalculated "Magic" rook lookup table
                }
            }
            //Queen mobility
            if (!(gs.GetPiece(1, p) is null))
            {                                                                                   // Queen exists
                ulong bm;                                                                                                           // Bishop moves
                ulong rm;                                                                                                           // Rook moves
                int pos = (int)Math.Log(gs.GetPiece(1, p).Position, 2);                                                             // Position of white or black queen depending on player perspective
                ulong bmp = ubmp[1][pos] & gs.BlockedSquares(PieceType.All);                                                        // Rook blocked squares
                ulong index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);                                                  // "Magic" lookup index calculation
                rm = rookMoves[pos, index] & ~gs.BlockedSquares(p);
                bmp = ubmp[0][pos] & gs.BlockedSquares(2);                                                                          // Bishop unblocked rook move pattern
                index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);                                                    // "Magic" lookup index calculation
                bm = bishopMoves[pos, index] & ~gs.BlockedSquares(p);
                mobility += Bitcount(rm | bm);
            }
            //Knight mobility
            for (int i = 4; i <= 5; i++)
            {
                if (!(gs.GetPiece(i, p) is null))                                                                                   // Knight exists
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);                                                         // Position of white or black Knight depending on player perspective
                    mobility += Bitcount(ubmp[4][pos] & ~gs.BlockedSquares(p));
                }
            }
            //Transformed pawn possible moves
            for (int i = 16; i <= 23; i++)
            {
                if (!(gs.GetPiece(i, p) is null))                                                                                       // Queen exists
                {
                    ulong bm;                                                                                                           // Bishop moves
                    ulong rm;                                                                                                           // Rook moves
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);                                                             // Position of white or black queen depending on player perspective
                    ulong bmp = ubmp[1][pos] & gs.BlockedSquares(PieceType.All);                                                        // Rook blocked squares
                    ulong index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);                                                  // "Magic" lookup index calculation
                    rm = rookMoves[pos, index] & ~gs.BlockedSquares(p);
                    bmp = ubmp[0][pos] & gs.BlockedSquares(2);                                                                          // Bishop unblocked rook move pattern
                    index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);                                                    // "Magic" lookup index calculation
                    bm = bishopMoves[pos, index] & ~gs.BlockedSquares(p);
                    mobility += Bitcount(rm | bm);
                }
            }
                    return mobility;
        }
        private ArrayList calculateOpponentMoves(GameState gs, int p, Boolean includeAttackOnOwnPieces = true)
        {
            ArrayList attackBB = new ArrayList();

            //Move Calculation: King            
            if (!(gs.GetPiece(0, p^1) is null))                                                                            //King exists
            {
                int pos = (int)Math.Log(gs.GetPiece(0, p^1).Position, 2);                                                  // King position
                ulong attack = ubmp[5][pos];
                if (!includeAttackOnOwnPieces) { attack &= ~gs.BlockedSquares(p ^ 1); }                                    // Don't include defence own pieces
                if (attack != 0) { attackBB.Add(attack); }                                                                 // Calculate leagal king moves   
            }
            //Queen possible moves
            if (!(gs.GetPiece(1, p^1) is null))                                                                           // Queen exists
            {
                int pos = (int)Math.Log(gs.GetPiece(1, p^1).Position, 2);                                                 // Position of white or black queen depending on player perspective
                ulong bmp = ubmp[1][pos] & (gs.BlockedSquares(2) & ~gs.GetPiece(0, p).Position);                          // Rook blocked squares with king removed
                ulong index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);                                        // "Magic" lookup index calculation
                ulong rookAttack = rookMoves[pos, index];
                bmp = ubmp[0][pos] & (gs.BlockedSquares(2) & ~gs.GetPiece(0, p).Position);                                // Bishop unblocked rook move pattern
                index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);                                          // "Magic" lookup index calculation            
                ulong bishopAttack = bishopMoves[pos, index];
                ulong attack = rookAttack | bishopAttack;
                if (!includeAttackOnOwnPieces) { attack &= ~gs.BlockedSquares(p ^ 1); }                                    // Don't include defence own pieces
                if (attack != 0) { attackBB.Add(attack); }
            }
            //Transformed pawn possible moves
            for (int i = 16; i <= 23; i++)
            {
                if (!(gs.GetPiece(i, p ^ 1) is null))                                                                       // Transformed pawn exists
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p ^ 1).Position, 2);                                             // Position of white or black queen depending on player perspective
                    ulong bmp = ubmp[1][pos] & (gs.BlockedSquares(2) & ~gs.GetPiece(0, p).Position);                        // Rook blocked squares with king removed
                    ulong index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);                                      // "Magic" lookup index calculation
                    ulong rookAttack = rookMoves[pos, index];
                    bmp = ubmp[0][pos] & (gs.BlockedSquares(2) & ~gs.GetPiece(0, p).Position);                              // Bishop unblocked rook move pattern
                    index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);                                        // "Magic" lookup index calculation            
                    ulong bishopAttack = bishopMoves[pos, index];
                    ulong attack = rookAttack | bishopAttack;
                    if (!includeAttackOnOwnPieces) { attack &= ~gs.BlockedSquares(p ^ 1); }                                 // Don't include defence own pieces
                    if (attack != 0) { attackBB.Add(attack); }
                }
            }
            //Bishop possible moves
            for (int i = 2; i <= 3; i++)
            {
                if (!(gs.GetPiece(i, p^1) is null))                                                                       // Bishop exists
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p^1).Position, 2);                                             // Position of white or black Bishop depending on player perspective
                    ulong bmp = ubmp[0][pos] & (gs.BlockedSquares(2) & ~gs.GetPiece(0, p).Position);                                                    // Bishop unblocked move pattern
                    ulong index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);                              // "Magic" lookup index calculation
                    ulong attack = bishopMoves[pos, index];
                    if (!includeAttackOnOwnPieces) { attack &= ~gs.BlockedSquares(p ^ 1); }                                    // Don't include defence own pieces
                    if (attack != 0) { attackBB.Add(attack); }                                     // Lookup in precalculated "Magic" bishop lookup table remove own pieces
                }
            }
            //Knight possible moves
            for (int i = 4; i <= 5; i++)
            {
                if (!(gs.GetPiece(i, p^1) is null))                                                                     // Knight exists
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p^1).Position, 2);                                           // Knight position
                    ulong attack = ubmp[4][pos];
                    if (!includeAttackOnOwnPieces) { attack &= ~gs.BlockedSquares(p ^ 1); }                             // Don't include defence own pieces
                    if (attack != 0) { attackBB.Add(attack); }                                                          // Lookup precalculated knight move pattern remove own pieces
                }
            }
            //Rook possible moves
            for (int i = 6; i <= 7; i++)
            {
                if (!(gs.GetPiece(i, p^1) is null))                                                                     // Rook exists
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p^1).Position, 2);                                           // Position of white or black rook depending on player perspective
                    //ulong test = gs.BlockedSquares(PieceType.All) & ~gs.GetPiece(0, p).Position;
                    ulong bmp = ubmp[1][pos] & (gs.BlockedSquares(PieceType.All) & ~gs.GetPiece(0, p).Position);                                        // Rook unblocked move pattern
                    ulong index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);                                  // "Magic" lookup index calculation
                    ulong attack = rookMoves[pos, index];
                    if (!includeAttackOnOwnPieces) { attack &= ~gs.BlockedSquares(p ^ 1); }                             // Don't include defence own pieces
                    if (attack != 0) { attackBB.Add(attack); }                                                          // Lookup in precalculated "Magic" rook lookup table remove own pieces
                }
            }
            //Pawn possible moves
            for (int i = 8; i <= 15; i++)
            {
                if (!(gs.GetPiece(i, p^1) is null))
                {                                                                                                       // Check if pawn exists
                    ulong[] files = new ulong[8]                                                                                                                // File bitboard filter
                    {
                    0x4040404040404040, 0xa0a0a0a0a0a0a0a0, 0x5050505050505050, 0x2828282828282828,0x1414141414141414, 0xa0a0a0a0a0a0a0a,0x505050505050505,0x202020202020202
                    };                                                                                                  // Files bitboard pattern filter                      
                    int pos = (int)Math.Log(gs.GetPiece(i, p^1).Position, 2);                                           // Pawn position 
                    int rank = pos / 8; int file = (rank + 1) * 8 - pos - 1;                                            // Calculate current file and rank based on position
                    ulong attack = (ubmp[3 - p^1][pos] & files[file]);
                    if (!includeAttackOnOwnPieces) { attack &= ~gs.BlockedSquares(p ^ 1); }                             // Don't include defence own pieces
                    if (attack != 0) { attackBB.Add(attack); }                                                          // Pawn sidewise capture remove own pieces                                                                                                                                                            
                }
            }
            return attackBB;
        }
        private Boolean[] RokadeBlocked(GameState gs, int p, ulong attackedBB)
        {
            ulong[,] castlingBlockers = new ulong[,] {              //Possible blocking positions
                { 0x600000000000000, 0x7000000000000000 },
                { 0x6, 0x70 }
            };
            Boolean[] blocked = new Boolean[2] { false, false };

            if (!gs.GetPiece(0, p).InitialPosition) { blocked[0] = true; blocked[1] = true; return blocked; } // King has been moved ): rokade not possible
            blocked[0] = 0 != ((gs.BlockedSquares(PieceType.All) | attackedBB) & castlingBlockers[p, 0]);                // Check if short side blocked by own or opponent pieces
            blocked[1] = 0 != ((gs.BlockedSquares(PieceType.All) | attackedBB) & castlingBlockers[p, 1]);                // Check if long side blocked by own or opponent pieces
            return blocked;

        }
        private struct pinnedSquares
        {
            public Piece attacker { get; set; }
            public ulong pinned {get;set;}
            public ulong attackBlockers { get; set; }
        }
        private ArrayList calculateKingAttacs(GameState gs, int p)
        {
            // Identify pinned squares (a pice moved from that position will cause king to be in chech, which is an illegal move)
            ArrayList listOfAttacks = new ArrayList();
            int[] slidingPiece = { 1, 2, 3, 6, 7, 16, 17, 18, 19, 20, 21, 22, 23};
            ulong kingPosBB = gs.GetPiece(0, p).Position; int kingPos = (int)Math.Log(kingPosBB, 2);                                        // King position as BB and Array index
            ulong bishopIndex = ((ubmp[0][kingPos] & gs.BlockedSquares(p ^ 1)) * bishopMagic[kingPos]) >> (64 - bishopIndexBits[kingPos]);  // "Magic" lookup index calculation for bishops                            
            ulong rookIndex = ((ubmp[1][kingPos] & gs.BlockedSquares(p ^ 1)) * rookMagic[kingPos]) >> (64 - rookIndexBits[kingPos]);        // "Magic" lookup index calculation for rooks
            ulong blockedQueenMoves = rookMoves[kingPos, rookIndex] | bishopMoves[kingPos, bishopIndex];                                    // Queen moves at king position blocked by opponent pieces
            ulong pkaBB = blockedQueenMoves & gs.BlockedSquares(p ^ 1);                                                                     // Potential opponent king attack pieces bitboard ????
            pinnedSquares attack = new pinnedSquares { };
            if (pkaBB != 0)
            {
                for (int i = 0; i <= 12; i++)
                {                                                                                                                           // Iterate all opponent sliding pieces
                    ulong ps = 0;
                    if (!(gs.GetPiece(slidingPiece[i], p ^ 1) is null))                                                                     // Check if sliding piece exists
                    {
                        ulong piecePosBB = gs.GetPiece(slidingPiece[i], p ^ 1).Position;                                                    // Opponent sliding piece with potential of pinning pieces
                        int piecePos = (int)Math.Log(piecePosBB, 2);                                                                        // Opponent sliding piece position [0,63]
                        ulong spb = (~blockedQueenMoves & ubmp[5][piecePos]);                                                               // Opponent sliding piece blockers
                        ulong bbm = spb | ((gs.BlockedSquares(p ^ 1) | kingPosBB) ^ piecePosBB);
                        bishopIndex = ((bbm & ubmp[0][piecePos]) * bishopMagic[piecePos]) >> (64 - bishopIndexBits[piecePos]);              // "Magic" lookup index calculation for bishops                            
                        rookIndex = ((bbm & ubmp[1][piecePos]) * rookMagic[piecePos]) >> (64 - rookIndexBits[piecePos]);                    // "Magic" lookup index calculation for rooks
                        if (slidingPiece[i] == 1) { ps = (rookMoves[piecePos, rookIndex] | bishopMoves[piecePos, bishopIndex]) & ~spb; }    // Queen attack bitboard
                        if (slidingPiece[i] == 2 || slidingPiece[i] == 3) { ps = bishopMoves[piecePos, bishopIndex] & ~spb; }               // Bishop attack bitboard
                        if (slidingPiece[i] == 6 || slidingPiece[i] == 7) { ps = rookMoves[piecePos, rookIndex] & ~spb; }                   // Rook attack bitboard
                        if (((piecePosBB & pkaBB) != 0) && ((ps & kingPosBB) != 0))
                        {
                            attack.attacker = gs.GetPiece(slidingPiece[i], p ^ 1);
                            attack.pinned = (ubmp[0][kingPos] | ubmp[1][kingPos]) & ps;
                            attack.attackBlockers = spb;
                            listOfAttacks.Add(attack);                                                                                      // Add possible attack rays
                        }
                    }
                }
            }
            return listOfAttacks;
        }
        private ArrayList calculatePinnedSquares(GameState gs, int p)                                                                       // Calculate pinned squares 
        {
            ArrayList listOfAttacks = new ArrayList();
            int[] RookLikePiece =   { 1, 6, 7, 16, 17, 18, 19, 20, 21, 22, 23};
            int[] BishopLikePiece = { 1, 2, 3, 16, 17, 18, 19, 20, 21, 22, 23 };
            int[] Knights = { 4, 5 };
            int[] Pawns = {8,9,10,11,12,13,14,15 };
            ulong kingPos = gs.GetPiece(0, p).Position; int kingPosI = (int)Math.Log(kingPos, 2);                                           // King position as BB and Array index
            pinnedSquares attack = new pinnedSquares { };

            for (int i = 0; i < 3; i++)                                                                                                     // Iterate all opponent pieces with rook like attack patterns
            {
                
                if (!(gs.GetPiece(RookLikePiece[i], p ^ 1) is null))                                                                        // Check if rook like piece exists
                {
                    ulong pos = gs.GetPiece(RookLikePiece[i], p ^ 1).Position; int posI = (int)Math.Log(pos, 2);                            // Position of possible opponent attacker piece
                    ulong hashIndex = ((ubmp[1][posI] & gs.BlockedSquares(p^1)) * rookMagic[posI]) >> (64 - rookIndexBits[posI]);           // "Magic" lookup index calculation for rooks - blocked by own pieces
                    ulong rAttack = rookMoves[posI, hashIndex];                                                                             // Rook moves seen from attacker piece - blocked by own pieces
                    hashIndex = ((ubmp[1][kingPosI] & gs.BlockedSquares(p^1)) * rookMagic[kingPosI]) >> (64 - rookIndexBits[kingPosI]);     // "Magic" lookup index calculation for rooks
                    ulong rDefence = rookMoves[kingPosI, hashIndex];                                                                        // Rook moves seen from king position
                    if ((rAttack & kingPos) != 0)
                    {
                        attack.attacker = gs.GetPiece(RookLikePiece[i], p ^ 1);                                                             // Add attacking piece
                        if (kingPos > pos) { attack.pinned = (rAttack & rDefence) & ((kingPos - 1) & ~(pos - 1)); } else                    // Add squares pinned through rook like attack patterns
                        { attack.pinned = (rAttack & rDefence) & (~(kingPos - 1) & (pos - 1)); }
                        listOfAttacks.Add(attack);
                    }
                }
            }
            for (int i = 0; i < 3; i++)                                                                                                     // Iterate all opponent pieces with bishop like attack patterns
            {
                if (!(gs.GetPiece(BishopLikePiece[i], p ^ 1) is null))                                                                      // Check if bishop like piece exists
                {
                    ulong pos = gs.GetPiece(BishopLikePiece[i], p ^ 1).Position; int posI = (int)Math.Log(pos, 2);                          // Position of possible opponent attacker piece
                    ulong hashIndex = ((ubmp[0][posI] & gs.BlockedSquares(p^1)) * bishopMagic[posI]) >> (64 - bishopIndexBits[posI]);       // "Magic" lookup index calculation for bishops - blocked by own pieces
                    ulong bAttack = bishopMoves[posI, hashIndex];                                                                           // Bishop moves seen from attacker piece - blocked by own pieces
                    hashIndex = ((ubmp[0][kingPosI] & gs.BlockedSquares(p ^ 1)) * bishopMagic[kingPosI]) >> (64 - bishopIndexBits[kingPosI]); // ??????????"Magic" lookup index calculation for bishops
                    ulong bDefence = bishopMoves[kingPosI, hashIndex];                                                                      // Bishop moves seen from king position
                    if ((bAttack & kingPos) != 0)
                    {
                        attack.attacker = gs.GetPiece(BishopLikePiece[i], p ^ 1);                                                           // Add attacking piece
                        if (kingPos > pos) { attack.pinned = (bAttack & bDefence) & ((kingPos - 1) & ~(pos - 1)); } else                      // Add squares pinned through bishop like attack patterns
                        { attack.pinned = (bAttack & bDefence) & (~(kingPos - 1) & (pos - 1)); }
                        listOfAttacks.Add(attack);
                    }
                }
            }
            for (int i = 0; i < 2; i++)                                                                                                     // Knight pieces attacks
            {
                if (!(gs.GetPiece(Knights[i], p ^ 1) is null))
                {
                    ulong pos = gs.GetPiece(Knights[i], p ^ 1).Position; int posI = (int)Math.Log(pos, 2);                                  // Position of possible opponent attacker piece
                    ulong knightAttack = ubmp[4][posI];                                                                                     // Knight attacks
                    if ((knightAttack & kingPos) != 0)
                    {
                        attack.attacker = gs.GetPiece(Knights[i], p ^ 1);                                                                   // Add attacking piece
                        attack.pinned = 0;
                        listOfAttacks.Add(attack);
                    }
                }
            }

            for (int i = 0; i < 8; i++)                                                                                                     // Pawn pieces attacks
            {
                if (!(gs.GetPiece(Pawns[i], p ^ 1) is null))
                {
                    ulong pos = gs.GetPiece(Pawns[i], p ^ 1).Position; int posI = (int)Math.Log(pos, 2);                                    // Position of possible opponent attacker piece
                    ulong pawnAttack = ubmp[3 - p ^ 1][posI];                                                                               // Knight attacks
                    if (p == 0) { pawnAttack &= ~pos << 8; }
                    if (p == 1) { pawnAttack &= ~pos >> 8; }
                    if ((pawnAttack & kingPos) != 0)
                    {
                        attack.attacker = gs.GetPiece(Pawns[i], p ^ 1);                                                                     // Add attacking piece
                        attack.pinned = 0;
                        listOfAttacks.Add(attack);
                    }
                }
            }
            return listOfAttacks;
        }
        private int Bitcount(ulong bb)
        {
            int count = 0;
            while (bb != 0) { count++; bb &= (bb - 1); }
            return count;
        }
        private int PosOfSetBitNr(int i, ulong bb)
        {

            int count = -1;
            for (int bit = 0; bit < 64; bit++)
            {
                if ((bb >> bit & 1) == 1) { count++; }
                if (count == i) { return bit; }
            }
            return -1;
        }
        private void InitUnblockedMoves()
        {
            // Bishop moves
            ubmp[0] = new ulong[64]{
                0x40201008040200,0x402010080400,0x4020100a00,0x40221400,0x2442800,0x204085000,0x20408102000,0x2040810204000,
                0x20100804020000,0x40201008040000,0x4020100a0000,0x4022140000,0x244280000,0x20408500000,0x2040810200000,0x4081020400000,
                0x10080402000200,0x20100804000400,0x4020100a000a00,0x402214001400,0x24428002800,0x2040850005000,0x4081020002000,0x8102040004000,
                0x8040200020400,0x10080400040800,0x20100a000a1000,0x40221400142200,0x2442800284400,0x4085000500800,0x8102000201000,0x10204000402000,
                0x4020002040800,0x8040004081000,0x100a000a102000,0x22140014224000,0x44280028440200,0x8500050080400,0x10200020100800,0x20400040201000,
                0x2000204081000,0x4000408102000,0xa000a10204000,0x14001422400000,0x28002844020000,0x50005008040200,20002010080400,0x40004020100800,
                0x20408102000,0x40810204000,0xa1020400000,0x142240000000,0x284402000000,0x500804020000,0x201008040200,0x402010080400,
                0x2040810204000,0x4081020400000,0xa102040000000,0x14224000000000,0x28440200000000,0x50080402000000,0x20100804020000,0x40201008040200};
            // Rock moves
            ubmp[1] = new ulong[64]{
                  0x101010101017e,0x202020202027c,0x404040404047a,0x8080808080876,0x1010101010106e,0x2020202020205e,0x4040404040403e,0x8080808080807e,
                  0x1010101017e00,0x2020202027c00,0x4040404047a00,0x8080808087600,0x10101010106e00,0x20202020205e00,0x40404040403e00,0x80808080807e00,
                  0x10101017e0100,0x20202027c0200,0x40404047a0400,0x8080808760800,0x101010106e1000,0x202020205e2000,0x404040403e4000,0x808080807e8000,
                  0x101017e010100,0x202027c020200,0x404047a040400,0x8080876080800,0x1010106e101000,0x2020205e202000,0x4040403e404000,0x8080807e808000,
                  0x1017e01010100,0x2027c02020200,0x4047a04040400,0x8087608080800,0x10106e10101000,0x20205e20202000,0x40403e40404000,0x80807e80808000,
                  0x17e0101010100,0x27c0202020200,0x47a0404040400,0x8760808080800,0x106e1010101000,0x205e2020202000,0x403e4040404000,0x807e8080808000,
                  0x7e010101010100,0x7c020202020200,0x7a040404040400,0x76080808080800,0x6e101010101000,0x5e202020202000,0x3e404040404000,0x7e808080808000,
                  0x7e01010101010100,0x7c02020202020200,0x7a04040404040400,0x7608080808080800,0x6e10101010101000,0x5e20202020202000,0x3e40404040404000,0x7e80808080808000}; //Updated
            // Pawn white moves
            ubmp[2] = new ulong[64]{
                0,0,0,0,0,0,0,0,
                16973824,34013184,68026368,136052736,272105472,544210944,1088421888,2160066560,
                50331648,117440512,234881024,469762048,939524096,1879048192,3758096384,3221225472,
                12884901888,30064771072,60129542144,120259084288,240518168576,481036337152,962072674304,824633720832,
                3298534883328,7696581394432,15393162788864,30786325577728,61572651155456,123145302310912,246290604621824,211106232532992,
                844424930131968,1970324836974592,3940649673949184,7881299347898368,15762598695796736,31525197391593472,63050394783186944,54043195528445952,
                216172782113783808,504403158265495552,1008806316530991104,2017612633061982208,4035225266123964416,8070450532247928832,16140901064495857664,13835058055282163712,
                0,0,0,0,0,0,0,0,};
            // Pawn Black moves
            ubmp[3] = new ulong[64]{
                0,0,0,0,0,0,0,0,
                3,7,14,28,56,112,224,192,
                768,1792,3584,7168,14336,28672,57344,49152,
                196608,458752,917504,1835008,3670016,7340032,14680064,12582912,
                50331648,117440512,234881024,469762048,939524096,1879048192,3758096384,3221225472,
                12884901888,30064771072,60129542144,120259084288,240518168576,481036337152,962072674304,824633720832,
                3302829850624,7705171329024,15410342658048,30820685316096,61641370632192,123282741264384,246565482528768,211655988346880,
                0,0,0,0,0,0,0,0,};
            // Knight moves
            ubmp[4] = new ulong[64]{
                132096,329728,659712,1319424,2638848,5277696,10489856,4202496,
                33816580,84410376,168886289,337772578,675545156,1351090312,2685403152,1075839008,
                8657044482,21609056261,43234889994,86469779988,172939559976,345879119952,687463207072,275414786112,
                2216203387392,5531918402816,11068131838464,22136263676928,44272527353856,88545054707712,175990581010432,70506185244672,
                567348067172352,1416171111120896,2833441750646784,5666883501293568,11333767002587136,22667534005174272,45053588738670592,18049583422636032,
                145241105196122112,362539804446949376,725361088165576704,1450722176331153408,2901444352662306816,5802888705324613632,11533718717099671552,4620693356194824192,
                288234782788157440,576469569871282176,1224997833292120064,2449995666584240128,4899991333168480256,9799982666336960512,1152939783987658752,2305878468463689728,
                1128098930098176,2257297371824128,4796069720358912,9592139440717824,19184278881435648,38368557762871296,4679521487814656,9077567998918656};
            // King moves
            ubmp[5] = new ulong[64] {
                770,1797,3594,7188,14376,28752,57504,49216,
                197123,460039,920078,1840156,3680312,7360624,14721248,12599488,
                50463488,117769984,235539968,471079936,942159872,1884319744,3768639488,3225468928,
                12918652928,30149115904,60298231808,120596463616,241192927232,482385854464,964771708928,825720045568,
                3307175149568,7718173671424,15436347342848,30872694685696,61745389371392,123490778742784,246981557485568,211384331665408,
                846636838289408,1975852459884544,3951704919769088,7903409839538176,15806819679076352,31613639358152704,63227278716305408,54114388906344448,
                216739030602088448,505818229730443264,1011636459460886528,2023272918921773056,4046545837843546112,8093091675687092224,16186183351374184448,13853283560024178688,
                144959613005987840,362258295026614272,724516590053228544,1449033180106457088,2898066360212914176,5796132720425828352,11592265440851656704,4665729213955833856};
        }
        private void InitRookLookupTable()
        {
            ulong index = 0;
            ulong bmp = 0;
            for (int pos = 0; pos < 64; pos++)
            {                                            //Iterate all 64 squares 
                ulong ubm = ubmp[1][pos];                                                   //Unblocked rook move pattern 
                int nPerm = (int)Math.Pow(2, Bitcount(ubm));                                //Number og possible permutations of the unblocked bitboard pattern with boarder removed
                for (int perm = 0; perm < nPerm; perm++)
                {                                   //Iterate all possible permutation of blocking piece patterns
                    bmp = 0;
                    for (int bit = 0; bit < Bitcount(ubm); bit++)
                    {
                        int bitPos = PosOfSetBitNr(bit, ubm);                               // Position of the n'th significant bit in <ubm>
                        if ((perm >> bit & 1) == 1) { bmp |= (ulong)Math.Pow(2, bitPos); }
                    }
                    index = (bmp * rookMagic[pos]) >> (64 - rookIndexBits[pos]);
                    rookMoves[pos, index] = CalculateRookMovesSlow(bmp, pos);
                }
            }

        }
        private void InitBishopLookupTable()
        {
            ulong index = 0;
            ulong bmp = 0;
            for (int pos = 0; pos < 64; pos++)                                              //Iterate all 64 squares
            {
                ulong ubm = ubmp[0][pos];                                                   //Unblocked bishop move pattern 
                int nPerm = (int)Math.Pow(2, Bitcount(ubm));                                //Number og possible permutations of the unblocked bitboard pattern with boarder removed
                for (int perm = 0; perm < nPerm; perm++)
                {                                  //Iterate all possible permutation of blocking piece patterns                                                                                       
                    bmp = 0;
                    for (int bit = 0; bit < Bitcount(ubm); bit++)
                    {
                        int bitPos = PosOfSetBitNr(bit, ubm);                               // Position of the n'th significant bit in <ubm>
                        if ((perm >> bit & 1) == 1) { bmp |= (ulong)Math.Pow(2, bitPos); }
                    }
                    index = (bmp * bishopMagic[pos]) >> (64 - bishopIndexBits[pos]);
                    bishopMoves[pos, index] = CalculateBishopMovesSlow(bmp, pos);
                }
            }
        }
        private ulong CalculateRookMovesSlow(ulong bmp, int pos)
        {
            ulong[] boarder = { 0xffffffffffffff, 0xfefefefefefefefe, 0xffffffffffffff00, 0x7f7f7f7f7f7f7f7f };
            int[] direction = { 8, 1, 8, 1 };
            ulong moves = 0;
            ulong iterator = (ulong)Math.Pow(2, pos);
            for (int i = 0; i < 4; i++)
            {  //Fill all directions
                iterator = (ulong)Math.Pow(2, pos);
                if (i == 0 || i == 3) { while ((boarder[i] & iterator) != 0 && (iterator & bmp) == 0) { iterator <<= direction[i]; moves |= iterator; } } //Norh or West
                if (i == 1 || i == 2) { while ((boarder[i] & iterator) != 0 && (iterator & bmp) == 0) { iterator >>= direction[i]; moves |= iterator; } } //South or east
            }
            return moves;
        }
        private ulong CalculateBishopMovesSlow(ulong bmp, int pos)
        {
            ulong[] boarder = { 0xfefefefefefefe, 0xfefefefefefefe00, 0x7f7f7f7f7f7f7f00, 0x7f7f7f7f7f7f7f };
            int[] direction = { 7, 9, 7, 9 };
            ulong moves = 0;
            ulong iterator = (ulong)Math.Pow(2, pos);
            for (int i = 0; i < 4; i++)
            {  //Fill in bishop moves - All directions
                iterator = (ulong)Math.Pow(2, pos);
                if (i == 0 || i == 3) { while ((boarder[i] & iterator) != 0 && (iterator & bmp) == 0) { iterator <<= direction[i]; moves |= iterator; } } //Norh east or south west
                if (i == 1 || i == 2) { while ((boarder[i] & iterator) != 0 && (iterator & bmp) == 0) { iterator >>= direction[i]; moves |= iterator; } } //South east or north west
            }
            return moves;
        }
        public void WriteBitboard(ulong bb)
        {
            String value = "";
            Debug.WriteLine(value);
            int count = 0;
            for (int i = 63; i >= 0; i--)
            {
                count++;
                value += String.Format("{0,3}", (bb >> i) & 1);
                if (count == 8) { Debug.WriteLine(value); count = 0; value = ""; }
            }
        }
        public MoveGenerator()                  //Constructor
        {
            InitUnblockedMoves();               //Initialize unblocked move patternes 
            InitRookLookupTable();              //Initialize Rook lookup tables based on Blocked Bitbord, Magic Number and IndexBits
            InitBishopLookupTable();            //Initialize Bishp lookup tables based on Blocked Bitbord, Magic Number and IndexBits
        }
    }
}
