using GameOfLife.Api.Data;
using GameOfLife.Api.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameOfLife.Api.Services
{
    public class GameOfLifeService
    {
        private readonly int _maxGenerations;
        private readonly IBoardRepository _boardRepository;

        public GameOfLifeService(IConfiguration configuration, IBoardRepository boardRepository)
        {
            _maxGenerations = configuration.GetValue<int>("GameSettings:MaxGenerationsToFinalState", 1000);
            _boardRepository = boardRepository;
        }

        public async Task<Board?> GetBoardByIdAsync(Guid id)
        {
            return await _boardRepository.GetByIdAsync(id);
        }

        public async Task<Board> CreateBoardAsync(Board board)
        {
            await _boardRepository.AddAsync(board);
            return board;
        }

        public async Task<Board?> GetNextGenerationAsync(Guid id)
        {
            var board = await _boardRepository.GetByIdAsync(id);
            if (board == null) return null;

            var nextGenerationBoard = CalculateNextGeneration(board);

            board.CellData = nextGenerationBoard.CellData;
            await _boardRepository.UpdateAsync(board);

            return board;
        }

        public async Task<Board?> GetGenerationsAwayAsync(Guid id, int generations)
        {
            var board = await _boardRepository.GetByIdAsync(id);
            if (board == null) return null;

            var currentGeneration = board;
            for (int i = 0; i < generations; i++)
            {
                currentGeneration = CalculateNextGeneration(currentGeneration);
            }

            board.CellData = currentGeneration.CellData;
            await _boardRepository.UpdateAsync(board);

            return board;
        }

        public async Task<Board?> GetFinalStateAsync(Guid id)
        {
            var board = await _boardRepository.GetByIdAsync(id);
            if (board == null) return null;

            var finalState = FindFinalState(board);

            board.CellData = finalState.CellData;
            await _boardRepository.UpdateAsync(board);

            return board;
        }

        private Board CalculateNextGeneration(Board currentBoard)
        {
            var nextBoard = new Board(currentBoard.Width, currentBoard.Height);
            var neighborCounts = new Dictionary<(int x, int y), int>();
            var liveCells = new HashSet<(int x, int y)>();

            for (int y = 0; y < currentBoard.Height; y++)
            {
                for (int x = 0; x < currentBoard.Width; x++)
                {
                    if (currentBoard.GetCellState(x, y))
                    {
                        liveCells.Add((x, y));
                        for (int j = -1; j <= 1; j++)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                if (i == 0 && j == 0) continue;

                                var neighbor = (x: x + i, y: y + j);

                                if (neighbor.x >= 0 && neighbor.x < currentBoard.Width &&
                                    neighbor.y >= 0 && neighbor.y < currentBoard.Height)
                                {
                                    if (!neighborCounts.ContainsKey(neighbor))
                                    {
                                        neighborCounts[neighbor] = 0;
                                    }
                                    neighborCounts[neighbor]++;
                                }
                            }
                        }
                    }
                }
            }

            foreach (var cell in neighborCounts)
            {
                var position = cell.Key;
                var count = cell.Value;
                bool isAlive = liveCells.Contains(position);

                if (isAlive && (count == 2 || count == 3))
                {
                    // Survives
                    nextBoard.SetCellState(position.x, position.y, true);
                }
                else if (!isAlive && count == 3)
                {
                    // Born
                    nextBoard.SetCellState(position.x, position.y, true);
                }
            }

            return nextBoard;
        }

        private Board FindFinalState(Board initialBoard)
        {
            var currentGeneration = initialBoard;
            var history = new HashSet<string> { initialBoard.CellData };

            for (int i = 0; i < _maxGenerations; i++)
            {
                var nextGeneration = CalculateNextGeneration(currentGeneration);

                if (nextGeneration.CellData == currentGeneration.CellData)
                {
                    return nextGeneration;
                }

                if (history.Contains(nextGeneration.CellData))
                {
                    return nextGeneration;
                }

                history.Add(nextGeneration.CellData);
                currentGeneration = nextGeneration;
            }

            throw new InvalidOperationException($"Board did not stabilize after {_maxGenerations} generations.");
        }
    }
}

