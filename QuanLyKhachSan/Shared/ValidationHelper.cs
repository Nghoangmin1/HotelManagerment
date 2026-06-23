using System;
using System.Text.RegularExpressions;

namespace HotelManagement.Shared
{
    public static class ValidationHelper
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Chấp nhận số điện thoại Việt Nam: bắt đầu bằng 0, từ 10-11 chữ số
        private static readonly Regex PhoneRegex = new Regex(
            @"^0[0-9]{9,10}$", 
            RegexOptions.Compiled);

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return EmailRegex.IsMatch(email);
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return PhoneRegex.IsMatch(phone);
        }

        public static string CleanString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return input.Trim();
        }
    }
}
