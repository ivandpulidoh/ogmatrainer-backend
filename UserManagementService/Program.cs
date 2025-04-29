using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserManagementService.Data;
using UserManagementService.Services; // Add this using
// Add other necessary usings for services/interfaces later

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
var jwtKey = configuration["Jwt:Key"];
var jwtIssuer = configuration["Jwt:Issuer"];
var jwtAudience = configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(connectionString)) {
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}
if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience)) {
     throw new InvalidOperationException("JWT settings (Key, Issuer, Audience) must be configured in appsettings.");
}


// --- Add services to the container ---

// 1. DbContext
builder.Services.AddDbContext<UserManagementDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure( // Optional: resilience
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
        )
    // .LogTo(Console.WriteLine, LogLevel.Information) // Optional: Log EF Core queries
    // .EnableSensitiveDataLogging() // Optional: Only for development
    );


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
        ValidateLifetime = true, // Ensure token isn't expired
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// 3. Authorization (needed to use [Authorize] attributes)
builder.Services.AddAuthorization();


// 4. Controllers
builder.Services.AddControllers();

// 5. Swagger/OpenAPI (already configured by template usually)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    // Optional: Add JWT UI support in Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, // Use Http for Bearer
        Scheme = "bearer", // Must be lower case
        BearerFormat = "JWT"
     });
     c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
     });
});


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPersonalInformationService, PersonalInformationService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// --- Build the App ---
var app = builder.Build();

// --- Configure the HTTP request pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // More detailed errors in dev
}
else
{
    // Add production error handling (e.g., custom middleware)
    app.UseExceptionHandler("/error"); // Needs an /error endpoint
    app.UseHsts(); // Enforce HTTPS
}

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS

// IMPORTANT: Authentication middleware MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers(); // Map attribute-routed controllers

// --- Run the App ---
app.Run();