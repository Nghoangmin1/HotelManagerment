using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using HotelManagement.Areas.Receptionist.Services;
using HotelManagement.Shared;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Models;
using HotelManagement.Data;

namespace HotelManagement.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist,Admin")]
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly ApplicationDbContext _context;

        public RoomController(
            IRoomService roomService,
            ApplicationDbContext context)
        {
            _roomService = roomService;
            _context = context;
        }

        // GET: Receptionist/Room
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return View(rooms);
        }

        // GET: Receptionist/Room/UpdateStatus/5
        [HttpGet]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var statuses = await _context.RoomStatuses.ToListAsync();
            ViewBag.StatusCode = new SelectList(statuses, "StatusCode", "StatusName", room.RoomStatus?.StatusCode);

            return View(room);
        }

        // POST: Receptionist/Room/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string statusCode)
        {
            try
            {
                string username = User.Identity?.Name ?? "Receptionist";
                await _roomService.UpdateRoomStatusAsync(id, statusCode, username);
                TempData["SuccessMessage"] = "Cập nhật trạng thái phòng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái phòng: {ex.Message}";
                var room = await _roomService.GetRoomByIdAsync(id);
                if (room != null)
                {
                    var statuses = await _context.RoomStatuses.ToListAsync();
                    ViewBag.StatusCode = new SelectList(statuses, "StatusCode", "StatusName", statusCode);
                    return View(room);
                }
                return NotFound();
            }
        }
    }
}
