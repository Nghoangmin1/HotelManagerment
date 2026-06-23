using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Customer.Repositories
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId);
        Task<Booking?> GetByIdAndCustomerIdAsync(int id, int customerId);
        Task<bool> AddAsync(Booking booking);
    }

    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.CheckInDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAndCustomerIdAsync(int id, int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == customerId);
        }

        public async Task<bool> AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
