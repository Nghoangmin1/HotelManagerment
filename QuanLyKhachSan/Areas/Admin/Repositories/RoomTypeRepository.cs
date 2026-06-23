using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public interface IRoomTypeRepository
    {
        Task<IEnumerable<RoomType>> GetAllAsync();
        Task<RoomType?> GetByIdAsync(int id);
        Task<bool> AddAsync(RoomType roomType);
        Task<bool> UpdateAsync(RoomType roomType);
        Task<bool> DeleteAsync(int id);
    }

    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RoomType>> GetAllAsync()
        {
            return await _context.RoomTypes.ToListAsync();
        }

        public async Task<RoomType?> GetByIdAsync(int id)
        {
            return await _context.RoomTypes.FindAsync(id);
        }

        public async Task<bool> AddAsync(RoomType roomType)
        {
            await _context.RoomTypes.AddAsync(roomType);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(RoomType roomType)
        {
            _context.RoomTypes.Update(roomType);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null) return false;
            _context.RoomTypes.Remove(roomType);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
