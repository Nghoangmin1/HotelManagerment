using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Customer.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Customer.Services
{
    public interface IServiceService
    {
        Task<IEnumerable<Service>> GetActiveServicesAsync();
        Task<Service?> GetServiceByIdAsync(int id);
        Task<bool> OrderServiceAsync(int bookingId, int serviceId, int quantity, string notes, string user);
        Task<IEnumerable<ServiceOrder>> GetCustomerBookingOrdersAsync(int bookingId);
    }

    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        public async Task<IEnumerable<Service>> GetActiveServicesAsync()
        {
            return await _serviceRepository.GetActiveServicesAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _serviceRepository.GetByIdAsync(id);
        }

        public async Task<bool> OrderServiceAsync(int bookingId, int serviceId, int quantity, string notes, string user)
        {
            if (!CustomerServiceValidationHelper.IsValidServiceOrder(quantity, notes, out string err))
            {
                throw new ArgumentException(err);
            }

            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null || !service.IsActive)
            {
                throw new InvalidOperationException("Dịch vụ không tồn tại hoặc đã ngưng phục vụ!");
            }

            var order = new ServiceOrder
            {
                BookingId = bookingId,
                ServiceId = serviceId,
                Quantity = quantity,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Notes = notes?.Trim() ?? string.Empty
            };

            var result = await _serviceRepository.AddOrderAsync(order);
            if (result)
            {
                Logger.LogInfo($"[CUSTOMER SERVICE ORDER] Booking #{bookingId}: Customer ordered {quantity}x '{service.ServiceName}'. Status: Pending. Notes: {order.Notes}. User: {user}");
            }
            return result;
        }

        public async Task<IEnumerable<ServiceOrder>> GetCustomerBookingOrdersAsync(int bookingId)
        {
            return await _serviceRepository.GetBookingOrdersAsync(bookingId);
        }
    }
}
