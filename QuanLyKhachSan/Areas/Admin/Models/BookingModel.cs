using System;
using System.Collections.Generic;
using HotelManagement.Models;

namespace HotelManagement.Areas.Admin.Models
{
    public class Booking
    {
        public int Id { get; set; }
        
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        
        public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, CheckedIn, CheckedOut, Cancelled

        // Navigation property for details (e.g. services consumed)
        public ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    }
}
