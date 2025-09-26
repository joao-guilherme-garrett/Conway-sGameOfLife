using GameOfLife.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GameOfLife.Api.Data
{
    public class BoardRepository : IBoardRepository
    {
        private readonly GameOfLifeContext _context;

        public BoardRepository(GameOfLifeContext context)
        {
            _context = context;
        }

        public async Task<Board?> GetByIdAsync(Guid id)
        {
            return await _context.Boards.FindAsync(id);
        }

        public async Task AddAsync(Board board)
        {
            await _context.Boards.AddAsync(board);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Board board)
        {
            _context.Boards.Update(board);
            await _context.SaveChangesAsync();
        }
    }
}

