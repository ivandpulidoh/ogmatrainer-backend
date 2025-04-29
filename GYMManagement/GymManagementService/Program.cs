using Microsoft.EntityFrameworkCore;
using GymManagementService.Data; // Your DbContext namespace
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // For ServerVersion

// Add services to the container.
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Retrieve the connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register the DbContext
builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseMySql(connectionString,
        // Specify the MySQL server version
        // AutoDetect is convenient but explicit version is more reliable
        // Example for MySQL 8.0: ServerVersion.Create(8, 0, 29, ServerType.MySql)
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: System.TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null) // Configure resilience
        )
    // Optional: Add logging for EF Core queries (useful during development)
    // .LogTo(Console.WriteLine, LogLevel.Information)
    // .EnableSensitiveDataLogging() // Only in development!
    );


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// *** Add this line AFTER builder.Services.AddControllers() ***
// This handles potential object cycle issues when serializing entities with navigation properties
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // Optional: Don't serialize nulls
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();