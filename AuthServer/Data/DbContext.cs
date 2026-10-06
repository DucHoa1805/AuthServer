using Microsoft.EntityFrameworkCore;
using AuthServer.Entities;

namespace AuthServer.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();

            // Unique chỉ áp dụng cho user còn hoạt động,
            // cho phép đăng ký lại username/email sau khi tài khoản bị xoá (soft delete)
            entity.HasIndex(u => u.Username).IsUnique().HasFilter("[IsActive] = 1");
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("[IsActive] = 1");
        });
    }
}