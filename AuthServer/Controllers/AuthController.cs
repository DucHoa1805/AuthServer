using AuthServer.Data;
using AuthServer.DTOs;
using AuthServer.Entities;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace AuthServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        // Check username tồn tại
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest(new
            {
                error = "Username đã tồn tại",
                field = "Username"
            });

        // Check email tồn tại
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new
            {
                error = "Email đã được đăng ký",
                field = "Email"
            });
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
              CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Created($"/api/auth/{user.Id}", new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,                    
            createdAt = user.CreatedAt             
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null)
            return Unauthorized("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid password");

        var token = GenerateJwtToken(user);
        return Ok(new { token, username = user.Username });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = /* extract from token */;
        var user = await _context.Users.FindAsync(userId);
        return Ok(new { id, username, email, createdAt });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        // 1. Lấy userId từ JWT token
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("Token không hợp lệ");

        // 2. Tìm user trong database
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("User không tìm thấy");

        // 3. Verify mật khẩu cũ
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest(new { error = "Mật khẩu cũ không chính xác" });

        // 4. Check mật khẩu mới khác mật khẩu cũ
        if (request.NewPassword == request.CurrentPassword)
            return BadRequest(new { error = "Mật khẩu mới phải khác mật khẩu cũ" });

        // 5. Hash mật khẩu mới
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // 6. Lưu vào database
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đổi mật khẩu thành công" });
    }

    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request)
    {
        // 1. Lấy userId từ JWT token
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("Token không hợp lệ");

        // 2. Tìm user trong database
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound("User không tìm thấy");

        // 3. Verify mật khẩu
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return BadRequest(new { error = "Mật khẩu không chính xác" });

        // 4. Xoá user
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Tài khoản đã được xoá thành công" });
    }
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}