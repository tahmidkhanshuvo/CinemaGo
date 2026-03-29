using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CinemaGo.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=cinemago;Username=postgres;Password=password";

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register DatabaseInitializer
builder.Services.AddScoped<DatabaseInitializer>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:SecretKey"] ?? "CinemaGoSecretKey12345678901234567890";
var jwtIssuer = "CinemaGo";
var jwtAudience = "CinemaGo";

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

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Initialize database (create tables and seed data)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync(context);
    
    // Also seed additional data (even if initial data exists)
    // This adds more movies, halls, and showtimes without deleting existing data
    await initializer.SeedDataAsync(context);
}


// app.UseSwagger();
// app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
