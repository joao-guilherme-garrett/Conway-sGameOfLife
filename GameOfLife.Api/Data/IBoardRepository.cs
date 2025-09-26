using GameOfLife.Api.Models;
using System;
using System.Threading.Tasks;

namespace GameOfLife.Api.Data
{
    public interface IBoardRepository
    {
        Task<Board?> GetByIdAsync(Guid id);
        Task AddAsync(Board board);
        Task UpdateAsync(Board board);
    }
}

