using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Areas.Customer.Services;
using HotelManagement.Models;
using HotelManagement.Data;

namespace HotelManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly IBookingService _bookingService;
        private readonly IAccountService _accountService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ServiceController(
            IServiceService serviceService,
            IBookingService bookingService,
            IAccountService accountService,
            UserManager<AppUser> userManager,
            ApplicationDbContext context)
        {
            _serviceService = serviceService;
            _bookingService = bookingService;
            _accountService = accountService;
            _userManager = userManager;
            _context = context;
        }

        // GET: Customer/Service
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var services = await _serviceService.GetActiveServicesAsync();
            return View(services);
        }

        // GET: Customer/Service/Order
        [HttpGet]
        public async Task<IActionResult> Order(int serviceId)
        {
            var service = await _serviceService.GetServiceByIdAsync(serviceId);
            if (service == null || !service.IsActive)
            {
                TempData["ErrorMessage"] = "Dịch vụ không tồn tại hoặc đã ngừng phục vụ!";
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);
            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Vui lòng cập nhật thông tin cá nhân trước khi sử dụng dịch vụ!";
                return RedirectToAction("Profile", "Account");
            }

            // Get active bookings of this customer to select from
            var bookings = await _bookingService.GetCustomerBookingHistoryAsync(customer.Id);
            var activeBookings = bookings.Where(b => b.Status == "Pending" || b.Status == "Confirmed" || b.Status == "CheckedIn").ToList();

            if (!activeBookings.Any())
            {
                TempData["ErrorMessage"] = "Bạn không có lịch đặt phòng hoạt động nào để đặt dịch vụ!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Service = service;
            ViewBag.BookingId = new SelectList(activeBookings.Select(b => new {
                Id = b.Id,
                DisplayText = $"Mã đặt phòng #{b.Id} - Phòng P.{b.Room?.RoomNumber} ({b.CheckInDate:dd/MM/yyyy} - {b.CheckOutDate:dd/MM/yyyy})"
            }), "Id", "DisplayText");

            return View();
        }

        // POST: Customer/Service/Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Order(int bookingId, int serviceId, int quantity, string notes)
        {
            var userId = _userManager.GetUserId(User);
            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                return Challenge();
            }

            try
            {
                // Verify booking belongs to this customer
                var booking = await _bookingService.GetCustomerBookingDetailsAsync(bookingId, customer.Id);
                if (booking == null)
                {
                    return NotFound();
                }

                string username = User.Identity?.Name ?? "Customer";
                var success = await _serviceService.OrderServiceAsync(bookingId, serviceId, quantity, notes, username);
                if (success)
                {
                    TempData["SuccessMessage"] = "Đặt dịch vụ trực tuyến thành công! Yêu cầu của bạn đang chờ phê duyệt.";
                    return RedirectToAction(nameof(History));
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể đặt dịch vụ!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Order), new { serviceId = serviceId });
        }

        // GET: Customer/Service/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);
            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                return Challenge();
            }

            var bookings = await _bookingService.GetCustomerBookingHistoryAsync(customer.Id);
            var allOrders = new List<Admin.Models.ServiceOrder>();

            foreach (var b in bookings)
            {
                var orders = await _serviceService.GetCustomerBookingOrdersAsync(b.Id);
                foreach (var o in orders)
                {
                    o.Booking = b; // Associate booking to display room number
                    allOrders.Add(o);
                }
            }

            return View(allOrders.OrderByDescending(o => o.OrderDate));
        }
    }
}
