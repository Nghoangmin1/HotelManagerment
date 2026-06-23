using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Receptionist.Models
{
    public class CheckInModel
    {
        [Required(ErrorMessage = "Vui lòng chọn phòng!")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng!")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại khách hàng!")]
        public string CustomerPhone { get; set; } = string.Empty;

        public DateTime CheckInDate { get; set; } = DateTime.Now;
    }
}
