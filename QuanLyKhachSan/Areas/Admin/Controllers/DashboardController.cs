using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelManagement.Areas.Admin.Services;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class DashboardController : Controller
    {
        private readonly IStatisticService _statisticService;

        public DashboardController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        public async Task<IActionResult> Index()
        {
            ReportLogger.LogDashboardAccessed(User.Identity?.Name ?? "Unknown");

            ViewBag.TotalCustomers = await _statisticService.GetTotalCustomersAsync();
            ViewBag.TotalRooms = await _statisticService.GetTotalRoomsAsync();
            ViewBag.TotalRevenue = await _statisticService.GetTotalRevenueAsync();
            
            var roomStats = await _statisticService.GetRoomStatisticsAsync();
            ViewBag.RoomStatsJson = ChartHelper.SerializeDataForChart(roomStats);

            return View(roomStats);
        }
    }
}
