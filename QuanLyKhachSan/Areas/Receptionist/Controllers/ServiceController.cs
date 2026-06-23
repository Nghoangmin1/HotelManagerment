using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Areas.Receptionist.Services;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Data;

namespace HotelManagement.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist,Admin")]
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public ServiceController(
            IServiceService serviceService,
            IBookingService bookingService,
            ApplicationDbContext context)
        {
            _serviceService = serviceService;
            _bookingService = bookingService;
            _context = context;
        }

        // GET: Receptionist/Service
        [HttpGet]
        public async Task<IActionResult> Index(int? bookingId)
        {
            // Get active bookings (CheckedIn) to display service usage for
            var activeBookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Customer)
                .Where(b => b.Status == "CheckedIn" || b.Status == "Pending" || b.Status == "Confirmed")
                .ToListAsync();

            ViewBag.ActiveBookings = activeBookings;

            int selectedBookingId = bookingId ?? (activeBookings.FirstOrDefault()?.Id ?? 0);
            ViewBag.SelectedBookingId = selectedBookingId;

            if (selectedBookingId > 0)
            {
                var usages = await _serviceService.GetBookingUsagesAsync(selectedBookingId);
                var orders = await _serviceService.GetBookingOrdersAsync(selectedBookingId);
                
                ViewBag.Usages = usages;
                ViewBag.Orders = orders;
                
                var currentBooking = activeBookings.FirstOrDefault(b => b.Id == selectedBookingId);
                ViewBag.CurrentBooking = currentBooking;
            }
            else
            {
                ViewBag.Usages = Enumerable.Empty<ServiceUsage>();
                ViewBag.Orders = Enumerable.Empty<ServiceOrder>();
                ViewBag.CurrentBooking = null;
            }

            return View();
        }

        // GET: Receptionist/Service/Assign?bookingId=5
        [HttpGet]
        public async Task<IActionResult> Assign(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return NotFound();

            var services = await _serviceService.GetActiveServicesAsync();
            ViewBag.ServiceId = new SelectList(services, "Id", "ServiceName");
            ViewBag.Booking = booking;

            return View();
        }

        // POST: Receptionist/Service/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int bookingId, int serviceId, int quantity, string notes)
        {
            try
            {
                string username = User.Identity?.Name ?? "Receptionist";
                var success = await _serviceService.AssignServiceToBookingAsync(bookingId, serviceId, quantity, notes, username);
                if (success)
                {
                    TempData["SuccessMessage"] = "Đã ghi nhận sử dụng dịch vụ tại phòng thành công!";
                    return RedirectToAction(nameof(Index), new { bookingId = bookingId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi khi ghi nhận sử dụng dịch vụ!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Assign), new { bookingId = bookingId });
        }

        // POST: Receptionist/Service/ProcessOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(int orderId, string status)
        {
            try
            {
                string username = User.Identity?.Name ?? "Receptionist";
                var success = await _serviceService.ProcessServiceOrderAsync(orderId, status, username);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Đã cập nhật đơn đặt dịch vụ sang: {status}!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật đơn đặt dịch vụ!";
                }

                var order = await _context.ServiceOrders.FindAsync(orderId);
                if (order != null)
                {
                    return RedirectToAction(nameof(Index), new { bookingId = order.BookingId });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
