using System;

namespace HotelManagement.Shared
{
    public static class CheckInOutValidationHelper
    {
        public static bool CanCheckIn(DateTime checkInDate, out string error)
        {
            error = string.Empty;
            // Check-in should normally be today or within 1 day buffer
            if (checkInDate.Date < DateTime.Today.AddDays(-1))
            {
                error = "Không thể check-in cho ngày trong quá khứ!";
                return false;
            }
            if (checkInDate.Date > DateTime.Today.AddDays(1))
            {
                error = "Ngày nhận phòng quá xa so với thực tế!";
                return false;
            }
            return true;
        }

        public static bool CanCheckOut(DateTime checkInDate, DateTime checkOutDate, out string error)
        {
            error = string.Empty;
            if (checkOutDate.Date < checkInDate.Date)
            {
                error = "Ngày trả phòng không thể trước ngày nhận phòng!";
                return false;
            }
            return true;
        }
    }
}
