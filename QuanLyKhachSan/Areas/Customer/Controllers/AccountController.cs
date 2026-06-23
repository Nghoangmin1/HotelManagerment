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
using CustomerEntity = HotelManagement.Areas.Admin.Models.Customer;

namespace HotelManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAccountService _accountService;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IAccountService accountService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _accountService = accountService;
        }

        // GET: Customer/Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            return View();
        }

        // POST: Customer/Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, string fullName, string phoneNumber, string identityCard, string address)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            if (!CustomerAccountValidationHelper.IsValidUsername(username))
            {
                ModelState.AddModelError(string.Empty, "Tên tài khoản không hợp lệ (từ 4 đến 30 ký tự, chỉ chứa chữ cái, số và dấu gạch dưới).");
            }
            if (!CustomerAccountValidationHelper.IsValidPassword(password))
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu phải từ 6 ký tự trở lên, bao gồm ít nhất 1 chữ hoa, 1 chữ thường và 1 chữ số.");
            }
            if (!CustomerAccountValidationHelper.ArePasswordsMatching(password, confirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp!");
            }
            if (!CustomerValidationHelper.IsValidFullName(fullName))
            {
                ModelState.AddModelError(string.Empty, "Họ và tên không hợp lệ.");
            }
            if (!CustomerValidationHelper.IsValidPhone(phoneNumber))
            {
                ModelState.AddModelError(string.Empty, "Số điện thoại không hợp lệ.");
            }
            if (!CustomerValidationHelper.IsValidIdentityCard(identityCard))
            {
                ModelState.AddModelError(string.Empty, "Số CMND/CCCD/Hộ chiếu không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = username.Trim(),
                    Email = email.Trim(),
                    FullName = fullName.Trim(),
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");

                    // Create Customer Profile linked to this user
                    var customer = new CustomerEntity
                    {
                        FullName = fullName.Trim(),
                        Email = email.Trim(),
                        PhoneNumber = phoneNumber.Trim(),
                        IdentityCard = identityCard.Trim(),
                        Address = address.Trim()
                    };

                    try
                    {
                        await _accountService.RegisterCustomerProfileAsync(user.Id, customer);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                    catch (Exception ex)
                    {
                        // Rollback user creation on profile failure
                        await _userManager.DeleteAsync(user);
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View();
        }

        // GET: Customer/Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Customer/Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Tài khoản và mật khẩu không được trống.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                    Logger.LogInfo($"[CUSTOMER LOGIN] User '{username}' logged in from IP {ip}");
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không chính xác.");
            return View();
        }

        // GET: Customer/Account/Profile
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var customer = await _accountService.GetProfileByUserIdAsync(userId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ cá nhân của bạn. Vui lòng cập nhật hồ sơ.";
                return View(new CustomerProfileModel());
            }

            var model = new CustomerProfileModel
            {
                FullName = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                IdentityCard = customer.IdentityCard,
                Address = customer.Address
            };

            return View(model);
        }

        // POST: Customer/Account/Profile
        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CustomerProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var customerUpdates = new CustomerEntity
            {
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IdentityCard = model.IdentityCard,
                Address = model.Address
            };

            try
            {
                await _accountService.UpdateProfileAsync(userId, customerUpdates);
                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }
}
