using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Receptionist.Repositories
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetActiveServicesAsync();
        Task<Service?> GetByIdAsync(int id);
        Task<bool> AddServiceUsageAsync(ServiceUsage usage);
        Task<IEnumerable<ServiceUsage>> GetUsagesByBookingIdAsync(int bookingId);
        Task<IEnumerable<ServiceOrder>> GetOrdersByBookingIdAsync(int bookingId);
        Task<ServiceOrder?> GetOrderByIdAsync(int orderId);
        Task<bool> UpdateServiceOrderAsync(ServiceOrder order);
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

        public async Task<bool> AddServiceUsageAsync(ServiceUsage usage)
        {
            await _context.ServiceUsages.AddAsync(usage);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<ServiceUsage>> GetUsagesByBookingIdAsync(int bookingId)
        {
            return await _context.ServiceUsages
                .Include(su => su.Service)
                .Where(su => su.BookingId == bookingId)
                .OrderByDescending(su => su.UsageDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceOrder>> GetOrdersByBookingIdAsync(int bookingId)
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
                .Include(so => so.Booking)
                .FirstOrDefaultAsync(so => so.Id == orderId);
        }

        public async Task<bool> UpdateServiceOrderAsync(ServiceOrder order)
        {
            _context.ServiceOrders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
