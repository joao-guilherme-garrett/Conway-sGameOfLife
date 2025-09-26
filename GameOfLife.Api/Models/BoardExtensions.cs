using System;

namespace GameOfLife.Api.Models
{
    public static class BoardExtensions
    {
        public static BoardStateDto ToDto(this Board board)
        {
            var cells = new int[board.Height][];
            for (int y = 0; y < board.Height; y++)
            {
                cells[y] = new int[board.Width];
                for (int x = 0; x < board.Width; x++)
                {
                    cells[y][x] = board.GetCellState(x, y) ? 1 : 0;
                }
            }
            return new BoardStateDto { Cells = cells };
        }

        public static Board FromDto(BoardStateDto dto)
        {
            int height = dto.Cells.Length;
            int width = height > 0 ? dto.Cells[0].Length : 0;
            var board = new Board(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (dto.Cells[y][x] == 1)
                    {
                        board.SetCellState(x, y, true);
                    }
                }
            }
            return board;
        }
    }
}
