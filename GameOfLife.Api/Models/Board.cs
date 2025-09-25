using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace GameOfLife.Api.Models
{
    public class Board
    {
        [Key]
        public Guid Id { get; set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        [NotMapped]
        private bool[][] _cells;

        public string CellData
        {
            get => JsonSerializer.Serialize(_cells);
            set => _cells = JsonSerializer.Deserialize<bool[][]>(value);
        }

        private Board() { }

        public Board(int width, int height)
        {
            Id = Guid.NewGuid();
            Width = width;
            Height = height;
            _cells = new bool[height][];
            for (int y = 0; y < height; y++)
            {
                _cells[y] = new bool[width];
            }
        }

        public bool GetCellState(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return false;
            }
            return _cells[y][x];
        }

        public void SetCellState(int x, int y, bool isAlive)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                _cells[y][x] = isAlive;
            }
        }

        public bool IsEqual(Board other)
        {
            if (other == null || this.Width != other.Width || this.Height != other.Height)
            {
                return false;
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (this.GetCellState(x, y) != other.GetCellState(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

