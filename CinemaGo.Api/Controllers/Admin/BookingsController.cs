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
public class BookingsController : ControllerBase
{
    private readonly CinemaDbContext _context;

    public BookingsController(CinemaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AdminBookingDto>>>> GetAllBookings(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = _context.TicketOrders
            .Include(o => o.User)
            .Include(o => o.ShowTime)
            .ThenInclude(s => s!.Movie)
            .Include(o => o.ShowTime)
            .ThenInclude(s => s!.Hall)
            .Include(o => o.OrderItems)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= fromDate);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= toDate);
        }

        var bookings = await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new AdminBookingDto
            {
                Id = o.Id,
                BookingCode = o.BookingCode,
                UserId = o.UserId,
                UserName = o.User.FullName,
                UserEmail = o.User.Email,
                MovieTitle = o.ShowTime!.Movie!.Title,
                HallName = o.ShowTime.Hall.Name,
                ShowDate = o.ShowTime.ShowDate,
                StartTime = o.ShowTime.StartTime,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                Seats = o.OrderItems.Select(i => i.SeatLabel).ToList(),
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();

        return Ok(ApiResponse<List<AdminBookingDto>>.SuccessResponse("Bookings fetched", bookings));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AdminBookingDto>>> GetBooking(int id)
    {
        var order = await _context.TicketOrders
            .Include(o => o.User)
            .Include(o => o.ShowTime)
            .ThenInclude(s => s!.Movie)
            .Include(o => o.ShowTime)
            .ThenInclude(s => s!.Hall)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(ApiResponse<AdminBookingDto>.ErrorResponse("Booking not found"));
        }

        var dto = new AdminBookingDto
        {
            Id = order.Id,
            BookingCode = order.BookingCode,
            UserId = order.UserId,
            UserName = order.User.FullName,
            UserEmail = order.User.Email,
            MovieTitle = order.ShowTime!.Movie!.Title,
            HallName = order.ShowTime.Hall.Name,
            ShowDate = order.ShowTime.ShowDate,
            StartTime = order.ShowTime.StartTime,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            Seats = order.OrderItems.Select(i => i.SeatLabel).ToList(),
            CreatedAt = order.CreatedAt,
            PaymentMethod = order.Payment?.Method,
            TransactionCode = order.Payment?.TransactionCode
        };

        return Ok(ApiResponse<AdminBookingDto>.SuccessResponse("Booking fetched", dto));
    }

    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<ApiResponse<string>>> CancelBooking(int id)
    {
        var order = await _context.TicketOrders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(ApiResponse<string>.ErrorResponse("Booking not found"));
        }

        if (order.Status == "Cancelled")
        {
            return BadRequest(ApiResponse<string>.ErrorResponse("Booking already cancelled"));
        }

        order.Status = "Cancelled";

        // Release seat reservations
        var orderSeatIds = order.OrderItems.Select(item => item.SeatId).ToList();
        var reservations = await _context.SeatReservations
            .Where(r => r.ShowTimeId == order.ShowTimeId && r.UserId == order.UserId &&
                orderSeatIds.Contains(r.SeatId) && r.Status == "Booked")
            .ToListAsync();

        foreach (var reservation in reservations)
        {
            reservation.Status = "Cancelled";
        }

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse("Booking cancelled"));
    }
}

public class AdminBookingDto
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Seats { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionCode { get; set; }
}
