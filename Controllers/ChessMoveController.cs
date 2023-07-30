using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HaaToChess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
// Haavard var her
namespace repos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChessMoveController : ControllerBase
    {

        private readonly ILogger<ChessMoveController> _logger;

        public ChessMoveController(ILogger<ChessMoveController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "getBestMove")]
        public ClientMove getBestMove(ChessBoardState currentState)
        {

            int[] fparam = new int[] { 1, 1, 1 };                                   // Set evaluation paramter weightings (Material, Position, Mobility) 
            int searchDepth = 5;                                                    // MiniMax Aplpha Beta search depth
            GameState currentBoard = new GameState(currentState.fenString);         // Initiate new game state using FEN string
            MoveSelector mg = new MoveSelector(fparam);                             // Initiate new move selector with evaluation weights
            ClientMove bestMove = mg.getBestMove(currentBoard, searchDepth) ;       // Return best move based on currentBoard state
            return bestMove;
        }
    }
}
