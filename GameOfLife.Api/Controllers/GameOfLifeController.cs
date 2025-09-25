using GameOfLife.Api.Data;
using GameOfLife.Api.Models;
using GameOfLife.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GameOfLife.Api.Controllers
{
    /// <summary>
    /// Manages the lifecycle of Conway's Game of Life boards.
    /// </summary>
    [ApiController]
    [Route("api/boards")]
    [Produces("application/json")]
    public class GameOfLifeController : ControllerBase
    {
        private readonly GameOfLifeService _gameOfLifeService;
        private readonly GameOfLifeContext _context;

        public GameOfLifeController(GameOfLifeService gameOfLifeService, GameOfLifeContext context)
        {
            _gameOfLifeService = gameOfLifeService;
            _context = context;
        }

        /// <summary>
        /// Creates a new Game of Life board with a specified initial state.
        /// </summary>
        /// <param name="boardStateDto">A DTO containing the initial cell states (0 for dead, 1 for alive).</param>
        /// <returns>The ID of the newly created board.</returns>
        /// <response code="201">Returns the newly created board's ID.</response>
        /// <response code="400">If the board data is null or empty.</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> CreateBoard([FromBody] BoardStateDto boardStateDto)
        {
            if (boardStateDto?.Cells == null || boardStateDto.Cells.Length == 0)
            {
                return BadRequest("Board data cannot be empty.");
            }

            var height = boardStateDto.Cells.Length;
            var width = boardStateDto.Cells[0].Length;
            var newBoard = new Board(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isAlive = boardStateDto.Cells[y].Length > x && boardStateDto.Cells[y][x] == 1;
                    newBoard.SetCellState(x, y, isAlive);
                }
            }

            _context.Boards.Add(newBoard);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBoard), new { id = newBoard.Id }, new { boardId = newBoard.Id });
        }

        /// <summary>
        /// Retrieves the current state of a specific board.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <returns>The current cell state of the board.</returns>
        /// <response code="200">Returns the board's state.</response>
        /// <response code="404">If a board with the specified ID is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BoardStateDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBoard(Guid id)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null) return NotFound();
            return Ok(board.ToDto());
        }

        /// <summary>
        /// Calculates the next generation for a board and persists the new state.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <returns>The board state after one generation.</returns>
        /// <response code="200">Returns the next generation's state.</response>
        /// <response code="404">If a board with the specified ID is not found.</response>
        [HttpGet("{id}/next")]
        [ProducesResponseType(typeof(BoardStateDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetNextGeneration(Guid id)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null) return NotFound($"Board with ID {id} not found.");

            var nextBoardState = _gameOfLifeService.CalculateNextGeneration(board);
            board.CellData = nextBoardState.CellData;
            await _context.SaveChangesAsync();
            return Ok(board.ToDto());
        }

        /// <summary>
        /// Advances a board by a specified number of generations and persists the final state.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <param name="generations">The number of generations to advance.</param>
        /// <returns>The board state after the specified number of generations.</returns>
        /// <response code="200">Returns the final state.</response>
        /// <response code="400">If the generation count is not a positive number.</response>
        /// <response code="404">If a board with the specified ID is not found.</response>
        [HttpGet("{id}/states/{generations}")]
        [ProducesResponseType(typeof(BoardStateDto), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetGenerationsAway(Guid id, int generations)
        {
            if (generations <= 0) return BadRequest("Number of generations must be a positive integer.");
            var board = await _context.Boards.FindAsync(id);
            if (board == null) return NotFound($"Board with ID {id} not found.");

            var currentGeneration = board;
            for (int i = 0; i < generations; i++)
            {
                currentGeneration = _gameOfLifeService.CalculateNextGeneration(currentGeneration);
            }

            board.CellData = currentGeneration.CellData;
            await _context.SaveChangesAsync();
            return Ok(board.ToDto());
        }

        /// <summary>
        /// Finds the final, stable state of a board and persists it.
        /// </summary>
        /// <remarks>
        /// This will simulate generations until the board becomes stable (stops changing) or a loop is detected.
        /// It will time out if a conclusion is not reached within the configured limit.
        /// </remarks>
        /// <param name="id">The unique identifier of the board.</param>
        /// <returns>The final, stable state of the board.</returns>
        /// <response code="200">Returns the final board state.</response>
        /// <response code="404">If a board with the specified ID is not found.</response>
        /// <response code="409">If the board does not stabilize within the configured generation limit.</response>
        [HttpGet("{id}/final")]
        [ProducesResponseType(typeof(BoardStateDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(object), 409)]
        public async Task<IActionResult> GetFinalState(Guid id)
        {
            var board = await _context.Boards.FindAsync(id);
            if (board == null)
            {
                return NotFound($"Board with ID {id} not found.");
            }

            try
            {
                var finalState = _gameOfLifeService.FindFinalState(board);
                board.CellData = finalState.CellData;
                await _context.SaveChangesAsync();
                return Ok(board.ToDto());
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
    public static class BoardExtensions
    {
        public static BoardStateDto ToDto(this Board board)
        {
            var boardDto = new BoardStateDto
            {
                Cells = new int[board.Height][]
            };
            for (int y = 0; y < board.Height; y++)
            {
                boardDto.Cells[y] = new int[board.Width];
                for (int x = 0; x < board.Width; x++)
                {
                    boardDto.Cells[y][x] = board.GetCellState(x, y) ? 1 : 0;
                }
            }
            return boardDto;
        }
    }
}

