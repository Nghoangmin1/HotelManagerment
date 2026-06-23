using System;

namespace HotelManagement.Areas.Admin.Models
{
    public class BookingDetail
    {
        public int Id { get; set; }
        
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        
        public string ServiceName { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public int Quantity { get; set; }
    }
}
