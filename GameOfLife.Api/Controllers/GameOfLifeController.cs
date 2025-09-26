using GameOfLife.Api.Models;
using GameOfLife.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace GameOfLife.Api.Controllers
{
    [ApiController]
    [Route("api/gameoflife")]
    public class GameOfLifeController : ControllerBase
    {
        private readonly GameOfLifeService _gameOfLifeService;
        private readonly IConfiguration _configuration;

        public GameOfLifeController(GameOfLifeService gameOfLifeService, IConfiguration configuration)
        {
            _gameOfLifeService = gameOfLifeService;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a new Game of Life board from an initial state.
        /// </summary>
        /// <param name="boardStateDto">A DTO containing the initial cell layout.</param>
        /// <returns>The ID of the newly created board.</returns>
        /// <response code="201">Returns the newly created board's ID.</response>
        /// <response code="400">If the input data is invalid or exceeds size limits.</response>
        [HttpPost("boards")]
        [ProducesResponseType(typeof(Guid), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateBoard([FromBody] BoardStateDto boardStateDto)
        {
            if (boardStateDto == null || boardStateDto.Cells == null || boardStateDto.Cells.Length == 0)
            {
                return BadRequest("Invalid board state provided.");
            }

            int maxHeight = _configuration.GetValue<int>("GameSettings:MaxBoardHeight", 200);
            int maxWidth = _configuration.GetValue<int>("GameSettings:MaxBoardWidth", 200);

            if (boardStateDto.Cells.Length > maxHeight || boardStateDto.Cells[0].Length > maxWidth)
            {
                return BadRequest($"Board dimensions cannot exceed {maxWidth}x{maxHeight}.");
            }

            var board = BoardExtensions.FromDto(boardStateDto);
            var createdBoard = await _gameOfLifeService.CreateBoardAsync(board);

            return CreatedAtAction(nameof(GetBoardById), new { id = createdBoard.Id }, new { boardId = createdBoard.Id });
        }

        /// <summary>
        /// Gets the current state of a specific board.
        /// </summary>
        /// <param name="id">The ID of the board to retrieve.</param>
        /// <returns>The current state of the board.</returns>
        /// <response code="200">Returns the board state.</response>
        /// <response code="404">If the board is not found.</response>
        [HttpGet("boards/{id}")]
        [ProducesResponseType(typeof(BoardStateDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBoardById(Guid id)
        {
            var board = await _gameOfLifeService.GetBoardByIdAsync(id);
            if (board == null)
            {
                return NotFound();
            }
            return Ok(board.ToDto());
        }


        /// <summary>
        /// Calculates the next generation for a specific board.
        /// </summary>
        /// <param name="id">The ID of the board to advance.</param>
        /// <returns>The board state after one generation.</returns>
        /// <response code="200">Returns the next generation's state.</response>
        /// <response code="404">If the board with the given ID is not found.</response>
        [HttpGet("boards/{id}/next")]
        [ProducesResponseType(typeof(BoardStateDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetNextGeneration(Guid id)
        {
            var board = await _gameOfLifeService.GetNextGenerationAsync(id);
            if (board == null)
            {
                return NotFound();
            }
            return Ok(board.ToDto());
        }

        /// <summary>
        /// Advances the board by a specified number of generations.
        /// </summary>
        /// <param name="id">The ID of the board to advance.</param>
        /// <param name="generations">The number of generations to advance.</param>
        /// <returns>The board state after the specified number of generations.</returns>
        /// <response code="200">Returns the final board state.</response>
        /// <response code="400">If the requested number of generations exceeds the limit.</response>
        /// <response code="404">If the board is not found.</response>
        [HttpGet("boards/{id}/states/{generations}")]
        [ProducesResponseType(typeof(BoardStateDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetGenerationsAway(Guid id, int generations)
        {
            int maxGenerations = _configuration.GetValue<int>("GameSettings:MaxGenerationsRequest", 5000);
            if (generations > maxGenerations)
            {
                return BadRequest($"Cannot request more than {maxGenerations} generations at a time.");
            }

            var board = await _gameOfLifeService.GetGenerationsAwayAsync(id, generations);
            if (board == null)
            {
                return NotFound();
            }
            return Ok(board.ToDto());
        }

        /// <summary>
        /// Calculates the final, stable state of a board.
        /// </summary>
        /// <remarks>
        /// The simulation stops if the board becomes stable (no changes), enters a repeating loop,
        /// or reaches a configured maximum number of generations.
        /// </remarks>
        /// <param name="id">The ID of the board.</param>
        /// <returns>The final board state.</returns>
        /// <response code="200">Returns the final stable or oscillating state.</response>
        /// <response code="404">If the board is not found.</response>
        /// <response code="409">If the board does not stabilize within the configured limit.</response>
        [HttpGet("boards/{id}/final")]
        [ProducesResponseType(typeof(BoardStateDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(string), 409)]
        public async Task<IActionResult> GetFinalState(Guid id)
        {
            try
            {
                var board = await _gameOfLifeService.GetFinalStateAsync(id);
                if (board == null)
                {
                    return NotFound();
                }
                return Ok(board.ToDto());
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}

