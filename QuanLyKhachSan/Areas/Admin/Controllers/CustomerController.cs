using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Areas.Admin.Services;
using HotelManagement.Areas.Admin.Models;
using CustomerEntity = HotelManagement.Areas.Admin.Models.Customer;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: Admin/Customer
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: Admin/Customer/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber,IdentityCard,Address")] CustomerEntity customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Admin";
                    await _customerService.CreateCustomerAsync(customer, username);
                    TempData["SuccessMessage"] = $"Đã thêm khách hàng {customer.FullName} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(customer);
        }

        // GET: Admin/Customer/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Admin/Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,PhoneNumber,IdentityCard,Address,CreatedAt")] CustomerEntity customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Admin";
                    await _customerService.UpdateCustomerAsync(customer, username);
                    TempData["SuccessMessage"] = $"Cập nhật khách hàng {customer.FullName} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(customer);
        }

        // POST: Admin/Customer/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string username = User.Identity?.Name ?? "Admin";
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer != null)
                {
                    await _customerService.DeleteCustomerAsync(id, username);
                    TempData["SuccessMessage"] = $"Đã xóa khách hàng {customer.FullName} khỏi hệ thống!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa khách hàng: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
