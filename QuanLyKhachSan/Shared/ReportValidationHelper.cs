using System;

namespace HotelManagement.Shared
{
    public static class ReportValidationHelper
    {
        public static bool IsValidDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return startDate.Value <= endDate.Value;
            }
            return true;
        }

        public static bool IsValidMonthYear(int month, int year)
        {
            return month >= 1 && month <= 12 && year >= 2000 && year <= DateTime.Now.Year + 1;
        }
    }
}
