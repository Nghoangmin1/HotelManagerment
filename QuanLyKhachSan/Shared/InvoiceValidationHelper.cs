using System;

namespace HotelManagement.Shared
{
    public static class InvoiceValidationHelper
    {
        public static bool IsValidInvoice(decimal totalAmount, decimal discount, decimal tax, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (totalAmount < 0)
            {
                errorMessage = "Tổng số tiền hóa đơn không được là số âm!";
                return false;
            }

            if (discount < 0)
            {
                errorMessage = "Số tiền giảm giá không được là số âm!";
                return false;
            }

            if (tax < 0)
            {
                errorMessage = "Thuế VAT không được là số âm!";
                return false;
            }

            if (discount > totalAmount)
            {
                errorMessage = "Tiền giảm giá không được vượt quá tổng giá trị hóa đơn!";
                return false;
            }

            return true;
        }
    }
}
