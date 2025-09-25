using GameOfLife.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api.Data
{
    public class GameOfLifeContext : DbContext
    {
        public GameOfLifeContext(DbContextOptions<GameOfLifeContext> options)
            : base(options)
        {
        }
        public DbSet<Board> Boards { get; set; }
    }
}
