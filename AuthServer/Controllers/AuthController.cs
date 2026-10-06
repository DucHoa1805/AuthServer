using AuthServer.Data;
using AuthServer.DTOs;
using AuthServer.Entities;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
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
        // Check username tồn tại (chỉ tính user còn hoạt động)
        if (await _context.Users.AnyAsync(u => u.Username == request.Username && u.IsActive))
            return BadRequest(new
            {
                error = "Username đã tồn tại",
                field = "Username"
            });

        // Check email tồn tại
        if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.IsActive))
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
     .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        // Thông báo chung cho cả 2 trường hợp (không tồn tại / sai mật khẩu)
        // để tránh lộ thông tin username nào hợp lệ
        if (user == null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "Username hoặc mật khẩu không chính xác" });

        var token = GenerateJwtToken(user);
        return Ok(new { token, username = user.Username });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var (user, error) = await GetCurrentUserAsync();
        if (user == null)
            return error!;

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            createdAt = user.CreatedAt
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var (user, error) = await GetCurrentUserAsync();
        if (user == null)
            return error!;

        // Verify mật khẩu cũ
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest(new { error = "Mật khẩu cũ không chính xác" });

        // Check mật khẩu mới khác mật khẩu cũ
        if (request.NewPassword == request.CurrentPassword)
            return BadRequest(new { error = "Mật khẩu mới phải khác mật khẩu cũ" });

        // Hash mật khẩu mới + tăng TokenVersion để vô hiệu hoá mọi token cũ
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.TokenVersion++;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Đổi mật khẩu thành công, vui lòng đăng nhập lại" });
    }

    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request)
    {
        var (user, error) = await GetCurrentUserAsync();
        if (user == null)
            return error!;

        // Verify mật khẩu
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return BadRequest(new { error = "Mật khẩu không chính xác" });

        // Soft delete: vô hiệu hoá tài khoản, giữ dữ liệu để có thể khôi phục
        user.IsActive = false;
        user.TokenVersion++;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Tài khoản đã được xoá thành công" });
    }

    /// <summary>
    /// Lấy user hiện tại từ JWT token (dùng chung cho các endpoint [Authorize]).
    /// Trả về (null, errorResponse) nếu token không hợp lệ hoặc user không còn hoạt động.
    /// </summary>
    private async Task<(User? user, IActionResult? error)> GetCurrentUserAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return (null, Unauthorized(new { error = "Token không hợp lệ" }));

        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsActive)
            return (null, Unauthorized(new { error = "Tài khoản không tồn tại hoặc đã bị vô hiệu hoá" }));

        return (user, null);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("token_version", user.TokenVersion.ToString())
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
