using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerEntity = HotelManagement.Areas.Admin.Models.Customer;
using HotelManagement.Areas.Receptionist.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Receptionist.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerEntity>> GetAllCustomersAsync();
        Task<CustomerEntity?> GetCustomerByIdAsync(int id);
        Task<bool> CreateCustomerAsync(CustomerEntity customer, string user);
        Task<bool> UpdateCustomerAsync(CustomerEntity customer, string user);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<CustomerEntity>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task<CustomerEntity?> GetCustomerByIdAsync(int id)
        {
            return await _customerRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateCustomerAsync(CustomerEntity customer, string user)
        {
            if (customer == null) return false;

            if (!CustomerValidationHelper.IsValidFullName(customer.FullName))
                throw new ArgumentException("Họ và tên không hợp lệ!");
            if (!CustomerValidationHelper.IsValidPhone(customer.PhoneNumber))
                throw new ArgumentException("Số điện thoại không hợp lệ!");
            if (!CustomerValidationHelper.IsValidEmail(customer.Email))
                throw new ArgumentException("Email không hợp lệ!");
            if (!CustomerValidationHelper.IsValidIdentityCard(customer.IdentityCard))
                throw new ArgumentException("Số CMND/CCCD/Hộ chiếu không hợp lệ!");

            var existing = await _customerRepository.GetByIdentityCardAsync(customer.IdentityCard);
            if (existing != null)
            {
                throw new InvalidOperationException($"Số CMND/CCCD {customer.IdentityCard} đã tồn tại!");
            }

            customer.CreatedAt = DateTime.UtcNow;
            var result = await _customerRepository.AddAsync(customer);
            if (result)
            {
                ReceptionistLogger.LogCustomerUpdated(user, customer.FullName, "Created customer profile");
            }
            return result;
        }

        public async Task<bool> UpdateCustomerAsync(CustomerEntity customer, string user)
        {
            if (customer == null) return false;

            if (!CustomerValidationHelper.IsValidFullName(customer.FullName))
                throw new ArgumentException("Họ và tên không hợp lệ!");
            if (!CustomerValidationHelper.IsValidPhone(customer.PhoneNumber))
                throw new ArgumentException("Số điện thoại không hợp lệ!");
            if (!CustomerValidationHelper.IsValidEmail(customer.Email))
                throw new ArgumentException("Email không hợp lệ!");
            if (!CustomerValidationHelper.IsValidIdentityCard(customer.IdentityCard))
                throw new ArgumentException("Số CMND/CCCD/Hộ chiếu không hợp lệ!");

            var existing = await _customerRepository.GetByIdentityCardAsync(customer.IdentityCard);
            if (existing != null && existing.Id != customer.Id)
            {
                throw new InvalidOperationException($"Số CMND/CCCD {customer.IdentityCard} đang được dùng bởi khách hàng khác!");
            }

            var result = await _customerRepository.UpdateAsync(customer);
            if (result)
            {
                ReceptionistLogger.LogCustomerUpdated(user, customer.FullName, "Updated customer details");
            }
            return result;
        }
    }
}
