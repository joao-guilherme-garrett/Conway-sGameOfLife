using GameOfLife.Api.Data;
using GameOfLife.Api.Models;
using GameOfLife.Api.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameOfLife.Tests
{
    public class GameOfLifeServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IBoardRepository> _mockBoardRepository;

        public GameOfLifeServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockBoardRepository = new Mock<IBoardRepository>();
        }

        private GameOfLifeService CreateServiceWithMaxGenerations(string maxGenerations)
        {
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns(maxGenerations);
            _mockConfiguration.Setup(x => x.GetSection("GameSettings:MaxGenerationsToFinalState")).Returns(mockConfigSection.Object);

            return new GameOfLifeService(_mockConfiguration.Object, _mockBoardRepository.Object);
        }

        [Fact]
        public async Task GetNextGenerationAsync_WithStableBlockPattern_ShouldNotChange()
        {
            var service = CreateServiceWithMaxGenerations("1000");
            var boardId = Guid.NewGuid();
            var initialBoard = new Board(4, 4);
            initialBoard.SetCellState(1, 1, true);
            initialBoard.SetCellState(1, 2, true);
            initialBoard.SetCellState(2, 1, true);
            initialBoard.SetCellState(2, 2, true);

            _mockBoardRepository.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync(initialBoard);

            var nextGenerationBoard = await service.GetNextGenerationAsync(boardId);

            Assert.NotNull(nextGenerationBoard);
            Assert.Equal(initialBoard.CellData, nextGenerationBoard.CellData);
        }

        [Fact]
        public async Task GetNextGenerationAsync_WithOscillatingBlinkerPattern_ShouldFlip()
        {
            var service = CreateServiceWithMaxGenerations("1000");
            var boardId = Guid.NewGuid();
            var initialBoard = new Board(5, 5);
            initialBoard.SetCellState(2, 1, true);
            initialBoard.SetCellState(2, 2, true);
            initialBoard.SetCellState(2, 3, true);

            var expectedBoard = new Board(5, 5);
            expectedBoard.SetCellState(1, 2, true);
            expectedBoard.SetCellState(2, 2, true);
            expectedBoard.SetCellState(3, 2, true);

            _mockBoardRepository.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync(initialBoard);

            var nextGenerationBoard = await service.GetNextGenerationAsync(boardId);

            Assert.NotNull(nextGenerationBoard);
            Assert.Equal(expectedBoard.CellData, nextGenerationBoard.CellData);
        }

        [Fact]
        public async Task GetNextGenerationAsync_WithSingleCell_ShouldDieFromUnderpopulation()
        {
            var service = CreateServiceWithMaxGenerations("1000");
            var boardId = Guid.NewGuid();
            var initialBoard = new Board(3, 3);
            initialBoard.SetCellState(1, 1, true);

            var expectedBoard = new Board(3, 3);

            _mockBoardRepository.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync(initialBoard);

            var nextGenerationBoard = await service.GetNextGenerationAsync(boardId);

            Assert.NotNull(nextGenerationBoard);
            Assert.Equal(expectedBoard.CellData, nextGenerationBoard.CellData);
        }

        [Fact]
        public async Task GetNextGenerationAsync_WithCellAndFourNeighbors_ShouldDieFromOverpopulation()
        {
            var service = CreateServiceWithMaxGenerations("1000");
            var boardId = Guid.NewGuid();
            var initialBoard = new Board(3, 3);
            initialBoard.SetCellState(1, 1, true);
            initialBoard.SetCellState(0, 1, true);
            initialBoard.SetCellState(2, 1, true);
            initialBoard.SetCellState(1, 0, true);
            initialBoard.SetCellState(1, 2, true);

            var expectedBoard = new Board(3, 3);
            expectedBoard.SetCellState(0, 0, true);
            expectedBoard.SetCellState(1, 0, true);
            expectedBoard.SetCellState(2, 0, true);
            expectedBoard.SetCellState(0, 1, true);
            // Cell (1,1) is dead
            expectedBoard.SetCellState(2, 1, true);
            expectedBoard.SetCellState(0, 2, true);
            expectedBoard.SetCellState(1, 2, true);
            expectedBoard.SetCellState(2, 2, true);


            _mockBoardRepository.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync(initialBoard);

            var nextGenerationBoard = await service.GetNextGenerationAsync(boardId);

            Assert.NotNull(nextGenerationBoard);
            Assert.Equal(expectedBoard.CellData, nextGenerationBoard.CellData);
        }

        [Fact]
        public async Task GetFinalStateAsync_WhenBoardDoesNotStabilize_ShouldThrowException()
        {
            var service = CreateServiceWithMaxGenerations("10");
            var boardId = Guid.NewGuid();
            var initialBoard = new Board(20, 20);
            initialBoard.SetCellState(1, 0, true);
            initialBoard.SetCellState(2, 1, true);
            initialBoard.SetCellState(0, 2, true);
            initialBoard.SetCellState(1, 2, true);
            initialBoard.SetCellState(2, 2, true);

            _mockBoardRepository.Setup(repo => repo.GetByIdAsync(boardId)).ReturnsAsync(initialBoard);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetFinalStateAsync(boardId));
        }
    }
}

