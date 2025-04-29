using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Dtos;
using NotificationService.Models; // Needed for endpoint mapping later
using NotificationService.Services; // Add this for the service layer

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container ---

// 1. Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Register Custom Services (we'll create this next)
builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>(); // Use fully qualified name or add using statement

builder.Services.AddControllers(); // If using Controllers approach

// Add minimal API services (or Controllers)
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "NotificationService API", Version = "v1" });
});

builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

var app = builder.Build();

// --- Configure the HTTP request pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationService API v1"));
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
