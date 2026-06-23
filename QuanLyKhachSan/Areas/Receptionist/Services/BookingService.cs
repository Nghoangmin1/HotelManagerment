using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Receptionist.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Receptionist.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<bool> CreateBookingAsync(Booking booking, string user);
        Task<bool> UpdateBookingAsync(Booking booking, string user);
        
        Task<bool> CheckInAsync(int bookingId, string user);
        Task<bool> CheckOutAsync(int bookingId, string user);
        Task<bool> AddServiceDetailAsync(int bookingId, BookingDetail detail, string user);
        Task<bool> RemoveServiceDetailAsync(int detailId, string user);
    }

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ApplicationDbContext _context;

        public BookingService(
            IBookingRepository bookingRepository,
            IRoomRepository roomRepository,
            ICustomerRepository customerRepository,
            ApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _customerRepository = customerRepository;
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _bookingRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateBookingAsync(Booking booking, string user)
        {
            if (booking == null) return false;

            if (!BookingValidationHelper.IsValidBookingDates(booking.CheckInDate, booking.CheckOutDate))
            {
                throw new ArgumentException("Ngày trả phòng phải sau ngày nhận phòng!");
            }

            var room = await _roomRepository.GetByIdAsync(booking.RoomId);
            if (room == null)
            {
                throw new ArgumentException("Phòng đã chọn không tồn tại!");
            }

            if (booking.Status == "CheckedIn" && room.RoomStatus?.StatusCode == "occupied")
            {
                throw new InvalidOperationException("Phòng này hiện đang có khách!");
            }

            int days = 1;
            if (booking.CheckOutDate.HasValue)
            {
                days = (booking.CheckOutDate.Value.Date - booking.CheckInDate.Date).Days;
                if (days < 1) days = 1;
            }
            booking.TotalPrice = room.Price * days;

            var result = await _bookingRepository.AddAsync(booking);
            if (result)
            {
                BookingLogger.LogBookingCreated(booking.Id, room.RoomNumber, booking.Customer?.FullName ?? $"Cust #{booking.CustomerId}", user);
                await SyncRoomStatusAsync(booking);
            }
            return result;
        }

        public async Task<bool> UpdateBookingAsync(Booking booking, string user)
        {
            if (booking == null) return false;

            if (!BookingValidationHelper.IsValidBookingDates(booking.CheckInDate, booking.CheckOutDate))
            {
                throw new ArgumentException("Ngày trả phòng không hợp lệ!");
            }

            var result = await _bookingRepository.UpdateAsync(booking);
            if (result)
            {
                BookingLogger.LogBookingUpdated(booking.Id, user, $"Status: {booking.Status}");
                await SyncRoomStatusAsync(booking);
                await RecalculateTotalPriceAsync(booking.Id);
            }
            return result;
        }

        public async Task<bool> CheckInAsync(int bookingId, string user)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return false;

            if (booking.Status == "CheckedIn")
            {
                throw new InvalidOperationException("Đặt phòng này đã được check-in trước đó!");
            }

            booking.Status = "CheckedIn";
            booking.CheckInDate = DateTime.Now;

            var result = await _bookingRepository.UpdateAsync(booking);
            if (result)
            {
                await SyncRoomStatusAsync(booking);
                ReceptionistLogger.LogCheckIn(user, booking.Room?.RoomNumber ?? "N/A", booking.Customer?.FullName ?? "N/A");
            }
            return result;
        }

        public async Task<bool> CheckOutAsync(int bookingId, string user)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null) return false;

            if (booking.Status != "CheckedIn")
            {
                throw new InvalidOperationException("Đặt phòng này không ở trạng thái CheckIn để thực hiện CheckOut!");
            }

            booking.Status = "CheckedOut";
            booking.CheckOutDate = DateTime.Now;

            var result = await _bookingRepository.UpdateAsync(booking);
            if (result)
            {
                await SyncRoomStatusAsync(booking);
                await RecalculateTotalPriceAsync(bookingId);
                
                // Refresh booking to fetch updated total price
                var updatedBooking = await _bookingRepository.GetByIdAsync(bookingId);
                ReceptionistLogger.LogCheckOut(
                    user, 
                    updatedBooking?.Room?.RoomNumber ?? "N/A", 
                    updatedBooking?.Customer?.FullName ?? "N/A", 
                    updatedBooking?.TotalPrice ?? 0);
            }
            return result;
        }

        public async Task<bool> AddServiceDetailAsync(int bookingId, BookingDetail detail, string user)
        {
            if (detail == null) return false;
            detail.BookingId = bookingId;

            if (detail.Quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0!");
            if (detail.ServicePrice < 0)
                throw new ArgumentException("Giá phải lớn hơn hoặc bằng 0!");

            var result = await _bookingRepository.AddDetailAsync(detail);
            if (result)
            {
                BookingLogger.LogServiceAdded(bookingId, detail.ServiceName, detail.ServicePrice, detail.Quantity, user);
                await RecalculateTotalPriceAsync(bookingId);
            }
            return result;
        }

        public async Task<bool> RemoveServiceDetailAsync(int detailId, string user)
        {
            var detail = await _bookingRepository.GetDetailByIdAsync(detailId);
            if (detail == null) return false;

            int bookingId = detail.BookingId;
            string serviceName = detail.ServiceName;

            var result = await _bookingRepository.DeleteDetailAsync(detailId);
            if (result)
            {
                BookingLogger.LogServiceRemoved(bookingId, serviceName, user);
                await RecalculateTotalPriceAsync(bookingId);
            }
            return result;
        }

        private async Task RecalculateTotalPriceAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingDetails)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return;

            decimal roomPrice = booking.Room?.Price ?? 0;
            int days = 1;
            if (booking.CheckOutDate.HasValue)
            {
                days = (booking.CheckOutDate.Value.Date - booking.CheckInDate.Date).Days;
                if (days < 1) days = 1;
            }

            decimal roomCost = roomPrice * days;
            decimal serviceCost = 0;

            foreach (var detail in booking.BookingDetails)
            {
                serviceCost += detail.ServicePrice * detail.Quantity;
            }

            booking.TotalPrice = roomCost + serviceCost;
            _context.Entry(booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        private async Task SyncRoomStatusAsync(Booking booking)
        {
            var room = await _roomRepository.GetByIdAsync(booking.RoomId);
            if (room == null) return;

            var customer = await _customerRepository.GetByIdAsync(booking.CustomerId);
            string custName = customer?.FullName ?? "";
            string custPhone = customer?.PhoneNumber ?? "";

            if (booking.Status == "CheckedIn")
            {
                var occupiedStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == "occupied");
                if (occupiedStatus != null)
                {
                    room.RoomStatusId = occupiedStatus.Id;
                    room.CustomerName = custName;
                    room.CustomerPhone = custPhone;
                    room.CheckInDate = booking.CheckInDate;
                }
            }
            else if (booking.Status == "CheckedOut")
            {
                var dirtyStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == "dirty");
                if (dirtyStatus != null)
                {
                    room.RoomStatusId = dirtyStatus.Id;
                    room.CustomerName = null;
                    room.CustomerPhone = null;
                    room.CheckInDate = null;
                }
            }
            else if (booking.Status == "Cancelled")
            {
                if (room.CustomerName == custName)
                {
                    var availStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == "available");
                    if (availStatus != null)
                    {
                        room.RoomStatusId = availStatus.Id;
                        room.CustomerName = null;
                        room.CustomerPhone = null;
                        room.CheckInDate = null;
                    }
                }
            }
            else if (booking.Status == "Confirmed" || booking.Status == "Pending")
            {
                var reservedStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == "reserved");
                if (reservedStatus != null)
                {
                    room.RoomStatusId = reservedStatus.Id;
                    room.CustomerName = custName;
                    room.CustomerPhone = custPhone;
                }
            }

            _context.Entry(room).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
