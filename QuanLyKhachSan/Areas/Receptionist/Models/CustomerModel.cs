using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Receptionist.Models
{
    public class CustomerModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống!")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên từ 2 đến 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống!")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống!")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "CMND/CCCD không được để trống!")]
        public string IdentityCard { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ không được để trống!")]
        public string Address { get; set; } = string.Empty;
    }
}
