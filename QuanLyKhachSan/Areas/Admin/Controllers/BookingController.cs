using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using HotelManagement.Areas.Admin.Services;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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

        // GET: Admin/Booking
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return View(bookings);
        }

        // GET: Admin/Booking/Details/5
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

        // GET: Admin/Booking/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            var rooms = await _roomService.GetAllRoomsAsync();

            ViewBag.CustomerId = new SelectList(customers, "Id", "FullName");
            
            // Format room selection text to include type and price
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

        // POST: Admin/Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,RoomId,CheckInDate,CheckOutDate,Status")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Admin";
                    await _bookingService.CreateBookingAsync(booking, username);
                    TempData["SuccessMessage"] = "Tạo mới đặt phòng thành công!";
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

        // POST: Admin/Booking/AddService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(int bookingId, string serviceName, decimal servicePrice, int quantity)
        {
            try
            {
                string username = User.Identity?.Name ?? "Admin";
                var detail = new BookingDetail
                {
                    ServiceName = serviceName,
                    ServicePrice = servicePrice,
                    Quantity = quantity
                };
                await _bookingService.AddServiceDetailAsync(bookingId, detail, username);
                TempData["SuccessMessage"] = $"Đã thêm dịch vụ '{serviceName}' vào hóa đơn thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi thêm dịch vụ: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        // POST: Admin/Booking/DeleteService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id, int bookingId)
        {
            try
            {
                string username = User.Identity?.Name ?? "Admin";
                await _bookingService.RemoveServiceDetailAsync(id, username);
                TempData["SuccessMessage"] = "Đã loại bỏ dịch vụ khỏi hóa đơn!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa dịch vụ: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        // POST: Admin/Booking/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null) return NotFound();

                booking.Status = status;
                if (status == "CheckedOut")
                {
                    booking.CheckOutDate = DateTime.Now;
                }

                string username = User.Identity?.Name ?? "Admin";
                await _bookingService.UpdateBookingAsync(booking, username);
                TempData["SuccessMessage"] = $"Cập nhật trạng thái đặt phòng thành công sang: {status}!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: Admin/Booking/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string username = User.Identity?.Name ?? "Admin";
                await _bookingService.DeleteBookingAsync(id, username);
                TempData["SuccessMessage"] = "Đã xóa lịch sử đặt phòng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa đặt phòng: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
