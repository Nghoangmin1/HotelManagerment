using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Customer.Repositories
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetBookingPaymentsAsync(int bookingId);
        Task<Invoice?> GetInvoiceByBookingIdAsync(int bookingId);
        Task<bool> AddPaymentAsync(Payment payment);
        Task<bool> UpdateInvoiceAsync(Invoice invoice);
    }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetBookingPaymentsAsync(int bookingId)
        {
            return await _context.Payments
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceByBookingIdAsync(int bookingId)
        {
            return await _context.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.Room)
                .Include(i => i.Payment)
                .FirstOrDefaultAsync(i => i.BookingId == bookingId);
        }

        public async Task<bool> AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
