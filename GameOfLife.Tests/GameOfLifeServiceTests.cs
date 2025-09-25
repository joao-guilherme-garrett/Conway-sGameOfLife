using GameOfLife.Api.Services;
using GameOfLife.Api.Models;

namespace GameOfLife.Tests
{
    public class GameOfLifeServiceTests
    {
        [Fact]
        public void CalculateNextGeneration_WithStableBlockPattern_ShouldNotChange()
        {
            var service = new GameOfLifeService();

            var initialBoard = new Board(4, 4);

            initialBoard.SetCellState(1, 1, true);
            initialBoard.SetCellState(1, 2, true);
            initialBoard.SetCellState(2, 1, true);
            initialBoard.SetCellState(2, 2, true);

            var expectedBoard = new Board(4, 4);
            expectedBoard.SetCellState(1, 1, true);
            expectedBoard.SetCellState(1, 2, true);
            expectedBoard.SetCellState(2, 1, true);
            expectedBoard.SetCellState(2, 2, true);


            var nextBoard = service.CalculateNextGeneration(initialBoard);


            Assert.True(expectedBoard.IsEqual(nextBoard));
        }

        [Fact]
        public void CalculateNextGeneration_WithBlinkerPattern_ShouldOscillate()
        {
            var service = new GameOfLifeService();
            var initialBoard = new Board(5, 5);

            initialBoard.SetCellState(2, 1, true);
            initialBoard.SetCellState(2, 2, true);
            initialBoard.SetCellState(2, 3, true);

            var expectedBoard = new Board(5, 5);
            expectedBoard.SetCellState(1, 2, true);
            expectedBoard.SetCellState(2, 2, true);
            expectedBoard.SetCellState(3, 2, true);

            var nextBoard = service.CalculateNextGeneration(initialBoard);

            Assert.True(expectedBoard.IsEqual(nextBoard));
        }
    }
}

