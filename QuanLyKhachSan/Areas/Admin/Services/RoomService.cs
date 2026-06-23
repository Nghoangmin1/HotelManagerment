using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Models;
using HotelManagement.Areas.Admin.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Admin.Services
{
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(int id);
        Task<Room?> GetRoomByNumberAsync(string roomNumber);
        Task<bool> CreateRoomAsync(Room room, string user);
        Task<bool> UpdateRoomAsync(Room room, string user);
        Task<bool> DeleteRoomAsync(int id, string user);
    }

    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _roomRepository;

        public RoomService(IRoomRepository roomRepository)
        {
            _roomRepository = roomRepository;
        }

        public async Task<IEnumerable<Room>> GetAllRoomsAsync()
        {
            return await _roomRepository.GetAllAsync();
        }

        public async Task<Room?> GetRoomByIdAsync(int id)
        {
            return await _roomRepository.GetByIdAsync(id);
        }

        public async Task<Room?> GetRoomByNumberAsync(string roomNumber)
        {
            return await _roomRepository.GetByRoomNumberAsync(roomNumber);
        }

        public async Task<bool> CreateRoomAsync(Room room, string user)
        {
            if (room == null) return false;
            
            // Validate room number
            if (!RoomValidationHelper.IsValidRoomNumber(room.RoomNumber))
            {
                throw new ArgumentException("Số phòng không hợp lệ! Chỉ được phép chứa chữ và số từ 2-10 ký tự.");
            }

            // Validate price
            if (!RoomValidationHelper.IsValidPrice(room.Price))
            {
                throw new ArgumentException("Giá phòng phải lớn hơn hoặc bằng 0!");
            }

            // Ensure no duplicate room number
            var existing = await _roomRepository.GetByRoomNumberAsync(room.RoomNumber);
            if (existing != null)
            {
                throw new InvalidOperationException($"Số phòng {room.RoomNumber} đã tồn tại trong hệ thống!");
            }

            var result = await _roomRepository.AddAsync(room);
            if (result)
            {
                RoomLogger.LogRoomCreated(room.RoomNumber, user);
            }
            return result;
        }

        public async Task<bool> UpdateRoomAsync(Room room, string user)
        {
            if (room == null) return false;

            if (!RoomValidationHelper.IsValidRoomNumber(room.RoomNumber))
            {
                throw new ArgumentException("Số phòng không hợp lệ!");
            }

            if (!RoomValidationHelper.IsValidPrice(room.Price))
            {
                throw new ArgumentException("Giá phòng không hợp lệ!");
            }

            // Ensure no duplicate room number if changed
            var existing = await _roomRepository.GetByRoomNumberAsync(room.RoomNumber);
            if (existing != null && existing.Id != room.Id)
            {
                throw new InvalidOperationException($"Số phòng {room.RoomNumber} đã được sử dụng bởi một phòng khác!");
            }

            var result = await _roomRepository.UpdateAsync(room);
            if (result)
            {
                RoomLogger.LogRoomUpdated(room.RoomNumber, user, $"Type ID: {room.RoomTypeId}, Status ID: {room.RoomStatusId}, Price: {room.Price}");
            }
            return result;
        }

        public async Task<bool> DeleteRoomAsync(int id, string user)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return false;

            // Chỉ cho phép xóa phòng có trạng thái "Còn trống"
            if (room.RoomStatus?.StatusCode != "available")
            {
                throw new InvalidOperationException(
                    $"Không thể xóa phòng P.{room.RoomNumber} vì phòng đang ở trạng thái \"{room.RoomStatus?.StatusName}\". Chỉ có thể xóa phòng đang trống!");
            }

            var result = await _roomRepository.DeleteAsync(id);
            if (result)
            {
                RoomLogger.LogRoomDeleted(room.RoomNumber, user);
            }
            return result;
        }
    }
}
