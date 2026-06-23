using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using HotelManagement.Areas.Admin.Models;
using HotelManagement.Areas.Receptionist.Repositories;
using HotelManagement.Shared;

namespace HotelManagement.Areas.Receptionist.Services
{
    public interface IPaymentService
    {
        Task<Invoice?> GetInvoiceByBookingIdAsync(int bookingId);
        Task<Invoice> CreateOrUpdateInvoicePreviewAsync(int bookingId, decimal discount, decimal tax);
        Task<bool> ProcessReceptionistPaymentAsync(int bookingId, decimal amount, string method, decimal discount, decimal tax, string receptionist);
        Task<IEnumerable<Payment>> GetBookingPaymentsAsync(int bookingId);
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
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
            return await _paymentRepository.GetInvoiceByBookingIdAsync(bookingId);
        }

        public async Task<Invoice> CreateOrUpdateInvoicePreviewAsync(int bookingId, decimal discount, decimal tax)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.BookingDetails)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                throw new InvalidOperationException("Không tìm thấy thông tin đặt phòng!");
            }

            // Calculate room charges
            int days = 1;
            var checkoutDate = booking.CheckOutDate ?? DateTime.UtcNow;
            days = (checkoutDate.Date - booking.CheckInDate.Date).Days;
            if (days < 1) days = 1;
            decimal roomCost = (booking.Room?.Price ?? booking.TotalPrice) * days;

            // Calculate extra services (BookingDetails + ServiceUsages)
            decimal serviceCost = booking.BookingDetails.Sum(d => d.ServicePrice * d.Quantity);
            
            var serviceUsages = await _context.ServiceUsages
                .Include(su => su.Service)
                .Where(su => su.BookingId == bookingId)
                .ToListAsync();
            decimal serviceUsagesCost = serviceUsages.Sum(su => su.Quantity * (su.Service?.Price ?? 0));

            decimal subTotal = roomCost + serviceCost + serviceUsagesCost;

            if (!InvoiceValidationHelper.IsValidInvoice(subTotal, discount, tax, out string error))
            {
                throw new ArgumentException(error);
            }

            decimal totalAmount = subTotal - discount + tax;
            if (totalAmount < 0) totalAmount = 0;

            var invoice = await _paymentRepository.GetInvoiceByBookingIdAsync(bookingId);
            if (invoice == null)
            {
                invoice = new Invoice
                {
                    BookingId = bookingId,
                    InvoiceNumber = "INV-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + bookingId,
                    TotalAmount = totalAmount,
                    Discount = discount,
                    Tax = tax,
                    IssueDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(1),
                    Status = "Unpaid"
                };
                await _paymentRepository.AddInvoiceAsync(invoice);
            }
            else
            {
                invoice.TotalAmount = totalAmount;
                invoice.Discount = discount;
                invoice.Tax = tax;
                await _paymentRepository.UpdateInvoiceAsync(invoice);
            }

            return invoice;
        }

        public async Task<bool> ProcessReceptionistPaymentAsync(int bookingId, decimal amount, string method, decimal discount, decimal tax, string receptionist)
        {
            if (!PaymentValidationHelper.IsValidPayment(amount, method, out string payError))
            {
                throw new ArgumentException(payError);
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                throw new InvalidOperationException("Không tìm thấy thông tin đặt phòng!");
            }

            var invoice = await CreateOrUpdateInvoicePreviewAsync(bookingId, discount, tax);

            // Record payment
            var txId = "TXN-REC-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = method,
                Status = "Completed",
                TransactionId = txId
            };

            var paymentResult = await _paymentRepository.AddPaymentAsync(payment);
            if (!paymentResult) return false;

            if (method.Equals("cash", StringComparison.OrdinalIgnoreCase))
            {
                ReceptionistPaymentLogger.LogCashPaymentReceived(receptionist, invoice.InvoiceNumber, amount);
            }
            else
            {
                PaymentLogger.LogPaymentProcessed(bookingId, amount, method, "Completed", txId, receptionist);
            }

            // Update Invoice status
            invoice.PaymentId = payment.Id;
            invoice.Status = "Paid";
            await _paymentRepository.UpdateInvoiceAsync(invoice);
            PaymentLogger.LogInvoiceIssued(invoice.InvoiceNumber, bookingId, amount, receptionist);

            // Update Booking and Room statuses
            booking.Status = "CheckedOut";
            booking.CheckOutDate = DateTime.UtcNow;
            _context.Bookings.Update(booking);

            if (booking.Room != null)
            {
                var dirtyStatus = await _context.RoomStatuses.FirstOrDefaultAsync(s => s.StatusCode == "dirty");
                if (dirtyStatus != null)
                {
                    booking.Room.RoomStatusId = dirtyStatus.Id;
                    booking.Room.CustomerName = string.Empty;
                    booking.Room.CustomerPhone = string.Empty;
                    booking.Room.CheckInDate = null;
                    _context.Rooms.Update(booking.Room);
                }
            }

            await _context.SaveChangesAsync();
            ReceptionistPaymentLogger.LogCheckoutBillingIssued(receptionist, booking.Room?.RoomNumber ?? "N/A", booking.Customer?.FullName ?? "Khách lẻ", invoice.TotalAmount);

            return true;
        }

        public async Task<IEnumerable<Payment>> GetBookingPaymentsAsync(int bookingId)
        {
            return await _paymentRepository.GetBookingPaymentsAsync(bookingId);
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _paymentRepository.GetAllInvoicesAsync();
        }
    }
}
