using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Admin.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Admin.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<RevenueReportModel>> GenerateRevenueReportAsync(DateTime startDate, DateTime endDate, string userName)
        {
            if (!ReportValidationHelper.IsValidDateRange(startDate, endDate))
            {
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            var report = await _reportRepository.GetRevenueReportAsync(startDate, endDate);
            ReportLogger.LogReportGenerated("Revenue Report", userName, startDate, endDate);
            return report;
        }

        public async Task<List<BookingReportModel>> GenerateBookingReportAsync(DateTime startDate, DateTime endDate, string userName)
        {
            if (!ReportValidationHelper.IsValidDateRange(startDate, endDate))
            {
                throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            var report = await _reportRepository.GetBookingReportAsync(startDate, endDate);
            ReportLogger.LogReportGenerated("Booking Report", userName, startDate, endDate);
            return report;
        }

        public byte[] ExportRevenueReportToCsv(IEnumerable<RevenueReportModel> data, string userName)
        {
            ReportLogger.LogReportExported("Revenue Report", "CSV", userName);
            return ReportExportHelper.ExportToCsv(data);
        }

        public byte[] ExportBookingReportToCsv(IEnumerable<BookingReportModel> data, string userName)
        {
            ReportLogger.LogReportExported("Booking Report", "CSV", userName);
            return ReportExportHelper.ExportToCsv(data);
        }
    }
}
