using System;
using System.Text.RegularExpressions;

namespace HotelManagement.Shared
{
    public static class CustomerAccountValidationHelper
    {
        private static readonly Regex UsernameRegex = new Regex(@"^[a-zA-Z0-9_]{4,30}$", RegexOptions.Compiled);

        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return UsernameRegex.IsMatch(username);
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 6) return false;
            
            bool hasDigit = false;
            bool hasUpper = false;
            bool hasLower = false;

            foreach (char c in password)
            {
                if (char.IsDigit(c)) hasDigit = true;
                else if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
            }

            return hasDigit && hasUpper && hasLower;
        }

        public static bool ArePasswordsMatching(string password, string confirmPassword)
        {
            return password == confirmPassword;
        }
    }
}
