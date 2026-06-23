using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(int id);
        Task<bool> AddAsync(Service service);
        Task<bool> UpdateAsync(Service service);
        Task<bool> DeleteAsync(int id);
        
        Task<IEnumerable<ServiceOrder>> GetAllOrdersAsync();
        Task<ServiceOrder?> GetOrderByIdAsync(int id);
        Task<bool> UpdateOrderAsync(ServiceOrder order);
        
        Task<IEnumerable<ServiceUsage>> GetAllUsagesAsync();
    }

    public class ServiceRepository : IServiceRepository
    {
        private readonly ApplicationDbContext _context;

        public ServiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _context.Services.ToListAsync();
        }

        public async Task<Service?> GetByIdAsync(int id)
        {
            return await _context.Services.FindAsync(id);
        }

        public async Task<bool> AddAsync(Service service)
        {
            await _context.Services.AddAsync(service);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(Service service)
        {
            _context.Services.Update(service);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return false;
            _context.Services.Remove(service);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<ServiceOrder>> GetAllOrdersAsync()
        {
            return await _context.ServiceOrders
                .Include(so => so.Service)
                .Include(so => so.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(so => so.Booking)
                    .ThenInclude(b => b!.Room)
                .ToListAsync();
        }

        public async Task<ServiceOrder?> GetOrderByIdAsync(int id)
        {
            return await _context.ServiceOrders
                .Include(so => so.Service)
                .Include(so => so.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(so => so.Booking)
                    .ThenInclude(b => b!.Room)
                .FirstOrDefaultAsync(so => so.Id == id);
        }

        public async Task<bool> UpdateOrderAsync(ServiceOrder order)
        {
            _context.ServiceOrders.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<ServiceUsage>> GetAllUsagesAsync()
        {
            return await _context.ServiceUsages
                .Include(su => su.Service)
                .Include(su => su.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(su => su.Booking)
                    .ThenInclude(b => b!.Room)
                .ToListAsync();
        }
    }
}
