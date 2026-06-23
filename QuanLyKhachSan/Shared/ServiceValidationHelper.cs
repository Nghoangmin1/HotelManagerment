using System;

namespace HotelManagement.Shared
{
    public static class ServiceValidationHelper
    {
        public static bool IsValidService(string serviceName, decimal price, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                errorMessage = "Tên dịch vụ không được để trống!";
                return false;
            }

            if (serviceName.Trim().Length < 3 || serviceName.Trim().Length > 100)
            {
                errorMessage = "Tên dịch vụ phải từ 3 đến 100 ký tự!";
                return false;
            }

            if (price < 0)
            {
                errorMessage = "Giá dịch vụ không được là số âm!";
                return false;
            }

            return true;
        }
    }
}
