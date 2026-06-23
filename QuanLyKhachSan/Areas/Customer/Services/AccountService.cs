using System;
using System.Threading.Tasks;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Customer.Repositories;
using HotelManagement.Shared;
using CustomerEntity = HotelManagement.Areas.Admin.Models.Customer;

namespace HotelManagement.Areas.Customer.Services
{
    public interface IAccountService
    {
        Task<CustomerEntity?> GetProfileByUserIdAsync(string userId);
        Task<bool> UpdateProfileAsync(string userId, CustomerEntity profileUpdates);
        Task<bool> RegisterCustomerProfileAsync(string userId, CustomerEntity profile);
    }

    public class AccountService : IAccountService
    {
        private readonly ICustomerRepository _customerRepository;

        public AccountService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CustomerEntity?> GetProfileByUserIdAsync(string userId)
        {
            return await _customerRepository.GetByUserIdAsync(userId);
        }

        public async Task<bool> UpdateProfileAsync(string userId, CustomerEntity profileUpdates)
        {
            if (profileUpdates == null) return false;

            var existing = await _customerRepository.GetByUserIdAsync(userId);
            if (existing == null)
            {
                // If profile doesn't exist yet, create it
                return await RegisterCustomerProfileAsync(userId, profileUpdates);
            }

            // Standard profile validations
            if (!CustomerValidationHelper.IsValidFullName(profileUpdates.FullName))
                throw new ArgumentException("Họ và tên không hợp lệ!");
            if (!CustomerValidationHelper.IsValidPhone(profileUpdates.PhoneNumber))
                throw new ArgumentException("Số điện thoại không hợp lệ!");
            if (!CustomerValidationHelper.IsValidEmail(profileUpdates.Email))
                throw new ArgumentException("Email không hợp lệ!");
            if (!CustomerValidationHelper.IsValidIdentityCard(profileUpdates.IdentityCard))
                throw new ArgumentException("Số CMND/CCCD/Hộ chiếu không hợp lệ!");

            existing.FullName = profileUpdates.FullName;
            existing.PhoneNumber = profileUpdates.PhoneNumber;
            existing.Email = profileUpdates.Email;
            existing.IdentityCard = profileUpdates.IdentityCard;
            existing.Address = profileUpdates.Address;

            var result = await _customerRepository.UpdateAsync(existing);
            if (result)
            {
                Logger.LogInfo($"[CUSTOMER PROFILE] Customer '{existing.FullName}' updated profile details (User: {userId})");
            }
            return result;
        }

        public async Task<bool> RegisterCustomerProfileAsync(string userId, CustomerEntity profile)
        {
            if (profile == null) return false;

            if (!CustomerValidationHelper.IsValidFullName(profile.FullName))
                throw new ArgumentException("Họ và tên không hợp lệ!");
            if (!CustomerValidationHelper.IsValidPhone(profile.PhoneNumber))
                throw new ArgumentException("Số điện thoại không hợp lệ!");
            if (!CustomerValidationHelper.IsValidEmail(profile.Email))
                throw new ArgumentException("Email không hợp lệ!");
            if (!CustomerValidationHelper.IsValidIdentityCard(profile.IdentityCard))
                throw new ArgumentException("Số CMND/CCCD/Hộ chiếu không hợp lệ!");

            profile.UserId = userId;
            profile.CreatedAt = DateTime.UtcNow;

            var result = await _customerRepository.AddAsync(profile);
            if (result)
            {
                Logger.LogInfo($"[CUSTOMER PROFILE] Customer '{profile.FullName}' registered a new online profile (User: {userId})");
            }
            return result;
        }
    }
}
