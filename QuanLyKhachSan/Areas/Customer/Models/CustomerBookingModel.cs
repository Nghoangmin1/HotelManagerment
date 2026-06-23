using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Customer.Models
{
    public class CustomerBookingModel
    {
        [Required(ErrorMessage = "Vui lòng chọn loại phòng!")]
        public int RoomTypeId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng!")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng!")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);

        public decimal EstimatedPrice { get; set; }
        
        public string? Notes { get; set; }
    }
}
