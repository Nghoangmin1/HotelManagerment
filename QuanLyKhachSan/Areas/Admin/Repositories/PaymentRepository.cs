using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;

namespace HotelManagement.Areas.Admin.Repositories
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<bool> AddPaymentAsync(Payment payment);
        
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<Invoice?> GetInvoiceByIdAsync(int id);
        Task<Invoice?> GetInvoiceByBookingIdAsync(int bookingId);
        Task<bool> AddInvoiceAsync(Invoice invoice);
        Task<bool> UpdateInvoiceAsync(Invoice invoice);
    }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Room)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Room)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.Room)
                .Include(i => i.Payment)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(i => i.Booking)
                    .ThenInclude(b => b!.Room)
                .Include(i => i.Payment)
                .FirstOrDefaultAsync(i => i.Id == id);
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

        public async Task<bool> AddInvoiceAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
