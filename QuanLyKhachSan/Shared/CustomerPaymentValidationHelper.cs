using System;
using System.Text.RegularExpressions;

namespace HotelManagement.Shared
{
    public static class CustomerPaymentValidationHelper
    {
        private static readonly Regex CardRegex = new Regex(@"^\d{16}$", RegexOptions.Compiled);

        public static bool IsValidOnlinePayment(string cardNumber, string cardHolder, string expiryDate, string cvv, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(cardNumber) || !CardRegex.IsMatch(cardNumber.Trim()))
            {
                errorMessage = "Số thẻ thanh toán không hợp lệ (phải gồm 16 chữ số)!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(cardHolder) || cardHolder.Trim().Length < 3)
            {
                errorMessage = "Tên chủ thẻ không hợp lệ (tối thiểu 3 ký tự)!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(expiryDate) || !expiryDate.Contains("/") || expiryDate.Trim().Length != 5)
            {
                errorMessage = "Hạn dùng thẻ không đúng định dạng (MM/YY)!";
                return false;
            }

            if (string.IsNullOrWhiteSpace(cvv) || cvv.Trim().Length != 3 || !int.TryParse(cvv.Trim(), out _))
            {
                errorMessage = "Mã CVV phải gồm đúng 3 chữ số!";
                return false;
            }

            return true;
        }
    }
}
