using System;

namespace HotelManagement.Shared
{
    public static class ReceptionistPaymentLogger
    {
        public static void LogCashPaymentReceived(string receptionist, string invoiceNumber, decimal amount)
        {
            Logger.LogInfo($"[RECEPTIONIST PAYMENT] {receptionist} received cash payment of {amount:N0}đ for Invoice '{invoiceNumber}'");
        }

        public static void LogCheckoutBillingIssued(string receptionist, string roomNumber, string customerName, decimal totalAmount)
        {
            Logger.LogInfo($"[RECEPTIONIST CHECKOUT] {receptionist} generated checkout billing statement for customer '{customerName}' in Room P.{roomNumber}. Amount: {totalAmount:N0}đ");
        }
    }
}
