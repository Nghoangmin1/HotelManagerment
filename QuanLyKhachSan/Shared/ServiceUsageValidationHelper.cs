using System;

namespace HotelManagement.Shared
{
    public static class ServiceUsageValidationHelper
    {
        public static bool IsValidUsage(int bookingId, int serviceId, int quantity, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (bookingId <= 0)
            {
                errorMessage = "Đặt phòng không hợp lệ!";
                return false;
            }

            if (serviceId <= 0)
            {
                errorMessage = "Dịch vụ không hợp lệ!";
                return false;
            }

            if (quantity <= 0)
            {
                errorMessage = "Số lượng dịch vụ sử dụng phải lớn hơn 0!";
                return false;
            }

            return true;
        }
    }
}
