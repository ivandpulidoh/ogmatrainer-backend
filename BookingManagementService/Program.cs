using BookingManagementService.Data;
using BookingManagementService.Services; // We'll create this next
using BookingManagementService.HostedServices; // We'll create this later
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using BookingManagementService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Configuration
var configuration = builder.Configuration;
var jwtKey = configuration["Jwt:Key"];
var jwtIssuer = configuration["Jwt:Issuer"];
var jwtAudience = configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT settings (Key, Issuer, Audience) must be configured in appsettings.");
}

// 2. Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
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

// 3. Authorization (needed to use [Authorize] attributes)
builder.Services.AddAuthorization();


// 2. Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Configure DbContext
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddHttpContextAccessor(); 

builder.Services.AddHttpClient("RoutineEquipmentServiceClient", (serviceProvider, client) =>
{

    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

    var baseUrl = configuration["RoutineEquipmentService:BaseUrl"];
    if (!string.IsNullOrEmpty(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }

    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

    var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
    if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
    }

});
builder.Services.AddScoped<IRoutineEquipmentServiceClient, RoutineEquipmentHttpClientService>();

// 4. Configure HttpClient for Notification Service
builder.Services.AddHttpClient("NotificationServiceClient", client =>
{
    var notificationServiceUrl = configuration["NotificationService:BaseUrl"];
    if (string.IsNullOrEmpty(notificationServiceUrl))
    {
        // Handle missing configuration appropriately
        throw new InvalidOperationException("Notification Service BaseUrl is not configured.");
    }
    client.BaseAddress = new Uri(notificationServiceUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// 5. Register Custom Services (Create these interfaces/classes next)
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<INotificationService, NotificationHttpService>(); // Service to wrap HttpClient logic

// 6. Register Hosted Service for checking missed reservations (Create later)
builder.Services.AddHostedService<MissedReservationChecker>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Enable if needed

app.UseAuthorization();

app.MapControllers();

app.Run();