using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerEntity = HotelManagement.Areas.Admin.Models.Customer;
using HotelManagement.Areas.Admin.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Admin.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerEntity>> GetAllCustomersAsync();
        Task<CustomerEntity?> GetCustomerByIdAsync(int id);
        Task<CustomerEntity?> GetCustomerByIdentityCardAsync(string identityCard);
        Task<bool> CreateCustomerAsync(CustomerEntity customer, string user);
        Task<bool> UpdateCustomerAsync(CustomerEntity customer, string user);
        Task<bool> DeleteCustomerAsync(int id, string user);
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

        public async Task<CustomerEntity?> GetCustomerByIdentityCardAsync(string identityCard)
        {
            return await _customerRepository.GetByIdentityCardAsync(identityCard);
        }

        public async Task<bool> CreateCustomerAsync(CustomerEntity customer, string user)
        {
            if (customer == null) return false;

            if (!CustomerValidationHelper.IsValidFullName(customer.FullName))
            {
                throw new ArgumentException("Họ và tên không hợp lệ! Phải từ 2 đến 100 ký tự.");
            }

            if (!CustomerValidationHelper.IsValidIdentityCard(customer.IdentityCard))
            {
                throw new ArgumentException("Số CMND/CCCD/Hộ chiếu không hợp lệ!");
            }

            if (!CustomerValidationHelper.IsValidPhone(customer.PhoneNumber))
            {
                throw new ArgumentException("Số điện thoại không hợp lệ! Số điện thoại phải bắt đầu bằng 0 và có 10-11 chữ số.");
            }

            if (!CustomerValidationHelper.IsValidEmail(customer.Email))
            {
                throw new ArgumentException("Email không đúng định dạng!");
            }

            var existing = await _customerRepository.GetByIdentityCardAsync(customer.IdentityCard);
            if (existing != null)
            {
                throw new InvalidOperationException($"Số CMND/CCCD/Hộ chiếu {customer.IdentityCard} đã tồn tại trong hệ thống!");
            }

            customer.CreatedAt = DateTime.UtcNow;
            var result = await _customerRepository.AddAsync(customer);
            if (result)
            {
                Logger.LogInfo($"[CUSTOMER] Customer '{customer.FullName}' (ID Card: {customer.IdentityCard}) was created by User: {user}");
            }
            return result;
        }

        public async Task<bool> UpdateCustomerAsync(CustomerEntity customer, string user)
        {
            if (customer == null) return false;

            if (!CustomerValidationHelper.IsValidFullName(customer.FullName))
            {
                throw new ArgumentException("Họ và tên không hợp lệ!");
            }

            if (!CustomerValidationHelper.IsValidIdentityCard(customer.IdentityCard))
            {
                throw new ArgumentException("Số CMND/CCCD/Hộ chiếu không hợp lệ!");
            }

            if (!CustomerValidationHelper.IsValidPhone(customer.PhoneNumber))
            {
                throw new ArgumentException("Số điện thoại không hợp lệ!");
            }

            if (!CustomerValidationHelper.IsValidEmail(customer.Email))
            {
                throw new ArgumentException("Email không hợp lệ!");
            }

            var existing = await _customerRepository.GetByIdentityCardAsync(customer.IdentityCard);
            if (existing != null && existing.Id != customer.Id)
            {
                throw new InvalidOperationException($"Số CMND/CCCD/Hộ chiếu {customer.IdentityCard} đang được sử dụng bởi khách hàng khác!");
            }

            var result = await _customerRepository.UpdateAsync(customer);
            if (result)
            {
                Logger.LogInfo($"[CUSTOMER] Customer '{customer.FullName}' (ID Card: {customer.IdentityCard}) was updated by User: {user}");
            }
            return result;
        }

        public async Task<bool> DeleteCustomerAsync(int id, string user)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null) return false;

            var result = await _customerRepository.DeleteAsync(id);
            if (result)
            {
                Logger.LogInfo($"[CUSTOMER] Customer '{customer.FullName}' (ID Card: {customer.IdentityCard}) was deleted by User: {user}");
            }
            return result;
        }
    }
}
