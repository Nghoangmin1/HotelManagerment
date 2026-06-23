using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Customer.Models
{
    public class CustomerProfileModel
    {
        [Required(ErrorMessage = "Họ và tên không được để trống!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2 đến 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống!")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống!")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số CMND/CCCD/Hộ chiếu không được để trống!")]
        [StringLength(50, ErrorMessage = "Số CMND/CCCD không được vượt quá 50 ký tự.")]
        public string IdentityCard { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống!")]
        public string Address { get; set; } = string.Empty;
    }
}
