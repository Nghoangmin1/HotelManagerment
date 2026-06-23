using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public interface IRoomRepository
    {
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(int id);
        Task<Room?> GetByRoomNumberAsync(string roomNumber);
        Task<bool> AddAsync(Room room);
        Task<bool> UpdateAsync(Room room);
        Task<bool> DeleteAsync(int id);
    }

    public class RoomRepository : IRoomRepository
    {
        private readonly ApplicationDbContext _context;

        public RoomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.RoomStatus)
                .ToListAsync();
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.RoomStatus)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Room?> GetByRoomNumberAsync(string roomNumber)
        {
            return await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.RoomStatus)
                .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
        }

        public async Task<bool> AddAsync(Room room)
        {
            await _context.Rooms.AddAsync(room);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Room room)
        {
            // Detach entity cũ nếu đang được tracked để tránh lỗi tracking conflict
            var existingTracked = _context.ChangeTracker.Entries<Room>()
                .FirstOrDefault(e => e.Entity.Id == room.Id);
            if (existingTracked != null)
            {
                existingTracked.State = EntityState.Detached;
            }

            _context.Rooms.Update(room);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return false;
            _context.Rooms.Remove(room);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
