using System.ComponentModel.DataAnnotations;

namespace BuiDinhHieu_2380600640.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email của bạn.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ. Ví dụ: nguyenvanw@example.com")]
        [Display(Name = "Email đăng ký")]
        public string Email { get; set; }
    }
}