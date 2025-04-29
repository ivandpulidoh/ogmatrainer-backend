using AuthenticationService.Data;
using AuthenticationService.Services;
using Microsoft.EntityFrameworkCore; // Keep this
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// --- Add services to the container ---

// 1. Configure DbContext for MariaDB
var connectionString = configuration.GetConnectionString("DefaultConnection");

// --- IMPORTANT CHANGE HERE ---
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        // Optional: Configure retry logic for transient failures
        // .EnableRetryOnFailure(
        //     maxRetryCount: 5,
        //     maxRetryDelay: TimeSpan.FromSeconds(30),
        //     errorNumbersToAdd: null)
);
// --- END OF IMPORTANT CHANGE ---

// 2. Register Custom Services (No change needed here)
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<TokenService>();

// 3. Add Controllers & API Explorer/Swagger (No change needed here)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Build the App ---
var app = builder.Build();

// --- Configure the HTTP request pipeline ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();   

     // Apply migrations automatically in development (optional)
     try // Add try-catch for potential startup migration issues
     {
         using (var scope = app.Services.CreateScope())
         {
             var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
             // Ensure database exists and migrations are applied
             dbContext.Database.Migrate();
         }
     }
     catch (Exception ex)
     {
         // Log the error if migration fails during startup
         var logger = app.Services.GetRequiredService<ILogger<Program>>();
         logger.LogError(ex, "An error occurred while migrating the database.");
         // Optionally, decide if the app should stop or continue if migration fails
         // throw; // Re-throw to stop the application
     } 
}   

app.UseHttpsRedirection();

// If needed:
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// --- Run the App ---
app.Run();