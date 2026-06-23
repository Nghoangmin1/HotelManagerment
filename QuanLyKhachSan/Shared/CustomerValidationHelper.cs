using System;
using System.Text.RegularExpressions;

namespace HotelManagement.Shared
{
    public static class CustomerValidationHelper
    {
        // Supports 9-digit / 12-digit CMND/CCCD, or 5-20 characters passport format
        private static readonly Regex IdentityCardRegex = new Regex(@"^[0-9]{9,12}$|^[A-Z0-9\-]{5,20}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return false;
            var trimmed = fullName.Trim();
            return trimmed.Length >= 2 && trimmed.Length <= 100;
        }

        public static bool IsValidIdentityCard(string identityCard)
        {
            if (string.IsNullOrWhiteSpace(identityCard)) return false;
            return IdentityCardRegex.IsMatch(identityCard.Trim());
        }

        public static bool IsValidEmail(string email)
        {
            return ValidationHelper.IsValidEmail(email);
        }

        public static bool IsValidPhone(string phone)
        {
            return ValidationHelper.IsValidPhone(phone);
        }
    }
}
