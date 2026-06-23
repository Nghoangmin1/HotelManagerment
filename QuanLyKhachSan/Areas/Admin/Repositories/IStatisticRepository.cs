using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public interface IStatisticRepository
    {
        Task<List<RoomStatisticModel>> GetRoomStatisticsAsync();
        Task<int> GetTotalCustomersAsync();
        Task<int> GetTotalRoomsAsync();
        Task<decimal> GetTotalRevenueAsync();
    }
}
