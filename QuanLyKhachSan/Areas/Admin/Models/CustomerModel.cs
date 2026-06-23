using System;
using System.Collections.Generic;
using HotelManagement.Models;

namespace HotelManagement.Areas.Admin.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string IdentityCard { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for bookings
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
