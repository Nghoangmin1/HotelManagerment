using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;
using CustomerEntity = HotelManagement.Areas.Admin.Models.Customer;

namespace HotelManagement.Areas.Admin.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerEntity>> GetAllAsync();
        Task<CustomerEntity?> GetByIdAsync(int id);
        Task<CustomerEntity?> GetByIdentityCardAsync(string identityCard);
        Task<bool> AddAsync(CustomerEntity customer);
        Task<bool> UpdateAsync(CustomerEntity customer);
        Task<bool> DeleteAsync(int id);
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerEntity>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<CustomerEntity?> GetByIdAsync(int id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CustomerEntity?> GetByIdentityCardAsync(string identityCard)
        {
            if (string.IsNullOrWhiteSpace(identityCard)) return null;
            string trimmed = identityCard.Trim();
            return await _context.Customers.FirstOrDefaultAsync(c => c.IdentityCard == trimmed);
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

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;
            _context.Customers.Remove(customer);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
