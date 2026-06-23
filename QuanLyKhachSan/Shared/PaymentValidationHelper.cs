using System;

namespace HotelManagement.Shared
{
    public static class PaymentValidationHelper
    {
        public static bool IsValidPayment(decimal amount, string method, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (amount <= 0)
            {
                errorMessage = "Số tiền thanh toán phải lớn hơn 0!";
                return false;
            }

            string trimmedMethod = method?.Trim().ToLower() ?? "";
            if (trimmedMethod != "cash" && trimmedMethod != "creditcard" && trimmedMethod != "banktransfer" && trimmedMethod != "online")
            {
                errorMessage = "Phương thức thanh toán không hợp lệ! Vui lòng chọn Cash, CreditCard, BankTransfer, hoặc Online.";
                return false;
            }

            return true;
        }
    }
}
