using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Data;
using CinemaGo.Api.DTOs;
using CinemaGo.Api.Helpers;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly CinemaDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(CinemaDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Email already exists",
                new Dictionary<string, List<string>> { { "email", new List<string> { "Email already exists" } } }));
        }

        // Create new user
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate token
        var token = JwtHelper.GenerateToken(user, _configuration["Jwt:SecretKey"]!, Math.Clamp(_configuration.GetValue<int?>("Jwt:ExpiryHours") ?? 24, 1, 168));

        var response = new AuthResponse
        {
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role
            },
            Token = token
        };

        return Ok(ApiResponse<AuthResponse>.SuccessResponse("Registration successful", response));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse("Invalid email or password"));
        }

        if (!user.IsActive)
        {
            return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse("Account is disabled"));
        }

        var token = JwtHelper.GenerateToken(user, _configuration["Jwt:SecretKey"]!, Math.Clamp(_configuration.GetValue<int?>("Jwt:ExpiryHours") ?? 24, 1, 168));

        var response = new AuthResponse
        {
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role
            },
            Token = token
        };

        return Ok(ApiResponse<AuthResponse>.SuccessResponse("Login successful", response));
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Unauthorized"));
        }

        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<UserDto>.ErrorResponse("User not found"));
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role
        };

        return Ok(ApiResponse<UserDto>.SuccessResponse("Current user fetched", userDto));
    }
}
