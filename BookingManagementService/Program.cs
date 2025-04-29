using BookingManagementService.Data;
using BookingManagementService.Services; // We'll create this next
using BookingManagementService.HostedServices; // We'll create this later
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Configuration
var configuration = builder.Configuration;

// 2. Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Configure DbContext
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

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