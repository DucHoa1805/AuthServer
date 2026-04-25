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
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]          // ← NEW
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password không được để trống")]
        [MinLength(8, ErrorMessage = "Password phải ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)",                   // ← NEW
        ErrorMessage = "Password phải có 1 ký tự hoa và 1 số")]
        public string Password { get; set; } = "";
    }
}
