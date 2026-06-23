using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly ApplicationDbContext _context;

        public StatisticRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RoomStatisticModel>> GetRoomStatisticsAsync()
        {
            // Configure Room Statistic Query
            var totalRooms = await _context.Rooms.CountAsync();
            if (totalRooms == 0) return new List<RoomStatisticModel>();

            var stats = await _context.Rooms
                .Include(r => r.RoomStatus)
                .GroupBy(r => new { r.RoomStatus.StatusCode, r.RoomStatus.StatusName })
                .Select(g => new RoomStatisticModel
                {
                    StatusCode = g.Key.StatusCode,
                    StatusName = g.Key.StatusName,
                    RoomCount = g.Count(),
                    Percentage = Math.Round((double)g.Count() / totalRooms * 100, 2)
                })
                .ToListAsync();

            return stats;
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _context.Customers.CountAsync();
        }

        public async Task<int> GetTotalRoomsAsync()
        {
            return await _context.Rooms.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Invoices
                .Where(i => i.Status == "Paid")
                .SumAsync(i => i.TotalAmount);
        }
    }
}
