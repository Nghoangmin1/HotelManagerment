using System;

namespace HotelManagement.Shared
{
    public static class PaymentLogger
    {
        public static void LogPaymentProcessed(int bookingId, decimal amount, string method, string status, string txId, string user)
        {
            Logger.LogInfo($"[PAYMENT] Booking #{bookingId}: Processed payment of {amount:N0}đ via {method}. Status: {status}. TxId: {txId}. User: {user}");
        }

        public static void LogInvoiceIssued(string invoiceNumber, int bookingId, decimal total, string user)
        {
            Logger.LogInfo($"[INVOICE] Booking #{bookingId}: Issued Invoice '{invoiceNumber}' for total amount of {total:N0}đ. User: {user}");
        }
    }
}
