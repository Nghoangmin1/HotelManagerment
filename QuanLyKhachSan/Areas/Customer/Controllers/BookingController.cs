using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Areas.Customer.Services;
using HotelManagement.Areas.Customer.Models;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Models;
using HotelManagement.Shared;
using HotelManagement.Data;

namespace HotelManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IAccountService _accountService;
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BookingController(
            IBookingService bookingService,
            IAccountService accountService,
            UserManager<AppUser> userManager,
            ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _accountService = accountService;
            _userManager = userManager;
            _context = context;
        }

        // GET: Customer/Booking/RoomSearch
        [HttpGet]
        public async Task<IActionResult> RoomSearch(DateTime? checkIn, DateTime? checkOut)
        {
            var inDate = checkIn ?? DateTime.Today;
            var outDate = checkOut ?? DateTime.Today.AddDays(1);

            ViewBag.CheckIn = inDate.ToString("yyyy-MM-dd");
            ViewBag.CheckOut = outDate.ToString("yyyy-MM-dd");

            try
            {
                var roomTypes = await _bookingService.SearchAvailableRoomTypesAsync(inDate, outDate);
                return View(roomTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new System.Collections.Generic.List<object>());
            }
        }

        // GET: Customer/Booking/RoomDetail/5
        [HttpGet]
        public async Task<IActionResult> RoomDetail(int id)
        {
            var typeRecord = await _context.RoomTypes.FindAsync(id);
            if (typeRecord == null) return NotFound();
            return View(typeRecord);
        }

        // GET: Customer/Booking/Create
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(int roomTypeId, string checkIn, string checkOut)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Bạn cần cập nhật thông tin cá nhân trước khi thực hiện đặt phòng!";
                return RedirectToAction("Profile", "Account");
            }

            if (!DateTime.TryParse(checkIn, out DateTime inDate)) inDate = DateTime.Today;
            if (!DateTime.TryParse(checkOut, out DateTime outDate)) outDate = DateTime.Today.AddDays(1);

            var model = new CustomerBookingModel
            {
                RoomTypeId = roomTypeId,
                CheckInDate = inDate,
                CheckOutDate = outDate
            };

            // Fetch room type base price to calculate estimated price
            var typeRecord = await _context.RoomTypes.FindAsync(roomTypeId);
            if (typeRecord != null)
            {
                ViewBag.RoomTypeName = typeRecord.TypeName;
                ViewBag.BasePrice = typeRecord.BasePrice;
                int days = (outDate.Date - inDate.Date).Days;
                if (days < 1) days = 1;
                model.EstimatedPrice = typeRecord.BasePrice * days;
            }

            return View(model);
        }

        // POST: Customer/Booking/Create
        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerBookingModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Profile", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var booking = await _bookingService.CreateOnlineBookingAsync(customer.Id, model.RoomTypeId, model.CheckInDate, model.CheckOutDate);
                    if (booking != null)
                    {
                        TempData["SuccessMessage"] = "Gửi yêu cầu đặt phòng thành công! Trạng thái đặt phòng của bạn là Chờ duyệt (Pending).";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            // Repopulate view data on failure
            var typeRecord = await _context.RoomTypes.FindAsync(model.RoomTypeId);
            if (typeRecord != null)
            {
                ViewBag.RoomTypeName = typeRecord.TypeName;
                ViewBag.BasePrice = typeRecord.BasePrice;
            }

            return View(model);
        }

        // GET: Customer/Booking
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                return View(new System.Collections.Generic.List<Booking>());
            }

            var bookings = await _bookingService.GetCustomerBookingHistoryAsync(customer.Id);
            return View(bookings);
        }

        // GET: Customer/Booking/Details/5
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                return NotFound();
            }

            var booking = await _bookingService.GetCustomerBookingDetailsAsync(id, customer.Id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }
    }
}
