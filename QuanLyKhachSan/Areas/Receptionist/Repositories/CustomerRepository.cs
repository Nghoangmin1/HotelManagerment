using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using CustomerEntity = HotelManagement.Areas.Admin.Models.Customer;

namespace HotelManagement.Areas.Receptionist.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerEntity>> GetAllAsync();
        Task<CustomerEntity?> GetByIdAsync(int id);
        Task<CustomerEntity?> GetByIdentityCardAsync(string identityCard);
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
            return await _context.Customers.FirstOrDefaultAsync(c => c.IdentityCard == identityCard.Trim());
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
