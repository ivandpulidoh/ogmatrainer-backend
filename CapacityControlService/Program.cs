using System.Text;
using CapacityControlService.Data;
using CapacityControlService.Interfaces;
using CapacityControlService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Add if using JWT
using Microsoft.AspNetCore.Authorization; // Add if using JWT/AuthZ
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Add if using JWT

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var jwtKey = configuration["Jwt:Key"];
var jwtIssuer = configuration["Jwt:Issuer"];
var jwtAudience = configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience)) {
     throw new InvalidOperationException("JWT settings (Key, Issuer, Audience) must be configured in appsettings.");
}

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Configure Swagger for JWT if needed

// Configure DbContext
builder.Services.AddDbContext<CapacityDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Configure HttpClient for Notification Service
builder.Services.AddHttpClient("NotificationServiceClient");

// Register Custom Services
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ISymptomService, SymptomService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<ICapacityService, CapacityService>();
builder.Services.AddScoped<INotificationService, NotificationHttpService>();
builder.Services.AddScoped<IAdminFinderService, AdminFinderService>();

//Authentication/Authorization
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization(options => {     
     options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Configure JWT for Swagger UI if using Auth
}

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();



app.MapControllers();

app.Run();