using Microsoft.AspNetCore.Mvc;
using TicTacToeServer.Models;
using TicTacToeServer.Services;

namespace TicTacToeServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameEngineService _gameEngine;

        public GameController(GameEngineService gameEngine)
        {
            _gameEngine = gameEngine;
        }

        // Ендпоінт для здійснення ходу: POST /api/game/move
        [HttpPost("move")]
        public async Task<IActionResult> MakeMove([FromBody] MoveRequest request)
        {
            // Перевірка формату
            if (char.ToUpper(request.Player) != 'X' && char.ToUpper(request.Player) != 'O')
                return BadRequest("Символ гравця має бути 'X' або 'O'.");

            // Намагаємося додати в чергу
            bool isQueued = await _gameEngine.QueueMoveAsync(request);

            if (isQueued)
                return Ok("Хід успішно додано до черги.");
            else
                return StatusCode(503, "Черга переповнена (максимум 50) або гра вже завершена.");
        }

        // Ендпоінт для перевірки стану гри: GET /api/game/status
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { Status = _gameEngine.GetStatus() });
        }

        [HttpGet("board")]
        public IActionResult GetBoard()
        {
            var currentBoard = _gameEngine.GetActiveBoard();

            return Ok(new
            {
                TotalMoves = currentBoard.Count,
                Moves = currentBoard
            });
        }
    }
}