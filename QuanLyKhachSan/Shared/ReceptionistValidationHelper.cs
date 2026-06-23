using System;

namespace HotelManagement.Shared
{
    public static class ReceptionistValidationHelper
    {
        public static bool IsValidRoomNumber(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber)) return false;
            // Room numbers should be simple numeric formats like 101, 204, etc.
            return System.Text.RegularExpressions.Regex.IsMatch(roomNumber.Trim(), @"^[0-9]{3,4}$");
        }

        public static bool IsValidStatusDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description)) return false;
            return description.Trim().Length >= 2 && description.Trim().Length <= 200;
        }

        public static bool IsValidGuestInfo(string name, string phone, out string error)
        {
            error = string.Empty;
            if (!CustomerValidationHelper.IsValidFullName(name))
            {
                error = "Họ tên khách hàng không hợp lệ (từ 2 đến 100 ký tự)!";
                return false;
            }
            if (!CustomerValidationHelper.IsValidPhone(phone))
            {
                error = "Số điện thoại không đúng định dạng Việt Nam!";
                return false;
            }
            return true;
        }
    }
}
