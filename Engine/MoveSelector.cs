using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;

namespace HaaToChess
{
    interface IMoveSelector
    {
        Move GetMove(GameState chessBoard,int perspective);
    }
    class MoveSelector
    {
        // Initiate movegenerator & chessboard
        private MoveGenerator mg;
        private StateEvaluator evaluator;
        private Random rnd = new Random();
        private const int infinite = 9999999;

        private double Max(double a, double b)
        {
            if (a > b) { return a; } else { return b; }
        }

        private double Min(double a, double b)
        {
            if (a < b) { return a; } else { return b; }
        }

        public ClientMove getClientMove(GameState gs,Move m)       // Translate move into Standard Algebraic Notation (SAN)
        {
            string[] sqID = new string[] { "a8", "b8", "c8", "d8", "e8", "f8", "g8", "h8" ,
                                             "a7", "b7", "c7", "d7", "e7", "f7", "g7", "h7" ,
                                             "a6",  "b6", "c6", "d6", "e6", "f6", "g6", "h6" ,
                                             "a5", "b5", "c5", "d5", "e5", "f5", "g5", "h5" ,
                                             "a4", "b4", "c4", "d4", "e4", "f4", "g4", "h4" ,
                                             "a3", "b3", "c3", "d3", "e3", "f3", "g3", "h3" ,
                                             "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2" ,
                                             "a1", "b1", "c1", "d1", "e1", "f1", "g1", "h1" };

            //string[] whitePieces = new string[16] { "K", "Q", "B", "B", "N", "N", "R", "R", "P", "P", "P", "P", "P", "P", "P", "P" };
            //string[] blackPieces = new string[16] { "k", "q", "b", "b", "n", "n", "r", "r", "p", "p", "p", "p", "p", "p", "p", "p" };

            Piece p = gs.GetPiece(m.PrimaryPieceID, gs.AIPlayer);
            int fromPos = (int)Math.Log(p.Position, 2);
            int toPos = (int)Math.Log(m.PrimaryNewPosition, 2);
            return new ClientMove(sqID[fromPos],sqID[toPos],p.Color);
        }

        public Move GetRandomMove(GameState chessBoard)
        {
            ArrayList moves;
            moves = mg.LeagalMoves(chessBoard, chessBoard.AIPlayer);                              // Calculate legal moves
            if (moves.Count == 0) { return null; }
            int r = rnd.Next(0, moves.Count);
            return (Move)moves[r];                                                        // Return a random leagal move
        }


        public ClientMove getBestMove(GameState board,int maxDepth)
        {
            int bestScore = int.MinValue;
            int bestIndex = 0;
            Move bestMove = null;
            ArrayList leagalMoves;

            leagalMoves = mg.LeagalMoves(board, board.AIPlayer);
            int cutoffCount = 0;
            bestScore = int.MinValue;
            for (int i=0;i<leagalMoves.Count;i++)
            {
                GameState cloneBoard = new GameState(board);
                cloneBoard.ExecuteMove((Move)leagalMoves[i],board.AIPlayer);
                
                int score = Minimax(ref cutoffCount, cloneBoard, maxDepth - 1, int.MinValue, int.MaxValue, false,board.AIPlayer);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                    bestMove = (Move)leagalMoves[bestIndex];
                }
            }
            
            return this.getClientMove(board, bestMove);
        }

        private int Minimax(ref int cutoffCount, GameState board, int depth, int alpha, int beta, bool isMaximizingPlayer,int AIColor)
        {
            int colorToProcess;
            if (isMaximizingPlayer) { colorToProcess = AIColor; } else { colorToProcess = AIColor ^ 1; }

            ArrayList leagalMoves = mg.LeagalMoves(board, colorToProcess);
            GameState cloneBoard = new GameState(board);
            if (depth == 0 || (leagalMoves.Count == 0)) // Må endres til å sjekke om det er sjakk matt
            {
                return evaluator.GameStateScore(board, AIColor);
            }

            if (isMaximizingPlayer)
            {
                int bestScore = int.MinValue;
                for (int i = 0; i < leagalMoves.Count; i++)
                {
                    cloneBoard.ExecuteMove((Move)leagalMoves[i], colorToProcess);
                    int score = Minimax(ref cutoffCount, cloneBoard, depth - 1, alpha, beta, false,AIColor);
                    bestScore = Math.Max(bestScore, score);
                    alpha = Math.Max(alpha, bestScore);
                    if (beta <= alpha) { cutoffCount++; break; } // Beta cutoff              
                }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                for (int i = 0; i < leagalMoves.Count; i++)
                {
                    cloneBoard.ExecuteMove((Move)leagalMoves[i], colorToProcess);
                    int score = Minimax(ref cutoffCount, cloneBoard, depth - 1, alpha, beta, true,AIColor);
                    bestScore = Math.Min(bestScore, score);
                    beta = Math.Min(beta, bestScore);
                    if (beta <= alpha){ cutoffCount++; break; } // Alpha cutoff
                }
                return bestScore;
            }
        }

        public MoveSelector(int[] featureParameteres)
        {
            mg = new MoveGenerator();
            evaluator = new StateEvaluator(featureParameteres, mg);
        }
    }
}
