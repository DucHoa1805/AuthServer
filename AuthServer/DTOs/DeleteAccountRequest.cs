using System.ComponentModel.DataAnnotations;

namespace AuthServer.DTOs
{
    public class DeleteAccountRequest
    {
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = "";
    }
}
