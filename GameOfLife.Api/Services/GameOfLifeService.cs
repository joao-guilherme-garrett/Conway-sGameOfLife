using GameOfLife.Api.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace GameOfLife.Api.Services
{
    public class GameOfLifeService
    {
        private readonly int _maxGenerations;
        public GameOfLifeService(IConfiguration configuration)
        {
            _maxGenerations = configuration.GetValue<int>("GameSettings:MaxGenerationsToFinalState", 1000);
        }

        public Board CalculateNextGeneration(Board currentBoard)
        {
            var nextBoard = new Board(currentBoard.Width, currentBoard.Height);

            for (int y = 0; y < currentBoard.Height; y++)
            {
                for (int x = 0; x < currentBoard.Width; x++)
                {
                    int liveNeighbors = CountLiveNeighbors(currentBoard, x, y);
                    bool isAlive = currentBoard.GetCellState(x, y);

                    if (isAlive && (liveNeighbors < 2 || liveNeighbors > 3))
                    {
                        // Dies
                    }
                    else if (!isAlive && liveNeighbors == 3)
                    {
                        // Born
                        nextBoard.SetCellState(x, y, true);
                    }
                    else if (isAlive)
                    {
                        // Survives
                        nextBoard.SetCellState(x, y, true);
                    }
                }
            }
            return nextBoard;
        }

        public Board FindFinalState(Board initialBoard)
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

        private int CountLiveNeighbors(Board board, int x, int y)
        {
            int count = 0;
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    if (i == 0 && j == 0) continue;

                    int neighborX = x + i;
                    int neighborY = y + j;

                    if (neighborX >= 0 && neighborX < board.Width &&
                        neighborY >= 0 && neighborY < board.Height &&
                        board.GetCellState(neighborX, neighborY))
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}

