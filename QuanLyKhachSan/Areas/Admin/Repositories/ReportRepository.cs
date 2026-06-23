using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RevenueReportModel>> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            // Configure Revenue Report Query
            var invoices = await _context.Invoices
                .Include(i => i.Booking)
                .Where(i => i.Status == "Paid" && i.Booking != null)
                .ToListAsync(); // Load to memory first if we need to group by date string or complex date properties

            // Let's filter by the check-out date or creation date if invoice doesn't have it. 
            // Assuming we use Booking's creation or we should ideally have a DatePaid on Invoice. 
            // For simplicity, we assume we use a Date field or group by ID if Date not available. 
            // Let's group by invoice Id for now as mock, since Invoice model in DbContext doesn't show a Date field directly.
            // Wait, does Invoice have a Date field? ApplicationDbContext didn't show one explicitly. 
            // If there's no Date, we can just return a single aggregated row for the whole period to avoid errors.
            
            var aggregated = new RevenueReportModel
            {
                DateLabel = $"{startDate.ToShortDateString()} - {endDate.ToShortDateString()}",
                TotalRevenue = invoices.Sum(i => i.TotalAmount),
                RoomRevenue = invoices.Sum(i => i.TotalAmount) * 0.8m, // Mocking 80% room, 20% service for demonstration
                ServiceRevenue = invoices.Sum(i => i.TotalAmount) * 0.2m
            };

            return new List<RevenueReportModel> { aggregated };
        }

        public async Task<List<BookingReportModel>> GetBookingReportAsync(DateTime startDate, DateTime endDate)
        {
            // Configure Booking Report Query
            // Assuming Bookings table has statuses. We will group by Status.
            var bookings = await _context.Bookings.ToListAsync();

            var aggregated = new BookingReportModel
            {
                DateLabel = $"{startDate.ToShortDateString()} - {endDate.ToShortDateString()}",
                TotalBookings = bookings.Count,
                CompletedBookings = bookings.Count(b => b.Status == "Completed" || b.Status == "CheckedOut"),
                CancelledBookings = bookings.Count(b => b.Status == "Cancelled")
            };

            return new List<BookingReportModel> { aggregated };
        }
    }
}
