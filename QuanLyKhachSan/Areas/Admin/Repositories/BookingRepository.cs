using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(int id);
        Task<bool> AddAsync(Booking booking);
        Task<bool> UpdateAsync(Booking booking);
        Task<bool> DeleteAsync(int id);
        
        Task<BookingDetail?> GetDetailByIdAsync(int id);
        Task<bool> AddDetailAsync(BookingDetail detail);
        Task<bool> DeleteDetailAsync(int id);
    }

    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomStatus)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomStatus)
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Booking booking)
        {
            _context.Entry(booking).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;
            _context.Bookings.Remove(booking);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<BookingDetail?> GetDetailByIdAsync(int id)
        {
            return await _context.BookingDetails
                .Include(d => d.Booking)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> AddDetailAsync(BookingDetail detail)
        {
            await _context.BookingDetails.AddAsync(detail);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteDetailAsync(int id)
        {
            var detail = await _context.BookingDetails.FindAsync(id);
            if (detail == null) return false;
            _context.BookingDetails.Remove(detail);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
