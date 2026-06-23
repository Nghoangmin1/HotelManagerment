using System;

namespace HotelManagement.Shared
{
    public static class BookingValidationHelper
    {
        public static bool IsValidBookingDates(DateTime checkInDate, DateTime? checkOutDate)
        {
            if (checkOutDate.HasValue)
            {
                // Check-out date must be on or after check-in date
                return checkOutDate.Value >= checkInDate;
            }
            return true;
        }

        public static bool IsValidPrice(decimal price)
        {
            return price >= 0;
        }

        public static bool IsValidStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            var s = status.Trim().ToLower();
            return s == "pending" || s == "confirmed" || s == "checkedin" || s == "checkedout" || s == "cancelled";
        }
    }
}
