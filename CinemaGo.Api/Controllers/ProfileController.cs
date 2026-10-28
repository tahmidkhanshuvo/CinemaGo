using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Data;
using CinemaGo.Api.DTOs;
using CinemaGo.Api.Helpers;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly CinemaDbContext _context;
    
    public ProfileController(CinemaDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
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
        
        return Ok(ApiResponse<UserDto>.SuccessResponse("Profile fetched", userDto));
    }
    
    [HttpPut]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileRequest request)
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
        
        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        var userDto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role
        };
        
        return Ok(ApiResponse<UserDto>.SuccessResponse("Profile updated", userDto));
    }
    
    [HttpPut("password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var userId))
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsActive)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest(ApiResponse<object>.ErrorResponse("Current password is incorrect."));
        if (request.NewPassword.Length < 12 || System.Text.Encoding.UTF8.GetByteCount(request.NewPassword) > 72
            || request.NewPassword != request.ConfirmPassword || request.CurrentPassword == request.NewPassword)
            return BadRequest(ApiResponse<object>.ErrorResponse("Choose a different password of at least 12 characters (at most 72 UTF-8 bytes), and confirm it."));

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse("Password changed. Sign in again on your devices."));
    }

    [HttpGet("purchases")]
    public async Task<ActionResult<ApiResponse<ProfilePurchasesDto>>> GetPurchases()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResponse<ProfilePurchasesDto>.ErrorResponse("Unauthorized"));
        }
        
        // Get OTT purchases
        var ottPurchases = await _context.OttPurchases
            .Include(p => p.Movie)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PurchasedAt)
            .Select(p => new OttPurchaseDto
            {
                Id = p.Id,
                MovieId = p.MovieId,
                MovieTitle = p.Movie.Title,
                MoviePosterUrl = p.Movie.PosterUrl ?? "",
                Amount = p.Amount,
                PurchasedAt = p.PurchasedAt
            })
            .ToListAsync();
        
        // Get ticket orders
        var ticketOrders = await _context.TicketOrders
            .Include(o => o.ShowTime)
            .ThenInclude(s => s!.Movie)
            .Include(o => o.ShowTime)
            .ThenInclude(s => s!.Hall)
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId && o.Status == "Paid")
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new TicketOrderDto
            {
                Id = o.Id,
                BookingCode = o.BookingCode,
                MovieTitle = o.ShowTime!.Movie!.Title,
                MoviePosterUrl = o.ShowTime.Movie.PosterUrl ?? "",
                HallName = o.ShowTime.Hall.Name,
                ShowDate = o.ShowTime.ShowDate,
                StartTime = o.ShowTime.StartTime,
                Seats = o.OrderItems.Select(i => i.SeatLabel).ToList(),
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
        
        var response = new ProfilePurchasesDto
        {
            OttPurchases = ottPurchases,
            TicketOrders = ticketOrders
        };
        
        return Ok(ApiResponse<ProfilePurchasesDto>.SuccessResponse("Purchases fetched", response));
    }
}
