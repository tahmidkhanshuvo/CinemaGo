using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Data;
using CinemaGo.Api.DTOs;
using CinemaGo.Api.Helpers;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class ShowTimesController : ControllerBase
{
    private readonly CinemaDbContext _context;
    
    public ShowTimesController(CinemaDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminShowTimeDto>>>> GetAllShowTimes(
        [FromQuery] int? movieId = null,
        [FromQuery] int? hallId = null,
        [FromQuery] DateTime? date = null)
    {
        var query = _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .AsQueryable();
        
        if (movieId.HasValue)
        {
            query = query.Where(s => s.MovieId == movieId);
        }
        
        if (hallId.HasValue)
        {
            query = query.Where(s => s.HallId == hallId);
        }
        
        if (date.HasValue)
        {
            query = query.Where(s => s.ShowDate.Date == date.Value.Date);
        }
        
        var showTimes = await query
            .OrderBy(s => s.ShowDate)
            .ThenBy(s => s.StartTime)
            .Select(s => new AdminShowTimeDto
            {
                Id = s.Id,
                MovieId = s.MovieId,
                MovieTitle = s.Movie.Title,
                HallId = s.HallId,
                HallName = s.Hall.Name,
                ShowDate = s.ShowDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BasePrice = s.BasePrice,
                Status = s.Status,
                BookedSeats = s.Reservations.Count(r => r.Status == "Booked")
            })
            .ToListAsync();
        
        return Ok(ApiResponse<List<AdminShowTimeDto>>.SuccessResponse("Showtimes fetched", showTimes));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminShowTimeDto>>> GetShowTime(int id)
    {
        var showTime = await _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (showTime == null)
        {
            return NotFound(ApiResponse<AdminShowTimeDto>.ErrorResponse("Showtime not found"));
        }
        
        var dto = new AdminShowTimeDto
        {
            Id = showTime.Id,
            MovieId = showTime.MovieId,
            MovieTitle = showTime.Movie.Title,
            HallId = showTime.HallId,
            HallName = showTime.Hall.Name,
            ShowDate = showTime.ShowDate,
            StartTime = showTime.StartTime,
            EndTime = showTime.EndTime,
            BasePrice = showTime.BasePrice,
            Status = showTime.Status,
            BookedSeats = showTime.Reservations.Count(r => r.Status == "Booked")
        };
        
        return Ok(ApiResponse<AdminShowTimeDto>.SuccessResponse("Showtime fetched", dto));
    }
    
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminShowTimeDto>>> CreateShowTime([FromBody] CreateShowTimeRequest request)
    {
        var movie = await _context.Movies.FindAsync(request.MovieId);
        if (movie == null)
        {
            return NotFound(ApiResponse<AdminShowTimeDto>.ErrorResponse("Movie not found"));
        }
        
        var hall = await _context.Halls.FindAsync(request.HallId);
        if (hall == null)
        {
            return NotFound(ApiResponse<AdminShowTimeDto>.ErrorResponse("Hall not found"));
        }
        
        var startTime = request.StartTime;
        var endTime = startTime.Add(TimeSpan.FromMinutes(request.DurationMinutes));
        
        // Convert ShowDate to UTC for PostgreSQL compatibility
        var showDateUtc = request.ShowDate.Kind == DateTimeKind.Utc 
            ? request.ShowDate 
            : request.ShowDate.ToUniversalTime();
        
        // Check for conflicts
        var conflicts = await _context.ShowTimes
            .Where(s => s.HallId == request.HallId 
                && s.ShowDate.Date == showDateUtc.Date
                && s.Status != "Cancelled"
                && ((s.StartTime <= startTime && startTime < s.EndTime)
                    || (s.StartTime < endTime && endTime <= s.EndTime)
                    || (startTime <= s.StartTime && s.EndTime <= endTime)))
            .AnyAsync();
        
        if (conflicts)
        {
            return BadRequest(ApiResponse<AdminShowTimeDto>.ErrorResponse("Time slot conflicts with existing showtime"));
        }
        
        var showTime = new ShowTime
        {
            MovieId = request.MovieId,
            HallId = request.HallId,
            ShowDate = showDateUtc,
            StartTime = startTime,
            EndTime = endTime,
            BasePrice = request.BasePrice,
            Status = "Scheduled",
            CreatedAt = DateTime.UtcNow
        };
        
        _context.ShowTimes.Add(showTime);
        await _context.SaveChangesAsync();
        
        var dto = new AdminShowTimeDto
        {
            Id = showTime.Id,
            MovieId = showTime.MovieId,
            MovieTitle = movie.Title,
            HallId = showTime.HallId,
            HallName = hall.Name,
            ShowDate = showTime.ShowDate,
            StartTime = showTime.StartTime,
            EndTime = showTime.EndTime,
            BasePrice = showTime.BasePrice,
            Status = showTime.Status,
            BookedSeats = 0
        };
        
        return Ok(ApiResponse<AdminShowTimeDto>.SuccessResponse("Showtime created", dto));
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<AdminShowTimeDto>>> UpdateShowTime(int id, [FromBody] UpdateShowTimeRequest request)
    {
        var showTime = await _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (showTime == null)
        {
            return NotFound(ApiResponse<AdminShowTimeDto>.ErrorResponse("Showtime not found"));
        }
        
        if (request.Status != null)
        {
            showTime.Status = request.Status;
        }
        
        if (request.BasePrice.HasValue)
        {
            showTime.BasePrice = request.BasePrice.Value;
        }
        
        await _context.SaveChangesAsync();
        
        var dto = new AdminShowTimeDto
        {
            Id = showTime.Id,
            MovieId = showTime.MovieId,
            MovieTitle = showTime.Movie.Title,
            HallId = showTime.HallId,
            HallName = showTime.Hall.Name,
            ShowDate = showTime.ShowDate,
            StartTime = showTime.StartTime,
            EndTime = showTime.EndTime,
            BasePrice = showTime.BasePrice,
            Status = showTime.Status,
            BookedSeats = showTime.Reservations.Count(r => r.Status == "Booked")
        };
        
        return Ok(ApiResponse<AdminShowTimeDto>.SuccessResponse("Showtime updated", dto));
    }
    
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteShowTime(int id)
    {
        var showTime = await _context.ShowTimes.FindAsync(id);
        if (showTime == null)
        {
            return NotFound(ApiResponse<string>.ErrorResponse("Showtime not found"));
        }
        
        // Check if there are any bookings
        var hasBookings = await _context.SeatReservations
            .AnyAsync(r => r.ShowTimeId == id && r.Status == "Booked");
        
        if (hasBookings)
        {
            showTime.Status = "Cancelled";
        }
        else
        {
            _context.ShowTimes.Remove(showTime);
        }
        
        await _context.SaveChangesAsync();
        
        return Ok(ApiResponse<string>.SuccessResponse("Showtime deleted/cancelled"));
    }
}

public class AdminShowTimeDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int HallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BookedSeats { get; set; }
}

public class UpdateShowTimeRequest
{
    public string? Status { get; set; }
    public decimal? BasePrice { get; set; }
}