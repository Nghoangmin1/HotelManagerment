using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public interface IRoleRepository
    {
        Task<IEnumerable<AppRole>> GetAllAsync();
        Task<AppRole?> GetByIdAsync(string id);
        Task<AppRole?> GetByNameAsync(string name);
        Task<bool> CreateAsync(AppRole role);
        Task<bool> UpdateAsync(AppRole role);
        Task<bool> DeleteAsync(string id);
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public class AppRoleInfo
        {
            public string? RoleId { get; set; }
            public string? RoleName { get; set; }
        }

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppRole>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<AppRole?> GetByIdAsync(string id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<AppRole?> GetByNameAsync(string name)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<bool> CreateAsync(AppRole role)
        {
            await _context.Roles.AddAsync(role);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(AppRole role)
        {
            _context.Roles.Update(role);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var role = await GetByIdAsync(id);
            if (role == null) return false;
            _context.Roles.Remove(role);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
