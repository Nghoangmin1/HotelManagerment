using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using HotelManagement.Models;
using HotelManagement.Areas.Admin.Repositories;

namespace HotelManagement.Areas.Admin.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<AppRole>> GetAllRolesAsync();
        Task<AppRole?> GetRoleByNameAsync(string name);
        Task<IdentityResult> CreateRoleAsync(AppRole role);
        Task<bool> DeleteRoleAsync(string id);
    }

    public class RoleService : IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IRoleRepository _roleRepository;

        public class AppRoleDetail
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
        }

        public RoleService(RoleManager<AppRole> roleManager, IRoleRepository roleRepository)
        {
            _roleManager = roleManager;
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<AppRole>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task<AppRole?> GetRoleByNameAsync(string name)
        {
            return await _roleRepository.GetByNameAsync(name);
        }

        public async Task<IdentityResult> CreateRoleAsync(AppRole role)
        {
            return await _roleManager.CreateAsync(role);
        }

        public async Task<bool> DeleteRoleAsync(string id)
        {
            return await _roleRepository.DeleteAsync(id);
        }
    }
}
