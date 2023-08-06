using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace HaaToChess
{
    interface IStateEvaluator {
        int GameStateScore(GameState gs, int perspective); // Retur value <-1..1> :) -1 = Loss, 1 = Win seen from computer perspective
        int gameProgress { get; set; }
    }

    
    class StateEvaluator {
        // Fields
        private int[] fParam; //= new int[2];                             // Array with evaluation function parameters
        private MoveGenerator mg;
        //Properties
        public int gameProgress { get; set; }
        // Private methodes
        private int Material(GameState gs, int p)                        // Material Score 
        {
            int maximizerValue = 0;
            int minimizerValue = 0;
            int sign;
            if (p == gs.AIPlayer) { sign = 1; } else { sign = -1; } //Swap score sign according to maximizer or minimizer perspective

            for (int i = 0; i <= 23; i++)
            {
                if (!(gs.GetPiece(i, p) is null)){ maximizerValue += gs.GetPiece(i,p).Value ; }
                if (!(gs.GetPiece(i, p^1) is null)) { minimizerValue += gs.GetPiece(i, p ^ 1).Value; }
            }
            return sign*(maximizerValue - minimizerValue);
        }
        private int Position(GameState gs, int p)                        // Position Score 
        {
            //Debug.WriteLine(gs.StateAsString());
            int maximizerValue = 0;
            int minimizerValue = 0;
            int sign;
            if (p == gs.AIPlayer) { sign = 1; } else { sign = -1; } //Swap score sign according to maximizer or minimizer perspective
            int[,][] posValue = new int[2, 9][];

            posValue[0, 0] = new int[64]{
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0
             };                             // King square value - Black MÅ OPPDATERES!!!
            posValue[1, 0] = new int[64]{
             0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0,
            0, 0, 0, 0, 0, 0 ,0, 0
             };                           // King square value - White MÅ OPPDATERES!!!
            posValue[0,1] = new int[64]{
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -10,  5,  5,  5,  5,  5,  0,-10,
              0,  0,  5,  5,  5,  5,  0,  0,
              0,  0,  5,  5,  5,  5,  0,  0,
            -10,  0,  5,  5,  5,  5,  0,-10,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
             };                             // Queen square value - Black
            posValue[1,1] = new int[64]{                                  
            -20,-10,-10, -5, -5,-10,-10,-20,
            -10,  0,  0,  0,  0,  0,  0,-10,
            -10,  0,  5,  5,  5,  5,  0,-10,
             -5,  0,  5,  5,  5,  5,  0, -5,
              0,  0,  5,  5,  5,  5,  0, -5,
            -10,  5,  5,  5,  5,  5,  0,-10,
            -10,  0,  5,  0,  0,  0,  0,-10,
            -20,-10,-10, -5, -5,-10,-10,-20
                };                             // Queen square value - White
            posValue[0,2] = new int[64]{
                -20,-10,-10,-10,-10,-10,-10,-20,
                -10,  5,  0,  0,  0,  0,  5,-10,
                -10, 10, 10, 10, 10, 10, 10,-10,
                -10,  0, 10, 10, 10, 10,  0,-10,
                -10,  5,  5, 10, 10,  5,  5,-10,
                -10,  0,  5, 10, 10,  5,  0,-10,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -20,-10,-10,-10,-10,-10,-10,-20
                };                             // Bishop square value - Black
            posValue[1,2] = new int[64]{                                  
                -20,-10,-10,-10,-10,-10,-10,-20,
                -10,  0,  0,  0,  0,  0,  0,-10,
                -10,  0,  5, 10, 10,  5,  0,-10,
                -10,  5,  5, 10, 10,  5,  5,-10,
                -10,  0, 10, 10, 10, 10,  0,-10,
                -10, 10, 10, 10, 10, 10, 10,-10,
                -10,  5,  0,  0,  0,  0,  5,-10,
                -20,-10,-10,-10,-10,-10,-10,-20
                };                             // Bishop square value - White
            posValue[0,4] = new int[64]{              
                -50,-40,-30,-30,-30,-30,-40,-50,
                -40,-20,  0,  5,  5,  0,-20,-40,
                -30,  0, 10, 15, 15, 10,  0,-30,
                -30,  5, 15, 20, 20, 15,  5,-30,
                -30,  0, 15, 20, 20, 15,  0,-30,
                -30,  5, 10, 15, 15, 10,  5,-30,
                -40,-20,  0,  0,  0,  0,-20,-40,
                -50,-40,-30,-30,-30,-30,-40,-50,
                };                             // Knight square value - Black
            posValue[1,4] = new int[64]{                                  
                -50,-40,-30,-30,-30,-30,-40,-50,
                -40,-20,  0,  0,  0,  0,-20,-40,
                -30,  0, 10, 15, 15, 10,  0,-30,
                -30,  5, 15, 20, 20, 15,  5,-30,
                -30,  0, 15, 20, 20, 15,  0,-30,
                -30,  5, 10, 15, 15, 10,  5,-30,
                -40,-20,  0,  5,  5,  0,-20,-40,
                -50,-40,-30,-30,-30,-30,-40,-50,
                };                             // Knght square value - White
            posValue[0,6] = new int[64]{                                  
              0,  0,  0,  5,  5,  0,  0,  0,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
              5, 10, 10, 10, 10, 10, 10,  5,
              0,  0,  0,  0,  0,  0,  0,  0
                };                             // Rook square value - Black
            posValue[1,6] = new int[64]{                                  
               0,  0,  0,  0,  0,  0,  0,  0,
              5, 10, 10, 10, 10, 10, 10,  5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
             -5,  0,  0,  0,  0,  0,  0, -5,
              0,  0,  0,  5,  5,  0,  0,  0
                };                             // Rook square value - White
            posValue[0,8] = new int[64]{                                  
                 0,  0,  0,  0,  0,  0,  0,  0,
                 5, 10, 10,-20,-20, 10, 10,  5,
                 5, -5,-10,  0,  0,-10, -5,  5,
                 0,  0,  0, 20, 20,  0,  0,  0,
                 5,  5, 10, 25, 25, 10,  5,  5,
                10, 10, 20, 30, 30, 20, 10, 10,
                30, 30, 30, 30, 30, 30, 30, 30,
                10, 10, 10, 10, 10, 10, 10, 10
                };                             // Pawn square value - Black
            posValue[1,8] = new int[64]{                                  
                10, 10, 10, 10, 10, 10, 10, 10,
                30, 30, 30, 30, 30, 30, 30, 30,
                10, 10, 20, 30, 30, 20, 10, 10,
                 5,  5, 10, 25, 25, 10,  5,  5,
                 0,  0,  0, 20, 20,  0,  0,  0,
                 5, -5,-10,  0,  0,-10, -5,  5,
                 5, 10, 10,-20,-20, 10, 10,  5,
                 0,  0,  0,  0,  0,  0,  0,  0
                };                             // Pawn square value - White

            for (int i = 0; i <= 23; i++) {
                if (!(gs.GetPiece(i, p) is null)) 
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p).Position, 2);
                    int pID = i;
                    if (gs.GetPiece(i, p).pieceID == 3) { pID = 2; }
                    if (gs.GetPiece(i, p).pieceID == 5) { pID = 4; }
                    if (gs.GetPiece(i, p).pieceID == 7) { pID = 6; }
                    if (gs.GetPiece(i, p).pieceID > 8) { pID = 8; }
                    int test = posValue[p, pID][pos];
                    maximizerValue += posValue[p,pID][pos];
                }
            }
            for (int i = 0; i <= 23; i++)
            {
                if (!(gs.GetPiece(i, p^1) is null))
                {
                    int pos = (int)Math.Log(gs.GetPiece(i, p^1).Position, 2);
                    int pID = i;
                    if (gs.GetPiece(i, p^1).pieceID == 3) { pID = 2; }
                    if (gs.GetPiece(i, p^1).pieceID == 5) { pID = 4; }
                    if (gs.GetPiece(i, p^1).pieceID == 7) { pID = 6; }
                    if (gs.GetPiece(i, p^1).pieceID > 8) { pID = 8; }
                    minimizerValue += posValue[p^1, pID][pos];
                }
            }
            //Debug.WriteLine(",  Position:" + (maximizerValue- minimizerValue));
            return sign*(maximizerValue- minimizerValue);
        }
        private int Mobility(GameState gs, int p)
        {
            int sign;
            if (p == gs.AIPlayer) { sign = 1; } else { sign = -1; } //Swap score sign according to maximizer or minimizer perspective
            return sign*(mg.SlidingPieceMobility(gs, p)-mg.SlidingPieceMobility(gs,p^1));
        }
        private void optimizeEvaluationFunction()
        {
            // Further development with parameter estimation based on previous played games
        }
        //Public methodes
        public int GameStateScore(GameState gs,int p) {
            int score=0;
            score = (fParam[0] * Material(gs, p)+ fParam[1] * Position(gs, p) + fParam[2]*Mobility(gs,p));
            //Debug.Write("Material: " + fParam[0] * Material(gs, p) + "  Position: " + fParam[1] * Position(gs, p) + " Mobility: " + fParam[2] * Mobility(gs, p));
            return score;
        }

        public StateEvaluator(int[] featureParameteres, MoveGenerator mg)
        {
            //optimizeEvaluationFunction();
            this.mg = mg;
            fParam = featureParameteres;
            gameProgress = 0;
        }
    }
}
