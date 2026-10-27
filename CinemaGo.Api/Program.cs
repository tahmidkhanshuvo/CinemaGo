using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CinemaGo.Api.Data;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Development.local.json", optional: true).AddEnvironmentVariables();

// Add services to the container.

// Get connection string from configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection before starting CinemaGo.");

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register DatabaseInitializer
builder.Services.AddScoped<DatabaseInitializer>();

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Configure Jwt__SecretKey with at least 32 bytes.");
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
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var id = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var database = context.HttpContext.RequestServices.GetRequiredService<CinemaDbContext>();
            var user = int.TryParse(id, out var userId) ? await database.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId) : null;
            if (user == null || !user.IsActive || user.Role != context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value)
                context.Fail("This account or role is no longer active.");
        }
    };
});

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = (builder.Configuration["FRONTEND_URL"] ?? "http://localhost:3000")
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Concat(new[] { "https://cinemago.tahmidkhan.com.bd" }).Distinct().ToArray();
        policy.WithOrigins(origins)
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
    if (builder.Configuration.GetValue<bool>("Database:SeedDemoData"))
        await initializer.SeedDataAsync(context);
}


// app.UseSwagger();
// app.UseSwaggerUI();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/healthz", async (CinemaDbContext database) =>
    await database.Database.CanConnectAsync()
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(503));

app.Run();
