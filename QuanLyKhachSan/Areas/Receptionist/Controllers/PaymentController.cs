using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Areas.Receptionist.Services;
using HotelManagement.Data;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist,Admin")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public PaymentController(
            IPaymentService paymentService,
            IBookingService bookingService,
            ApplicationDbContext context)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _context = context;
        }

        // GET: Receptionist/Payment
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var invoices = await _paymentService.GetAllInvoicesAsync();
            return View(invoices);
        }

        // GET: Receptionist/Payment/Checkout?bookingId=5
        [HttpGet]
        public async Task<IActionResult> Checkout(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r!.RoomType)
                .Include(b => b.Customer)
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return NotFound();

            if (booking.Status == "CheckedOut")
            {
                TempData["ErrorMessage"] = "Đặt phòng này đã được thực hiện checkout và thanh toán trước đó!";
                return RedirectToAction("Details", "Booking", new { id = bookingId });
            }

            // Fetch service usages
            var serviceUsages = await _context.ServiceUsages
                .Include(su => su.Service)
                .Where(su => su.BookingId == bookingId)
                .ToListAsync();

            // Calculate room charges
            int days = 1;
            var checkoutDate = booking.CheckOutDate ?? DateTime.UtcNow;
            days = (checkoutDate.Date - booking.CheckInDate.Date).Days;
            if (days < 1) days = 1;

            decimal roomCost = (booking.Room?.Price ?? booking.TotalPrice) * days;
            decimal serviceCost = booking.BookingDetails.Sum(d => d.ServicePrice * d.Quantity);
            decimal serviceUsagesCost = serviceUsages.Sum(su => su.Quantity * (su.Service?.Price ?? 0));
            decimal subTotal = roomCost + serviceCost + serviceUsagesCost;

            ViewBag.Booking = booking;
            ViewBag.ServiceUsages = serviceUsages;
            ViewBag.Days = days;
            ViewBag.RoomCost = roomCost;
            ViewBag.ServiceCost = serviceCost;
            ViewBag.ServiceUsagesCost = serviceUsagesCost;
            ViewBag.SubTotal = subTotal;

            // Return default preview parameters
            ViewBag.Tax = subTotal * 0.1m;
            ViewBag.Discount = 0m;
            ViewBag.TotalAmount = subTotal + (subTotal * 0.1m);

            return View();
        }

        // POST: Receptionist/Payment/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int bookingId, decimal discount, decimal tax, decimal amount, string method)
        {
            try
            {
                string receptionist = User.Identity?.Name ?? "Receptionist";
                var success = await _paymentService.ProcessReceptionistPaymentAsync(bookingId, amount, method, discount, tax, receptionist);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Thanh toán hóa đơn checkout và trả phòng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi khi xử lý thanh toán hóa đơn checkout!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Checkout), new { bookingId = bookingId });
        }
    }
}
