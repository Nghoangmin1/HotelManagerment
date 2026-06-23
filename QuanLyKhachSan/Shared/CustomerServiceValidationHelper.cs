using System;

namespace HotelManagement.Shared
{
    public static class CustomerServiceValidationHelper
    {
        public static bool IsValidServiceOrder(int quantity, string notes, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (quantity <= 0)
            {
                errorMessage = "Số lượng yêu cầu phải lớn hơn 0!";
                return false;
            }

            if (quantity > 20)
            {
                errorMessage = "Số lượng đặt dịch vụ trong một lần vượt quá giới hạn cho phép (tối đa 20)!";
                return false;
            }

            if (notes != null && notes.Length > 500)
            {
                errorMessage = "Ghi chú không được vượt quá 500 ký tự!";
                return false;
            }

            return true;
        }
    }
}
