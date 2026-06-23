using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Admin.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Admin.Services
{
    public interface IServiceService
    {
        Task<IEnumerable<Service>> GetAllServicesAsync();
        Task<Service?> GetServiceByIdAsync(int id);
        Task<bool> CreateServiceAsync(Service service, string user);
        Task<bool> UpdateServiceAsync(Service service, string user);
        Task<bool> DeleteServiceAsync(int id, string user);
    }

    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            return await _serviceRepository.GetAllAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _serviceRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateServiceAsync(Service service, string user)
        {
            if (service == null) return false;

            if (!ServiceValidationHelper.IsValidService(service.ServiceName, service.Price, out string err))
            {
                throw new ArgumentException(err);
            }

            service.ServiceName = service.ServiceName.Trim();
            service.Description = service.Description?.Trim() ?? string.Empty;
            service.CreatedAt = DateTime.UtcNow;

            var result = await _serviceRepository.AddAsync(service);
            if (result)
            {
                Logger.LogInfo($"[SERVICE CATALOG] {user} created service '{service.ServiceName}' with price {service.Price:N0}đ");
            }
            return result;
        }

        public async Task<bool> UpdateServiceAsync(Service service, string user)
        {
            if (service == null) return false;

            if (!ServiceValidationHelper.IsValidService(service.ServiceName, service.Price, out string err))
            {
                throw new ArgumentException(err);
            }

            var existing = await _serviceRepository.GetByIdAsync(service.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Dịch vụ không tồn tại!");
            }

            existing.ServiceName = service.ServiceName.Trim();
            existing.Price = service.Price;
            existing.Description = service.Description?.Trim() ?? string.Empty;
            existing.IsActive = service.IsActive;

            var result = await _serviceRepository.UpdateAsync(existing);
            if (result)
            {
                Logger.LogInfo($"[SERVICE CATALOG] {user} updated service '{existing.ServiceName}'. Price: {existing.Price:N0}đ, Active: {existing.IsActive}");
            }
            return result;
        }

        public async Task<bool> DeleteServiceAsync(int id, string user)
        {
            var existing = await _serviceRepository.GetByIdAsync(id);
            if (existing == null) return false;

            var result = await _serviceRepository.DeleteAsync(id);
            if (result)
            {
                Logger.LogInfo($"[SERVICE CATALOG] {user} deleted service '{existing.ServiceName}' (ID: {id})");
            }
            return result;
        }
    }
}
