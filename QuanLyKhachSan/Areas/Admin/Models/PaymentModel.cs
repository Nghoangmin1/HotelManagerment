using System;

namespace HotelManagement.Areas.Admin.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string PaymentMethod { get; set; } = "Cash"; // Cash, CreditCard, BankTransfer, Online
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
        public string TransactionId { get; set; } = string.Empty;
    }
}
