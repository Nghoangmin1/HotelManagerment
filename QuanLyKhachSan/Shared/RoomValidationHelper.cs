using System;
using System.Text.RegularExpressions;

namespace HotelManagement.Shared
{
    public static class RoomValidationHelper
    {
        private static readonly Regex RoomNumberRegex = new Regex(@"^[0-9A-Za-z\-]{2,10}$", RegexOptions.Compiled);

        public static bool IsValidRoomNumber(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber)) return false;
            return RoomNumberRegex.IsMatch(roomNumber.Trim());
        }

        public static bool IsValidPrice(decimal price)
        {
            return price >= 0;
        }
    }
}
