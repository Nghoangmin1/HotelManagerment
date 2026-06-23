using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagement.Areas.Receptionist.Services;
using CustomerEntity = HotelManagement.Areas.Admin.Models.Customer;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Receptionist.Controllers
{
    [Area("Receptionist")]
    [Authorize(Roles = "Receptionist,Admin")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: Receptionist/Customer
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return View(customers);
        }

        // GET: Receptionist/Customer/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Receptionist/Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber,IdentityCard,Address")] CustomerEntity customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Receptionist";
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

        // GET: Receptionist/Customer/Edit/5
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

        // POST: Receptionist/Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,PhoneNumber,IdentityCard,Address,CreatedAt,UserId")] CustomerEntity customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string username = User.Identity?.Name ?? "Receptionist";
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
    }
}
