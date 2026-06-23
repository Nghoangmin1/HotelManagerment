using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Customer.Repositories;
using HotelManagement.Models;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Customer.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<object>> SearchAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut);
        Task<Booking?> CreateOnlineBookingAsync(int customerId, int roomTypeId, DateTime checkIn, DateTime checkOut);
        Task<IEnumerable<Booking>> GetCustomerBookingHistoryAsync(int customerId);
        Task<Booking?> GetCustomerBookingDetailsAsync(int bookingId, int customerId);
    }

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ApplicationDbContext _context;

        public BookingService(
            IBookingRepository bookingRepository,
            ICustomerRepository customerRepository,
            ApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _customerRepository = customerRepository;
            _context = context;
        }

        public async Task<IEnumerable<object>> SearchAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut)
        {
            if (!CustomerBookingValidationHelper.IsValidBookingDates(checkIn, checkOut, out string error))
            {
                throw new ArgumentException(error);
            }

            // Find all room IDs that have overlapping bookings
            var overlappingRoomIds = await _context.Bookings
                .Where(b => b.Status != "Cancelled" && b.Status != "CheckedOut")
                .Where(b => b.CheckInDate < checkOut && (b.CheckOutDate == null || b.CheckOutDate > checkIn))
                .Select(b => b.RoomId)
                .Distinct()
                .ToListAsync();

            // Group available rooms by RoomType
            var rooms = await _context.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.RoomStatus)
                .Where(r => r.RoomStatus!.StatusCode != "dirty") // Exclude dirty rooms from immediate search
                .ToListAsync();

            var availableRoomsGrouped = rooms
                .Where(r => !overlappingRoomIds.Contains(r.Id))
                .GroupBy(r => r.RoomType)
                .Select(g => new
                {
                    RoomType = g.Key,
                    AvailableCount = g.Count(),
                    Price = g.First().Price
                })
                .ToList();

            // Include all room types, even if 0 rooms available
            var allRoomTypes = await _context.RoomTypes.ToListAsync();
            var results = new List<object>();

            foreach (var type in allRoomTypes)
            {
                var match = availableRoomsGrouped.FirstOrDefault(g => g.RoomType?.Id == type.Id);
                results.Add(new
                {
                    Id = type.Id,
                    TypeName = type.TypeName,
                    BasePrice = type.BasePrice,
                    Description = type.Description,
                    AvailableCount = match?.AvailableCount ?? 0
                });
            }

            return results;
        }

        public async Task<Booking?> CreateOnlineBookingAsync(int customerId, int roomTypeId, DateTime checkIn, DateTime checkOut)
        {
            if (!CustomerBookingValidationHelper.IsValidBookingDates(checkIn, checkOut, out string error))
            {
                throw new ArgumentException(error);
            }

            // Find overlapping booking room IDs
            var overlappingRoomIds = await _context.Bookings
                .Where(b => b.Status != "Cancelled" && b.Status != "CheckedOut")
                .Where(b => b.CheckInDate < checkOut && (b.CheckOutDate == null || b.CheckOutDate > checkIn))
                .Select(b => b.RoomId)
                .Distinct()
                .ToListAsync();

            // Find first available room of the requested type
            var room = await _context.Rooms
                .Include(r => r.RoomStatus)
                .Where(r => r.RoomTypeId == roomTypeId && r.RoomStatus!.StatusCode == "available")
                .Where(r => !overlappingRoomIds.Contains(r.Id))
                .FirstOrDefaultAsync();

            if (room == null)
            {
                throw new InvalidOperationException("Xin lỗi, hiện tại đã hết phòng trống thuộc loại này trong khoảng thời gian đã chọn.");
            }

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
            {
                throw new InvalidOperationException("Không tìm thấy hồ sơ khách hàng.");
            }

            int days = (checkOut.Date - checkIn.Date).Days;
            if (days < 1) days = 1;

            var booking = new Booking
            {
                CustomerId = customerId,
                RoomId = room.Id,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                TotalPrice = room.Price * days,
                Status = "Pending"
            };

            var result = await _bookingRepository.AddAsync(booking);
            if (result)
            {
                // Sync room status to Reserved
                var reservedStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == "reserved");
                if (reservedStatus != null)
                {
                    room.RoomStatusId = reservedStatus.Id;
                    room.CustomerName = customer.FullName;
                    room.CustomerPhone = customer.PhoneNumber;
                    _context.Rooms.Update(room);
                    await _context.SaveChangesAsync();
                }

                BookingLogger.LogBookingCreated(booking.Id, room.RoomNumber, customer.FullName, "Online System");
            }

            return booking;
        }

        public async Task<IEnumerable<Booking>> GetCustomerBookingHistoryAsync(int customerId)
        {
            return await _bookingRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<Booking?> GetCustomerBookingDetailsAsync(int bookingId, int customerId)
        {
            return await _bookingRepository.GetByIdAndCustomerIdAsync(bookingId, customerId);
        }
    }
}
