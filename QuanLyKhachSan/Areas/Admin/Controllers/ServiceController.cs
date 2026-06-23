using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Admin.Services;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceController : Controller
    {
        private readonly IServiceService _serviceService;

        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        // GET: Admin/Service
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var services = await _serviceService.GetAllServicesAsync();
            return View(services);
        }

        // GET: Admin/Service/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Service/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceName,Price,Description,IsActive")] Service service)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Admin";
                    await _serviceService.CreateServiceAsync(service, username);
                    TempData["SuccessMessage"] = $"Đã thêm dịch vụ '{service.ServiceName}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(service);
        }

        // GET: Admin/Service/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _serviceService.GetServiceByIdAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // POST: Admin/Service/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServiceName,Price,Description,IsActive")] Service service)
        {
            if (id != service.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Admin";
                    await _serviceService.UpdateServiceAsync(service, username);
                    TempData["SuccessMessage"] = $"Đã cập nhật dịch vụ '{service.ServiceName}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(service);
        }

        // POST: Admin/Service/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string username = User.Identity?.Name ?? "Admin";
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service != null)
                {
                    await _serviceService.DeleteServiceAsync(id, username);
                    TempData["SuccessMessage"] = $"Đã xóa dịch vụ '{service.ServiceName}' khỏi danh mục!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa dịch vụ: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
