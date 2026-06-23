using System;

namespace HotelManagement.Areas.Receptionist.Models
{
    public class RoomStatusModel
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty; // available, occupied, dirty, reserved
        public string StatusName { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime? CheckInDate { get; set; }
    }
}
