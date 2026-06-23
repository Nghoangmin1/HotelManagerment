using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Receptionist.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Receptionist.Services
{
    public interface IServiceService
    {
        Task<IEnumerable<Service>> GetActiveServicesAsync();
        Task<Service?> GetServiceByIdAsync(int id);
        Task<bool> AssignServiceToBookingAsync(int bookingId, int serviceId, int quantity, string notes, string username);
        Task<IEnumerable<ServiceUsage>> GetBookingUsagesAsync(int bookingId);
        Task<IEnumerable<ServiceOrder>> GetBookingOrdersAsync(int bookingId);
        Task<bool> ProcessServiceOrderAsync(int orderId, string status, string username);
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

        public async Task<bool> AssignServiceToBookingAsync(int bookingId, int serviceId, int quantity, string notes, string username)
        {
            if (!ServiceUsageValidationHelper.IsValidUsage(bookingId, serviceId, quantity, out string error))
            {
                throw new ArgumentException(error);
            }

            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null || !service.IsActive)
            {
                throw new InvalidOperationException("Dịch vụ không tồn tại hoặc đã ngừng hoạt động!");
            }

            var usage = new ServiceUsage
            {
                BookingId = bookingId,
                ServiceId = serviceId,
                Quantity = quantity,
                UsageDate = DateTime.UtcNow,
                Notes = notes?.Trim() ?? string.Empty
            };

            var result = await _serviceRepository.AddServiceUsageAsync(usage);
            if (result)
            {
                Logger.LogInfo($"[RECEPTIONIST SERVICE ASSIGN] Booking #{bookingId}: Assigned {quantity}x '{service.ServiceName}'. Notes: {usage.Notes}. User: {username}");
            }
            return result;
        }

        public async Task<IEnumerable<ServiceUsage>> GetBookingUsagesAsync(int bookingId)
        {
            return await _serviceRepository.GetUsagesByBookingIdAsync(bookingId);
        }

        public async Task<IEnumerable<ServiceOrder>> GetBookingOrdersAsync(int bookingId)
        {
            return await _serviceRepository.GetOrdersByBookingIdAsync(bookingId);
        }

        public async Task<bool> ProcessServiceOrderAsync(int orderId, string status, string username)
        {
            var order = await _serviceRepository.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Không tìm thấy đơn đặt dịch vụ!");
            }

            order.Status = status; // Approved, Completed, Cancelled
            var result = await _serviceRepository.UpdateServiceOrderAsync(order);

            if (result)
            {
                Logger.LogInfo($"[SERVICE ORDER PROCESS] Order #{orderId} updated to {status} by receptionist {username}.");

                // If approved or completed, create a service usage so that it reflects on the final bill
                if (status == "Approved" || status == "Completed")
                {
                    var usage = new ServiceUsage
                    {
                        BookingId = order.BookingId,
                        ServiceId = order.ServiceId,
                        Quantity = order.Quantity,
                        UsageDate = DateTime.UtcNow,
                        Notes = $"Được duyệt từ đơn hàng online #{orderId}. {order.Notes}"
                    };
                    await _serviceRepository.AddServiceUsageAsync(usage);
                }
            }

            return result;
        }
    }
}
