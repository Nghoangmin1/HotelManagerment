using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Customer.Repositories
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetActiveServicesAsync();
        Task<Service?> GetByIdAsync(int id);
        Task<bool> AddOrderAsync(ServiceOrder order);
        Task<IEnumerable<ServiceOrder>> GetBookingOrdersAsync(int bookingId);
        Task<ServiceOrder?> GetOrderByIdAsync(int orderId);
    }

    public class ServiceRepository : IServiceRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetActiveServicesAsync()
        {
            return await _context.Services.Where(s => s.IsActive).ToListAsync();
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _context.Services.FindAsync(id);
        }

        public async Task<bool> AddOrderAsync(ServiceOrder order)
        {
            await _context.ServiceOrders.AddAsync(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<ServiceOrder>> GetBookingOrdersAsync(int bookingId)
        {
            return await _context.ServiceOrders
                .Include(so => so.Service)
                .Where(so => so.BookingId == bookingId)
                .OrderByDescending(so => so.OrderDate)
                .ToListAsync();
        }

        public async Task<ServiceOrder?> GetOrderByIdAsync(int orderId)
        {
            return await _context.ServiceOrders
                .Include(so => so.Service)
                .FirstOrDefaultAsync(so => so.Id == orderId);
        }
    }
}
