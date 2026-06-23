using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelManagement.Areas.Admin.Services;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RevenueReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = endDate ?? DateTime.Now;

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            try
            {
                var report = await _reportService.GenerateRevenueReportAsync(start, end, User.Identity?.Name ?? "Unknown");
                return View(report);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new List<Models.RevenueReportModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportRevenueReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var report = await _reportService.GenerateRevenueReportAsync(startDate, endDate, User.Identity?.Name ?? "Unknown");
                var csvBytes = _reportService.ExportRevenueReportToCsv(report, User.Identity?.Name ?? "Unknown");
                return File(csvBytes, "text/csv", $"RevenueReport_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(RevenueReport));
            }
        }

        [HttpGet]
        public async Task<IActionResult> BookingReport(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = endDate ?? DateTime.Now;

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            try
            {
                var report = await _reportService.GenerateBookingReportAsync(start, end, User.Identity?.Name ?? "Unknown");
                return View(report);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new List<Models.BookingReportModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportBookingReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var report = await _reportService.GenerateBookingReportAsync(startDate, endDate, User.Identity?.Name ?? "Unknown");
                var csvBytes = _reportService.ExportBookingReportToCsv(report, User.Identity?.Name ?? "Unknown");
                return File(csvBytes, "text/csv", $"BookingReport_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(BookingReport));
            }
        }

        [HttpGet]
        public async Task<IActionResult> RoomStatusReport([FromServices] IStatisticService statisticService)
        {
            try
            {
                var report = await statisticService.GetRoomStatisticsAsync();
                ReportLogger.LogReportGenerated("Room Status Report", User.Identity?.Name ?? "Unknown");
                return View(report);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new List<Models.RoomStatisticModel>());
            }
        }
    }
}
