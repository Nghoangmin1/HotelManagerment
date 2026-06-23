using System.ComponentModel;

namespace HotelManagement.Areas.Admin.Models
{
    public class RoomStatisticModel
    {
        [DisplayName("Trạng thái")]
        public string StatusName { get; set; } = string.Empty;

        [DisplayName("Mã trạng thái")]
        public string StatusCode { get; set; } = string.Empty;

        [DisplayName("Số lượng phòng")]
        public int RoomCount { get; set; }

        [DisplayName("Tỷ lệ (%)")]
        public double Percentage { get; set; }
    }
}
