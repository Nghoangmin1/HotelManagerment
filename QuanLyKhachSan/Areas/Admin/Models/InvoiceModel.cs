using System;

namespace HotelManagement.Areas.Admin.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public Booking? Booking { get; set; }
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; } = 0;
        public decimal Tax { get; set; } = 0;
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(3);
        public string Status { get; set; } = "Unpaid"; // Unpaid, Paid, Cancelled
    }
}
