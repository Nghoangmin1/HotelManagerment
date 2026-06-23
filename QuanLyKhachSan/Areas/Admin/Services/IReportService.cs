using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Admin.Services
{
    public interface IReportService
    {
        Task<List<RevenueReportModel>> GenerateRevenueReportAsync(DateTime startDate, DateTime endDate, string userName);
        Task<List<BookingReportModel>> GenerateBookingReportAsync(DateTime startDate, DateTime endDate, string userName);
        byte[] ExportRevenueReportToCsv(IEnumerable<RevenueReportModel> data, string userName);
        byte[] ExportBookingReportToCsv(IEnumerable<BookingReportModel> data, string userName);
    }
}
