using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Customer.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Customer.Services
{
    public interface IPaymentService
    {
        Task<Invoice?> GetInvoiceByBookingIdAsync(int bookingId);
        Task<IEnumerable<Payment>> GetBookingPaymentsAsync(int bookingId);
        Task<bool> ProcessOnlinePaymentAsync(int bookingId, decimal amount, string cardNumber, string cardHolder, string expiryDate, string cvv, string username);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly ApplicationDbContext _context;

        public PaymentService(IPaymentRepository paymentRepository, ApplicationDbContext context)
        {
            _paymentRepository = paymentRepository;
            _context = context;
        }

        public async Task<Invoice?> GetInvoiceByBookingIdAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
            
            if (booking == null) return null;

            var invoice = await _paymentRepository.GetInvoiceByBookingIdAsync(bookingId);
            
            var serviceOrders = await _context.ServiceOrders
                .Include(so => so.Service)
                .Where(so => so.BookingId == bookingId && so.Status != "Cancelled")
                .ToListAsync();
            
            decimal serviceTotal = serviceOrders.Sum(so => so.Quantity * (so.Service?.Price ?? 0));
            decimal totalAmount = booking.TotalPrice + serviceTotal;

            if (invoice == null)
            {
                invoice = new Invoice
                {
                    BookingId = bookingId,
                    InvoiceNumber = "INV-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + bookingId,
                    TotalAmount = totalAmount,
                    Discount = 0,
                    Tax = totalAmount * 0.1m,
                    IssueDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(3),
                    Status = "Unpaid"
                };
                
                await _context.Invoices.AddAsync(invoice);
                await _context.SaveChangesAsync();
            }
            else
            {
                invoice.TotalAmount = totalAmount;
                invoice.Tax = totalAmount * 0.1m;
                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();
            }

            return invoice;
        }

        public async Task<IEnumerable<Payment>> GetBookingPaymentsAsync(int bookingId)
        {
            return await _paymentRepository.GetBookingPaymentsAsync(bookingId);
        }

        public async Task<bool> ProcessOnlinePaymentAsync(int bookingId, decimal amount, string cardNumber, string cardHolder, string expiryDate, string cvv, string username)
        {
            if (!CustomerPaymentValidationHelper.IsValidOnlinePayment(cardNumber, cardHolder, expiryDate, cvv, out string error))
            {
                throw new ArgumentException(error);
            }

            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Không tìm thấy thông tin đặt phòng!");
            }

            var txId = "TXN-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = "Online",
                Status = "Completed",
                TransactionId = txId
            };

            var paymentResult = await _paymentRepository.AddPaymentAsync(payment);
            if (!paymentResult) return false;

            PaymentLogger.LogPaymentProcessed(bookingId, amount, "Online", "Completed", txId, username);

            var invoice = await GetInvoiceByBookingIdAsync(bookingId);
            if (invoice != null)
            {
                invoice.PaymentId = payment.Id;
                invoice.Status = "Paid";
                await _paymentRepository.UpdateInvoiceAsync(invoice);
                PaymentLogger.LogInvoiceIssued(invoice.InvoiceNumber, bookingId, amount, username);
            }

            booking.Status = "Confirmed";
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
