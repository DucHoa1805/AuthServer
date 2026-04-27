using AuthServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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