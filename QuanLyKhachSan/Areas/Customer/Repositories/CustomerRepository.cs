using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;
using CustomerEntity = HotelManagement.Areas.Admin.Models.Customer;

namespace HotelManagement.Areas.Customer.Repositories
{
    public interface ICustomerRepository
    {
        Task<CustomerEntity?> GetByUserIdAsync(string userId);
        Task<CustomerEntity?> GetByIdAsync(int id);
        Task<bool> AddAsync(CustomerEntity customer);
        Task<bool> UpdateAsync(CustomerEntity customer);
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerEntity?> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;
            return await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<CustomerEntity?> GetByIdAsync(int id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AddAsync(CustomerEntity customer)
        {
            await _context.Customers.AddAsync(customer);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(CustomerEntity customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
