namespace AuthServer.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;      // ← NEW
    public DateTime? UpdatedAt { get; set; }                         // ← NEW
    public bool IsActive { get; set; } = true;
}