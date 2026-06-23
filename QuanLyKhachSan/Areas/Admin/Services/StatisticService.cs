using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Admin.Repositories;

namespace HotelManagement.Areas.Admin.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IStatisticRepository _statisticRepository;

        public StatisticService(IStatisticRepository statisticRepository)
        {
            _statisticRepository = statisticRepository;
        }

        public async Task<List<RoomStatisticModel>> GetRoomStatisticsAsync()
        {
            return await _statisticRepository.GetRoomStatisticsAsync();
        }

        public async Task<int> GetTotalCustomersAsync()
        {
            return await _statisticRepository.GetTotalCustomersAsync();
        }

        public async Task<int> GetTotalRoomsAsync()
        {
            return await _statisticRepository.GetTotalRoomsAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _statisticRepository.GetTotalRevenueAsync();
        }
    }
}
