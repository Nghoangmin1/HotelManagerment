using System;

namespace HotelManagement.Areas.Admin.Models
{
    public class ServiceUsage
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public int ServiceId { get; set; }
        public Service? Service { get; set; }
        public int Quantity { get; set; }
        public DateTime UsageDate { get; set; } = DateTime.UtcNow;
        public string Notes { get; set; } = string.Empty;
    }
}
