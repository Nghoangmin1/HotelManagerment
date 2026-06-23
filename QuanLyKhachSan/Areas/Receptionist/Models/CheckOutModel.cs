using System;

namespace HotelManagement.Areas.Receptionist.Models
{
    public class CheckOutModel
    {
        public int BookingId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; } = DateTime.Now;
        public decimal RoomCharge { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalCharge { get; set; }
    }
}
