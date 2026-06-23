using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Areas.Admin.Services;
using HotelManagement.Areas.Admin.Repositories;
using HotelManagement.Models;
using HotelManagement.Data;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly ApplicationDbContext _context;

        public RoomController(
            IRoomService roomService,
            IRoomTypeRepository roomTypeRepository,
            ApplicationDbContext context)
        {
            _roomService = roomService;
            _roomTypeRepository = roomTypeRepository;
            _context = context;
        }

        // GET: Admin/Room
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return View(rooms);
        }

        // GET: Admin/Room/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // GET: Admin/Room/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.RoomTypeId = new SelectList(await _roomTypeRepository.GetAllAsync(), "Id", "TypeName");
            ViewBag.RoomStatusId = new SelectList(await _context.RoomStatuses.ToListAsync(), "Id", "StatusName");
            return View();
        }

        // POST: Admin/Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomNumber,RoomTypeId,RoomStatusId,Price,CustomerName,CustomerPhone,CheckInDate")] Room room)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Admin";
                    await _roomService.CreateRoomAsync(room, username);
                    TempData["SuccessMessage"] = $"Đã tạo mới phòng P.{room.RoomNumber} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            ViewBag.RoomTypeId = new SelectList(await _roomTypeRepository.GetAllAsync(), "Id", "TypeName", room.RoomTypeId);
            ViewBag.RoomStatusId = new SelectList(await _context.RoomStatuses.ToListAsync(), "Id", "StatusName", room.RoomStatusId);
            return View(room);
        }

        // GET: Admin/Room/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            ViewBag.RoomTypeId = new SelectList(await _roomTypeRepository.GetAllAsync(), "Id", "TypeName", room.RoomTypeId);
            ViewBag.RoomStatusId = new SelectList(await _context.RoomStatuses.ToListAsync(), "Id", "StatusName", room.RoomStatusId);
            return View(room);
        }

        // POST: Admin/Room/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomNumber,RoomTypeId,RoomStatusId,Price,CustomerName,CustomerPhone,CheckInDate")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Admin";
                    await _roomService.UpdateRoomAsync(room, username);
                    TempData["SuccessMessage"] = $"Cập nhật phòng P.{room.RoomNumber} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            ViewBag.RoomTypeId = new SelectList(await _roomTypeRepository.GetAllAsync(), "Id", "TypeName", room.RoomTypeId);
            ViewBag.RoomStatusId = new SelectList(await _context.RoomStatuses.ToListAsync(), "Id", "StatusName", room.RoomStatusId);
            return View(room);
        }

        // POST: Admin/Room/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string username = User.Identity?.Name ?? "Admin";
                var room = await _roomService.GetRoomByIdAsync(id);
                if (room != null)
                {
                    await _roomService.DeleteRoomAsync(id, username);
                    TempData["SuccessMessage"] = $"Đã xóa phòng P.{room.RoomNumber} khỏi hệ thống!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa phòng: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
