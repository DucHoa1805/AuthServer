using System.ComponentModel.DataAnnotations;

namespace AuthServer.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username không được để trống")]
        [MinLength(3, ErrorMessage = "Username phải ít nhất 3 ký tự")]
        [MaxLength(20, ErrorMessage = "Username tối đa 20 ký tự")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]          
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password không được để trống")]
        [MinLength(8, ErrorMessage = "Password phải ít nhất 8 ký tự")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password phải có: 1 chữ thường, 1 chữ hoa, 1 số, 1 ký tự đặc biệt (@$!%*?&)")]
        public string Password { get; set; } = "";
    }
}
