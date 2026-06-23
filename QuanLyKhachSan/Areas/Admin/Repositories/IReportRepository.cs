using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public interface IReportRepository
    {
        Task<List<RevenueReportModel>> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
        Task<List<BookingReportModel>> GetBookingReportAsync(DateTime startDate, DateTime endDate);
    }
}
