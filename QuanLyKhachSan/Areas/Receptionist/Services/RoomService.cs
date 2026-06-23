using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.Areas.Receptionist.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Receptionist.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(int id);
        Task<Room?> GetRoomByRoomNumberAsync(string roomNumber);
        Task<bool> UpdateRoomStatusAsync(int roomId, string statusCode, string user);
    }

    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;
        private readonly ApplicationDbContext _context;

        public RoomService(IRoomRepository roomRepository, ApplicationDbContext context)
        {
            _roomRepository = roomRepository;
            _context = context;
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        public async Task<Room?> GetRoomByRoomNumberAsync(string roomNumber)
        {
            return await _roomRepository.GetByRoomNumberAsync(roomNumber);
        }

        public async Task<bool> UpdateRoomStatusAsync(int roomId, string statusCode, string user)
        {
            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room == null) return false;

            var newStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == statusCode);
            if (newStatus == null)
            {
                throw new ArgumentException($"Mã trạng thái phòng '{statusCode}' không tồn tại!");
            }

            string oldStatusName = room.RoomStatus?.StatusName ?? "Không rõ";
            room.RoomStatusId = newStatus.Id;

            // Clear details if marked as available
            if (statusCode == "available")
            {
                room.CustomerName = null;
                room.CustomerPhone = null;
                room.CheckInDate = null;
            }

            var result = await _roomRepository.UpdateAsync(room);
            if (result)
            {
                ReceptionistLogger.LogRoomStatusUpdated(user, room.RoomNumber, oldStatusName, newStatus.StatusName);
            }
            return result;
        }
    }
}
