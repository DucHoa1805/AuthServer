using System.ComponentModel.DataAnnotations;

namespace AuthServer.DTOs;

public class LoginRequest
{
    [Required(ErrorMessage = "Username không được để trống")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Password không được để trống")]
    public required string Password { get; set; }
}
