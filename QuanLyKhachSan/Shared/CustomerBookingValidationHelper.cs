using System;

namespace HotelManagement.Shared
{
    public static class CustomerBookingValidationHelper
    {
        public static bool IsValidBookingDates(DateTime checkIn, DateTime? checkOut, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            // Check-in date cannot be in the past
            if (checkIn.Date < DateTime.Today)
            {
                errorMessage = "Ngày nhận phòng không được ở quá khứ!";
                return false;
            }

            // Check-out date validation
            if (checkOut.HasValue)
            {
                if (checkOut.Value.Date < checkIn.Date)
                {
                    errorMessage = "Ngày trả phòng phải sau ngày nhận phòng!";
                    return false;
                }
                if (checkOut.Value.Date == checkIn.Date)
                {
                    errorMessage = "Thời gian lưu trú tối thiểu là 1 ngày!";
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidStayDuration(DateTime checkIn, DateTime? checkOut, out int days)
        {
            days = 1;
            if (checkOut.HasValue)
            {
                days = (checkOut.Value.Date - checkIn.Date).Days;
                return days >= 1;
            }
            return true;
        }
    }
}
