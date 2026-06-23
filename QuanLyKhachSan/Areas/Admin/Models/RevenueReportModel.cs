using System;
using System.ComponentModel;

namespace HotelManagement.Areas.Admin.Models
{
    public class RevenueReportModel
    {
        [DisplayName("Ngày / Tháng")]
        public string DateLabel { get; set; } = string.Empty;

        [DisplayName("Doanh thu Tiền phòng")]
        public decimal RoomRevenue { get; set; }

        [DisplayName("Doanh thu Dịch vụ")]
        public decimal ServiceRevenue { get; set; }

        [DisplayName("Tổng Doanh thu")]
        public decimal TotalRevenue { get; set; }
    }
}
