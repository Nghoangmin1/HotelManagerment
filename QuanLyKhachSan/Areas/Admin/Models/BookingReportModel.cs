using System;
using System.ComponentModel;

namespace HotelManagement.Areas.Admin.Models
{
    public class BookingReportModel
    {
        [DisplayName("Ngày / Tháng")]
        public string DateLabel { get; set; } = string.Empty;

        [DisplayName("Tổng số Đặt phòng")]
        public int TotalBookings { get; set; }

        [DisplayName("Đã hoàn thành")]
        public int CompletedBookings { get; set; }

        [DisplayName("Đã hủy")]
        public int CancelledBookings { get; set; }

        [DisplayName("Tỷ lệ Hoàn thành (%)")]
        public double CompletionRate => TotalBookings > 0 ? Math.Round((double)CompletedBookings / TotalBookings * 100, 2) : 0;
    }
}
