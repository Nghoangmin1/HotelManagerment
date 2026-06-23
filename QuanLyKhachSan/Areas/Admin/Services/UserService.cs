using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using HotelManagement.Models;
using HotelManagement.Areas.Admin.Repositories;
using HotelManagement.Data;

namespace HotelManagement.Areas.Admin.Services
{
    public interface IUserService
    {
        Task<SignInResult> PasswordSignInAsync(string username, string password, bool isPersistent, bool lockoutOnFailure);
        Task SignOutAsync();
        Task<IdentityResult> CreateUserAsync(AppUser user, string password, string role);
        Task<IEnumerable<AppUser>> GetAllUsersAsync();
        Task<AppUser?> GetUserByIdAsync(string id);
        Task AddAuditLogAsync(string userId, string userName, string action, string ipAddress);
    }

    public class UserService : IUserService
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public UserService(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUserRepository userRepository,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<SignInResult> PasswordSignInAsync(string username, string password, bool isPersistent, bool lockoutOnFailure)
        {
            return await _signInManager.PasswordSignInAsync(username, password, isPersistent, lockoutOnFailure);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> CreateUserAsync(AppUser user, string password, string role)
        {
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                // Add to role
                await _userManager.AddToRoleAsync(user, role);
            }
            return result;
        }

        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<AppUser?> GetUserByIdAsync(string id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task AddAuditLogAsync(string userId, string userName, string action, string ipAddress)
        {
            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                Timestamp = DateTime.UtcNow,
                IpAddress = ipAddress
            };
            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
