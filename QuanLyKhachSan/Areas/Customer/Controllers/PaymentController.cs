using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Areas.Customer.Services;
using HotelManagement.Models;
using HotelManagement.Data;

namespace HotelManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IAccountService _accountService;
        private readonly UserManager<AppUser> _userManager;

        public PaymentController(
            IPaymentService paymentService,
            IBookingService bookingService,
            IAccountService accountService,
            UserManager<AppUser> userManager)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _accountService = accountService;
            _userManager = userManager;
        }

        // GET: Customer/Payment/Checkout?bookingId=5
        [HttpGet]
        public async Task<IActionResult> Checkout(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                return Challenge();
            }

            // Verify booking belongs to this customer
            var booking = await _bookingService.GetCustomerBookingDetailsAsync(bookingId, customer.Id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.Status == "CheckedOut" || booking.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Đặt phòng này đã hoàn tất checkout hoặc đã hủy, không thể thanh toán!";
                return RedirectToAction("Details", "Booking", new { id = bookingId });
            }

            var invoice = await _paymentService.GetInvoiceByBookingIdAsync(bookingId);
            if (invoice == null)
            {
                return NotFound();
            }

            ViewBag.Booking = booking;
            return View(invoice);
        }

        // POST: Customer/Payment/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int bookingId, decimal amount, string cardNumber, string cardHolder, string expiryDate, string cvv)
        {
            var userId = _userManager.GetUserId(User);
            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                return Challenge();
            }

            try
            {
                var booking = await _bookingService.GetCustomerBookingDetailsAsync(bookingId, customer.Id);
                if (booking == null)
                {
                    return NotFound();
                }

                string username = User.Identity?.Name ?? "Customer";
                var success = await _paymentService.ProcessOnlinePaymentAsync(bookingId, amount, cardNumber, cardHolder, expiryDate, cvv, username);

                if (success)
                {
                    TempData["SuccessMessage"] = "Thanh toán đặt phòng trực tuyến thành công!";
                    return RedirectToAction(nameof(Receipt), new { bookingId = bookingId });
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi hệ thống khi xử lý thanh toán!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Checkout), new { bookingId = bookingId });
        }

        // GET: Customer/Payment/Receipt?bookingId=5
        [HttpGet]
        public async Task<IActionResult> Receipt(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                return Challenge();
            }

            var booking = await _bookingService.GetCustomerBookingDetailsAsync(bookingId, customer.Id);
            if (booking == null)
            {
                return NotFound();
            }

            var invoice = await _paymentService.GetInvoiceByBookingIdAsync(bookingId);
            if (invoice == null)
            {
                return NotFound();
            }

            ViewBag.Booking = booking;
            return View(invoice);
        }
    }
}
