using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.Areas.Admin.Repositories;
using HotelManagement.Areas.Admin.Services;
using HotelManagement.Shared;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIGURE DATABASE CONNECTION (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. CONFIGURE ASP.NET IDENTITY AUTHENTICATION SERVICE
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

// 3. CONFIGURE SESSION MANAGEMENT SERVICE
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

// 4. REGISTER REPOSITORIES AND SERVICES
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// REGISTER ADMIN AREA REPOSITORIES & SERVICES
builder.Services.AddScoped<HotelManagement.Areas.Admin.Repositories.IReportRepository, HotelManagement.Areas.Admin.Repositories.ReportRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Admin.Repositories.IStatisticRepository, HotelManagement.Areas.Admin.Repositories.StatisticRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Admin.Services.IReportService, HotelManagement.Areas.Admin.Services.ReportService>();
builder.Services.AddScoped<HotelManagement.Areas.Admin.Services.IStatisticService, HotelManagement.Areas.Admin.Services.StatisticService>();

// REGISTER CUSTOMER AREA REPOSITORIES & SERVICES
builder.Services.AddScoped<HotelManagement.Areas.Customer.Repositories.ICustomerRepository, HotelManagement.Areas.Customer.Repositories.CustomerRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Customer.Repositories.IBookingRepository, HotelManagement.Areas.Customer.Repositories.BookingRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Customer.Repositories.IServiceRepository, HotelManagement.Areas.Customer.Repositories.ServiceRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Customer.Repositories.IPaymentRepository, HotelManagement.Areas.Customer.Repositories.PaymentRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Customer.Services.IAccountService, HotelManagement.Areas.Customer.Services.AccountService>();
builder.Services.AddScoped<HotelManagement.Areas.Customer.Services.IBookingService, HotelManagement.Areas.Customer.Services.BookingService>();
builder.Services.AddScoped<HotelManagement.Areas.Customer.Services.IServiceService, HotelManagement.Areas.Customer.Services.ServiceService>();
builder.Services.AddScoped<HotelManagement.Areas.Customer.Services.IPaymentService, HotelManagement.Areas.Customer.Services.PaymentService>();

// REGISTER RECEPTIONIST AREA REPOSITORIES & SERVICES
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Repositories.ICustomerRepository, HotelManagement.Areas.Receptionist.Repositories.CustomerRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Repositories.IBookingRepository, HotelManagement.Areas.Receptionist.Repositories.BookingRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Repositories.IRoomRepository, HotelManagement.Areas.Receptionist.Repositories.RoomRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Repositories.IServiceRepository, HotelManagement.Areas.Receptionist.Repositories.ServiceRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Repositories.IPaymentRepository, HotelManagement.Areas.Receptionist.Repositories.PaymentRepository>();
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Services.ICustomerService, HotelManagement.Areas.Receptionist.Services.CustomerService>();
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Services.IBookingService, HotelManagement.Areas.Receptionist.Services.BookingService>();
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Services.IRoomService, HotelManagement.Areas.Receptionist.Services.RoomService>();
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Services.IServiceService, HotelManagement.Areas.Receptionist.Services.ServiceService>();
builder.Services.AddScoped<HotelManagement.Areas.Receptionist.Services.IPaymentService, HotelManagement.Areas.Receptionist.Services.PaymentService>();

var app = builder.Build();

// 5. SEED DATABASE ON STARTUP
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        // Seed Roles
        string[] rolesToSeed = { "Admin", "Receptionist", "Customer" };
        foreach (var roleName in rolesToSeed)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                string desc = roleName switch
                {
                    "Admin" => "Quản trị viên hệ thống",
                    "Receptionist" => "Nhân viên lễ tân",
                    "Customer" => "Khách hàng đăng ký trực tuyến",
                    _ => ""
                };
                var role = new AppRole(roleName, desc);
                await roleManager.CreateAsync(role);
                Logger.LogInfo($"Created default role: {roleName}");
            }
        }

        // Seed Default Admin User
        string adminEmail = "admin@hotel.com";
        string adminUsername = "admin";
        var adminUser = await userManager.FindByNameAsync(adminUsername);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                FullName = "Quản trị viên",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Logger.LogInfo($"Created default admin account: {adminEmail} / Admin@123");
            }
        }

        // Seed Default Receptionist User
        string recepEmail = "receptionist@hotel.com";
        string recepUsername = "receptionist";
        var recepUser = await userManager.FindByNameAsync(recepUsername);
        if (recepUser == null)
        {
            recepUser = new AppUser
            {
                UserName = recepUsername,
                Email = recepEmail,
                FullName = "Lễ tân viên",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(recepUser, "Receptionist@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(recepUser, "Receptionist");
                Logger.LogInfo($"Created default receptionist account: {recepEmail} / Receptionist@123");
            }
        }

        // Seed Default Customer User
        string customerEmail = "customer@hotel.com";
        string customerUsername = "customer";
        var customerUser = await userManager.FindByNameAsync(customerUsername);
        if (customerUser == null)
        {
            customerUser = new AppUser
            {
                UserName = customerUsername,
                Email = customerEmail,
                FullName = "Khách hàng mẫu",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(customerUser, "Customer@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(customerUser, "Customer");
                Logger.LogInfo($"Created default customer account: {customerEmail} / Customer@123");
            }
        }

        // Ensure the corresponding Customer profile record exists
        if (customerUser != null)
        {
            if (!context.Customers.Any(c => c.UserId == customerUser.Id))
            {
                context.Customers.Add(new HotelManagement.Areas.Admin.Models.Customer
                {
                    UserId = customerUser.Id,
                    FullName = "Khách hàng mẫu",
                    Email = customerEmail,
                    PhoneNumber = "0900000000",
                    IdentityCard = "000000000000",
                    Address = "Địa chỉ mẫu",
                    CreatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
                Logger.LogInfo("Seeded Customer profile record.");
            }
        }

        // Seed Room Types lookup table
        if (!context.RoomTypes.Any())
        {
            context.RoomTypes.AddRange(
                new RoomType { TypeName = "Phòng Đơn Standard", BasePrice = 350000, Description = "Diện tích 20m², 1 giường đơn thích hợp cho khách du lịch cá nhân." },
                new RoomType { TypeName = "Phòng Đôi Standard", BasePrice = 500000, Description = "Diện tích 25m², 2 giường đơn hoặc 1 giường đôi tiện lợi." },
                new RoomType { TypeName = "Phòng Đôi Deluxe", BasePrice = 800000, Description = "Diện tích 30m², đầy đủ trang thiết bị cao cấp, view thành phố." },
                new RoomType { TypeName = "Phòng Suite VIP", BasePrice = 1500000, Description = "Diện tích 45m², phòng khách riêng biệt, bồn tắm nằm cao cấp." }
            );
            await context.SaveChangesAsync();
            Logger.LogInfo("Seeded Room Types.");
        }

        // Seed Room Statuses lookup table
        if (!context.RoomStatuses.Any())
        {
            context.RoomStatuses.AddRange(
                new RoomStatus { StatusCode = "available", StatusName = "Còn trống", Description = "Phòng sạch sẽ, sẵn sàng đón khách." },
                new RoomStatus { StatusCode = "occupied", StatusName = "Đang ở", Description = "Đang có khách lưu trú." },
                new RoomStatus { StatusCode = "dirty", StatusName = "Cần dọn", Description = "Khách đã trả phòng, đang chờ dọn dẹp vệ sinh." },
                new RoomStatus { StatusCode = "reserved", StatusName = "Đặt trước", Description = "Đã được đặt cọc trước bởi khách hàng." }
            );
            await context.SaveChangesAsync();
            Logger.LogInfo("Seeded Room Statuses.");
        }

        // Seed normalized Rooms
        if (!context.Rooms.Any())
        {
            var singleType = context.RoomTypes.First(t => t.TypeName == "Phòng Đơn Standard").Id;
            var doubleType = context.RoomTypes.First(t => t.TypeName == "Phòng Đôi Standard").Id;
            var deluxeType = context.RoomTypes.First(t => t.TypeName == "Phòng Đôi Deluxe").Id;
            var vipType = context.RoomTypes.First(t => t.TypeName == "Phòng Suite VIP").Id;

            var availStatus = context.RoomStatuses.First(s => s.StatusCode == "available").Id;
            var occupiedStatus = context.RoomStatuses.First(s => s.StatusCode == "occupied").Id;
            var dirtyStatus = context.RoomStatuses.First(s => s.StatusCode == "dirty").Id;
            var reservedStatus = context.RoomStatuses.First(s => s.StatusCode == "reserved").Id;

            context.Rooms.AddRange(
                new Room { RoomNumber = "101", RoomTypeId = singleType, RoomStatusId = availStatus, Price = 350000 },
                new Room { RoomNumber = "102", RoomTypeId = singleType, RoomStatusId = occupiedStatus, Price = 350000, CustomerName = "Nguyễn Văn A", CustomerPhone = "0901234567", CheckInDate = DateTime.Now.AddDays(-1) },
                new Room { RoomNumber = "103", RoomTypeId = doubleType, RoomStatusId = availStatus, Price = 500000 },
                new Room { RoomNumber = "104", RoomTypeId = doubleType, RoomStatusId = dirtyStatus, Price = 500000 },
                new Room { RoomNumber = "201", RoomTypeId = deluxeType, RoomStatusId = occupiedStatus, Price = 800000, CustomerName = "Trần Thị B", CustomerPhone = "0988776655", CheckInDate = DateTime.Now.AddDays(-2) },
                new Room { RoomNumber = "202", RoomTypeId = deluxeType, RoomStatusId = availStatus, Price = 800000 },
                new Room { RoomNumber = "203", RoomTypeId = vipType, RoomStatusId = reservedStatus, Price = 1500000, CustomerName = "Lê Văn C", CustomerPhone = "0912345678" },
                new Room { RoomNumber = "204", RoomTypeId = vipType, RoomStatusId = availStatus, Price = 1500000 }
            );
            await context.SaveChangesAsync();
            Logger.LogInfo("Seeded Normalized Rooms.");
        }
    }
    catch (Exception ex)
    {
        Logger.LogError("An error occurred during database migration or seeding.", ex);
    }
}

// 6. CONFIGURE REQUEST PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseMiddleware<ExceptionHandler>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
