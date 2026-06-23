using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.Areas.Admin.Services;
using Microsoft.AspNetCore.Identity;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(
            ApplicationDbContext context,
            IUserService userService,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _userService = userService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var rooms = await _context.Rooms
                .Include(r => r.RoomStatus)
                .Include(r => r.RoomType)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

            var logs = await _context.AuditLogs.OrderByDescending(l => l.Timestamp).Take(5).ToListAsync();

            ViewBag.Rooms = rooms;
            ViewBag.AuditLogs = logs;

            // Stats using lookup codes
            ViewBag.TotalRooms = rooms.Count;
            ViewBag.AvailableRooms = rooms.Count(r => r.RoomStatus?.StatusCode == "available");
            ViewBag.OccupiedRooms = rooms.Count(r => r.RoomStatus?.StatusCode == "occupied");
            ViewBag.DirtyRooms = rooms.Count(r => r.RoomStatus?.StatusCode == "dirty");
            ViewBag.ReservedRooms = rooms.Count(r => r.RoomStatus?.StatusCode == "reserved");

            // Tính doanh thu từ tổng hóa đơn đã thanh toán
            ViewBag.TotalRevenue = await _context.Invoices
                .Where(i => i.Status == "Paid")
                .SumAsync(i => i.TotalAmount);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(string roomNumber, string customerName, string customerPhone)
        {
            if (string.IsNullOrWhiteSpace(roomNumber) || string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerPhone))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin khách hàng!";
                return RedirectToAction(nameof(Dashboard));
            }

            var room = await _context.Rooms
                .Include(r => r.RoomStatus)
                .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);

            if (room == null)
            {
                TempData["ErrorMessage"] = "Phòng không tồn tại!";
                return RedirectToAction(nameof(Dashboard));
            }

            if (room.RoomStatus?.StatusCode != "available" && room.RoomStatus?.StatusCode != "reserved")
            {
                TempData["ErrorMessage"] = "Phòng không khả dụng để check-in!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Fetch occupied status ID
            var occupiedStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == "occupied");
            if (occupiedStatus == null)
            {
                TempData["ErrorMessage"] = "Trạng thái 'Đang ở' không tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Dashboard));
            }

            room.RoomStatusId = occupiedStatus.Id;
            room.CustomerName = customerName;
            room.CustomerPhone = customerPhone;
            room.CheckInDate = DateTime.Now;

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            // Log Audit
            var user = await _userManager.GetUserAsync(User);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            await _userService.AddAuditLogAsync(
                user?.Id ?? "System", 
                user?.UserName ?? "Admin", 
                $"Nhận phòng P.{room.RoomNumber} cho khách {customerName}", 
                ip);

            TempData["SuccessMessage"] = $"Đã check-in phòng P.{room.RoomNumber} thành công!";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(string roomNumber)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomStatus)
                .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);

            if (room == null || room.RoomStatus?.StatusCode != "occupied")
            {
                TempData["ErrorMessage"] = "Yêu cầu trả phòng không hợp lệ!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Fetch dirty status ID
            var dirtyStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == "dirty");
            if (dirtyStatus == null)
            {
                TempData["ErrorMessage"] = "Trạng thái 'Cần dọn' không tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Tính số đêm ở và tạo hóa đơn thanh toán
            int stayDays = 1; // Tối thiểu 1 đêm
            if (room.CheckInDate.HasValue)
            {
                stayDays = (DateTime.Now.Date - room.CheckInDate.Value.Date).Days;
                if (stayDays < 1) stayDays = 1; // Tối thiểu 1 đêm
            }

            decimal totalAmount = stayDays * room.Price;

            // Tạo hóa đơn thanh toán (Invoice) với trạng thái "Paid"
            var invoice = new HotelManagement.Areas.Admin.Models.Invoice
            {
                InvoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}-{room.RoomNumber}",
                TotalAmount = totalAmount,
                Status = "Paid",
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow
            };
            await _context.Invoices.AddAsync(invoice);

            // Lưu thông tin khách trước khi xóa
            string guestName = room.CustomerName ?? "Khách lẻ";

            // Cập nhật trạng thái phòng
            room.RoomStatusId = dirtyStatus.Id;
            room.CustomerName = null;
            room.CustomerPhone = null;
            room.CheckInDate = null;

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            // Log Audit
            var user = await _userManager.GetUserAsync(User);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            await _userService.AddAuditLogAsync(
                user?.Id ?? "System", 
                user?.UserName ?? "Admin", 
                $"Trả phòng P.{room.RoomNumber} (Khách hàng cũ: {guestName})", 
                ip);

            TempData["SuccessMessage"] = $"Đã trả phòng P.{room.RoomNumber} thành công! Thanh toán: {totalAmount:N0}đ ({stayDays} đêm). Trạng thái phòng hiện tại: Cần dọn dẹp.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanRoom(string roomNumber)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomStatus)
                .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);

            if (room == null || room.RoomStatus?.StatusCode != "dirty")
            {
                TempData["ErrorMessage"] = "Yêu cầu dọn phòng không hợp lệ!";
                return RedirectToAction(nameof(Dashboard));
            }

            // Fetch available status ID
            var availStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == "available");
            if (availStatus == null)
            {
                TempData["ErrorMessage"] = "Trạng thái 'Còn trống' không tồn tại trong hệ thống!";
                return RedirectToAction(nameof(Dashboard));
            }

            room.RoomStatusId = availStatus.Id;

            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();

            // Log Audit
            var user = await _userManager.GetUserAsync(User);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            await _userService.AddAuditLogAsync(
                user?.Id ?? "System", 
                user?.UserName ?? "Admin", 
                $"Hoàn thành dọn dẹp phòng P.{room.RoomNumber}", 
                ip);

            TempData["SuccessMessage"] = $"Phòng P.{room.RoomNumber} đã dọn dẹp xong và sẵn sàng đón khách!";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
