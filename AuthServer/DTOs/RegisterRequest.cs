using System.ComponentModel.DataAnnotations;

namespace AuthServer.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; } = "";

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = "";
    }
}
