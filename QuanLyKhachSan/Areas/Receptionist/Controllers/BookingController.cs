using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using HotelManagement.Areas.Receptionist.Services;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist,Admin")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ICustomerService _customerService;
        private readonly IRoomService _roomService;

        public BookingController(
            IBookingService bookingService,
            ICustomerService customerService,
            IRoomService roomService)
        {
            _bookingService = bookingService;
            _customerService = customerService;
            _roomService = roomService;
        }

        // GET: Receptionist/Booking
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return View(bookings);
        }

        // GET: Receptionist/Booking/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        // GET: Receptionist/Booking/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var rooms = await _roomService.GetAllRoomsAsync();

            ViewBag.CustomerId = new SelectList(customers, "Id", "FullName");
            
            var roomItems = new System.Collections.Generic.List<object>();
            foreach (var r in rooms)
            {
                roomItems.Add(new { Id = r.Id, Text = $"P.{r.RoomNumber} - {r.RoomType?.TypeName} ({r.Price:N0}đ)" });
            }
            ViewBag.RoomId = new SelectList(roomItems, "Id", "Text");

            return View(new Booking 
            { 
                CheckInDate = DateTime.Today,
                Status = "Pending"
            });
        }

        // POST: Receptionist/Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,RoomId,CheckInDate,CheckOutDate,Status")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Receptionist";
                    await _bookingService.CreateBookingAsync(booking, username);
                    TempData["SuccessMessage"] = "Tạo mới yêu cầu đặt phòng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            var customers = await _customerService.GetAllCustomersAsync();
            var rooms = await _roomService.GetAllRoomsAsync();
            ViewBag.CustomerId = new SelectList(customers, "Id", "FullName", booking.CustomerId);
            
            var roomItems = new System.Collections.Generic.List<object>();
            foreach (var r in rooms)
            {
                roomItems.Add(new { Id = r.Id, Text = $"P.{r.RoomNumber} - {r.RoomType?.TypeName} ({r.Price:N0}đ)" });
            }
            ViewBag.RoomId = new SelectList(roomItems, "Id", "Text", booking.RoomId);

            return View(booking);
        }

        // POST: Receptionist/Booking/CheckIn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            try
            {
                string username = User.Identity?.Name ?? "Receptionist";
                await _bookingService.CheckInAsync(id, username);
                TempData["SuccessMessage"] = "Nhận phòng (Check-in) thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi nhận phòng: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: Receptionist/Booking/CheckOut/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(int id)
        {
            try
            {
                string username = User.Identity?.Name ?? "Receptionist";
                await _bookingService.CheckOutAsync(id, username);
                TempData["SuccessMessage"] = "Trả phòng (Check-out) và tính tiền thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi trả phòng: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: Receptionist/Booking/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking != null && booking.Status == "Pending")
                {
                    booking.Status = "Confirmed";
                    string username = User.Identity?.Name ?? "Receptionist";
                    await _bookingService.UpdateBookingAsync(booking, username);
                    TempData["SuccessMessage"] = "Đã xác nhận yêu cầu đặt phòng thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xác nhận: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: Receptionist/Booking/AddService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(int bookingId, string serviceName, decimal servicePrice, int quantity)
        {
            try
            {
                string username = User.Identity?.Name ?? "Receptionist";
                var detail = new BookingDetail
                {
                    ServiceName = serviceName,
                    ServicePrice = servicePrice,
                    Quantity = quantity
                };
                await _bookingService.AddServiceDetailAsync(bookingId, detail, username);
                TempData["SuccessMessage"] = $"Đã thêm dịch vụ '{serviceName}' thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thêm dịch vụ: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        // POST: Receptionist/Booking/DeleteService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id, int bookingId)
        {
            try
            {
                string username = User.Identity?.Name ?? "Receptionist";
                await _bookingService.RemoveServiceDetailAsync(id, username);
                TempData["SuccessMessage"] = "Đã xóa dịch vụ khỏi hóa đơn!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa dịch vụ: {ex.Message}";
            }
            return RedirectToAction(nameof(Details), new { id = bookingId });
        }
    }
}
