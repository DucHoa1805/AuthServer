using AuthServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Fail-fast: chặn startup nếu cấu hình JWT thiếu thay vì crash lệch lúc runtime
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException(
        "Jwt:Key chưa được cấu hình. Dùng user secrets: " +
        "'dotnet user-secrets set \"Jwt:Key\" \"<key-ít-nhất-32-ký-tự>\"'");
if (jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key phải ít nhất 32 ký tự để an toàn với HMAC-SHA256.");
if (string.IsNullOrWhiteSpace(jwtIssuer) || string.IsNullOrWhiteSpace(jwtAudience))
    throw new InvalidOperationException("Jwt:Issuer và Jwt:Audience là bắt buộc.");

builder.Services.AddControllers();

// 1. Thêm cái này để hỗ trợ Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuthServer API",
        Version = "v1",
        Description = "Secure Authentication API with JWT"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using Bearer scheme.\r\n\r\n" +
                      "Enter 'Bearer' [space] then your token.\r\n\r\n" +
                      "Example: 'Bearer eyJhbGc...'"
    });

    c.AddSecurityRequirement(document =>
    {
        var securityRequirement = new OpenApiSecurityRequirement();
        securityRequirement.Add(
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>());
        return securityRequirement;
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Token cấp trước lần đổi mật khẩu/xoá tài khoản gần nhất bị từ chối (401)
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var db = context.HttpContext.RequestServices
                    .GetRequiredService<AppDbContext>();

                var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    context.Fail("Token không hợp lệ");
                    return;
                }

                var user = await db.Users.FindAsync(userId);
                var versionClaim = context.Principal?.FindFirst("token_version");

                if (user == null || !user.IsActive ||
                    versionClaim == null ||
                    !int.TryParse(versionClaim.Value, out var tokenVersion) ||
                    tokenVersion != user.TokenVersion)
                {
                    context.Fail("Token đã hết hiệu lực");
                }
            }
        };
    });
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 2. Cấu hình Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // Tạo ra file swagger.json
    app.UseSwaggerUI(); // Tạo ra giao diện /swagger/index.html
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();