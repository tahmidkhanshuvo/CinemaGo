using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Data;
using CinemaGo.Api.DTOs;
using CinemaGo.Api.Helpers;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CinemaController : ControllerBase
{
    private readonly CinemaDbContext _context;

    public CinemaController(CinemaDbContext context)
    {
        _context = context;
    }

    [HttpGet("showtimes")]
    public async Task<ActionResult<ApiResponse<List<ShowTimeDto>>>> GetShowTimes(
        [FromQuery] int? movieId = null,
        [FromQuery] int? hallId = null,
        [FromQuery] DateTime? date = null)
    {
        var query = _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.Status != "Cancelled");

        if (movieId.HasValue)
            query = query.Where(s => s.MovieId == movieId.Value);
        if (hallId.HasValue)
            query = query.Where(s => s.HallId == hallId.Value);
        if (date.HasValue)
        {
            // Convert to UTC for PostgreSQL compatibility
            var dateValue = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
            var dateEnd = dateValue.AddDays(1);
            query = query.Where(s => s.ShowDate >= dateValue && s.ShowDate < dateEnd);
        }

        // Get showtimes with available seats count
        var showTimeIds = await query.Select(s => s.Id).ToListAsync();
        var reservedCounts = await _context.SeatReservations
            .Where(r => showTimeIds.Contains(r.ShowTimeId) && r.Status == "Booked")
            .GroupBy(r => r.ShowTimeId)
            .Select(g => new { ShowTimeId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ShowTimeId, x => x.Count);

        var hallSeats = await _context.Halls
            .Where(h => query.Select(s => s.HallId).Contains(h.Id))
            .Select(h => new { HallId = h.Id, TotalSeats = h.TotalRows * h.TotalSeatsPerRow })
            .ToDictionaryAsync(x => x.HallId, x => x.TotalSeats);

        var showTimes = await query
            .OrderBy(s => s.ShowDate)
            .ThenBy(s => s.StartTime)
            .Select(s => new ShowTimeDto
            {
                Id = s.Id,
                MovieId = s.MovieId,
                MovieTitle = s.Movie.Title,
                MoviePosterUrl = s.Movie.PosterUrl ?? "",
                HallId = s.HallId,
                HallName = s.Hall.Name,
                BranchName = s.Hall.BranchName ?? s.Hall.Name,
                ShowDate = s.ShowDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BasePrice = s.BasePrice,
                Status = s.Status,
                AvailableSeats = 0 // Will be set below
            })
            .ToListAsync();

        // Set available seats
        foreach (var st in showTimes)
        {
            var totalSeats = hallSeats.GetValueOrDefault(st.HallId, 0);
            var reserved = reservedCounts.GetValueOrDefault(st.Id, 0);
            st.AvailableSeats = totalSeats - reserved;
        }

        return Ok(ApiResponse<List<ShowTimeDto>>.SuccessResponse("Showtimes fetched", showTimes));
    }

    [HttpGet("showtimes/{id}")]
    public async Task<ActionResult<ApiResponse<ShowTimeWithSeatsDto>>> GetShowTimeWithSeats(int id)
    {
        var showTime = await _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (showTime == null)
        {
            return NotFound(ApiResponse<ShowTimeWithSeatsDto>.ErrorResponse("Showtime not found"));
        }

        // Get reserved seats
        var reservedSeatIds = await _context.SeatReservations
            .Where(r => r.ShowTimeId == id && r.Status == "Booked")
            .Select(r => r.SeatId)
            .ToListAsync();

        // Get all seats for the hall
        var seats = await _context.HallSeats
            .Include(s => s.Category)
            .Where(s => s.HallId == showTime.HallId && s.IsActive)
            .OrderBy(s => s.RowLabel)
            .ThenBy(s => s.SeatNumber)
            .Select(s => new SeatDto
            {
                Id = s.Id,
                SeatLabel = s.SeatLabel,
                RowLabel = s.RowLabel,
                SeatNumber = s.SeatNumber,
                CategoryName = s.Category.Name,
                Price = showTime.BasePrice + s.Category.FixedExtraPrice,
                ExtraPrice = s.Category.FixedExtraPrice,
                IsAvailable = !reservedSeatIds.Contains(s.Id)
            })
            .ToListAsync();

        var categories = await _context.SeatCategories
            .Where(c => c.HallId == showTime.HallId)
            .Select(c => new SeatCategoryDto
            {
                Id = c.Id,
                HallId = c.HallId,
                Name = c.Name,
                FixedExtraPrice = c.FixedExtraPrice
            })
            .ToListAsync();

        var result = new ShowTimeWithSeatsDto
        {
            Id = showTime.Id,
            MovieId = showTime.MovieId,
            MovieTitle = showTime.Movie.Title,
            MoviePosterUrl = showTime.Movie.PosterUrl ?? "",
            HallId = showTime.HallId,
            HallName = showTime.Hall.Name,
            ShowDate = showTime.ShowDate,
            StartTime = showTime.StartTime,
            EndTime = showTime.EndTime,
            BasePrice = showTime.BasePrice,
            Status = showTime.Status,
            Seats = seats,
            Categories = categories,
            ReservedSeatIds = reservedSeatIds
        };

        return Ok(ApiResponse<ShowTimeWithSeatsDto>.SuccessResponse("Showtime fetched", result));
    }

    [HttpPost("book")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<BookingResponseDto>>> BookTickets([FromBody] BookingRequestDto request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResponse<BookingResponseDto>.ErrorResponse("Unauthorized"));
        }

        if (request.SeatIds == null || request.SeatIds.Count == 0 || request.SeatIds.Count > 10 ||
            request.SeatIds.Distinct().Count() != request.SeatIds.Count)
        {
            return BadRequest(ApiResponse<BookingResponseDto>.ErrorResponse("Select between 1 and 10 different seats."));
        }

        // Serialize bookings across application instances; roll back every write together.
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({request.ShowTimeId})");

        // Get showtime
        var showTime = await _context.ShowTimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == request.ShowTimeId);

        if (showTime == null)
        {
            return NotFound(ApiResponse<BookingResponseDto>.ErrorResponse("Showtime not found"));
        }

        if (showTime.Status != "Scheduled" || !showTime.Movie.IsPublished || !showTime.Hall.IsActive || showTime.ShowDate.Date.Add(showTime.StartTime) <= DateTime.UtcNow)
        {
            return BadRequest(ApiResponse<BookingResponseDto>.ErrorResponse("This showtime is no longer available for booking."));
        }

        // Check seat availability
        var reservedSeatIds = await _context.SeatReservations
            .Where(r => r.ShowTimeId == request.ShowTimeId && r.Status == "Booked")
            .Select(r => r.SeatId)
            .ToListAsync();

        foreach (var seatId in request.SeatIds)
        {
            if (reservedSeatIds.Contains(seatId))
            {
                return BadRequest(ApiResponse<BookingResponseDto>.ErrorResponse($"Seat {seatId} is already booked"));
            }
        }

        // Get seat details and calculate price
        var seats = await _context.HallSeats
            .Include(s => s.Category)
            .Where(s => request.SeatIds.Contains(s.Id) && s.HallId == showTime.HallId && s.IsActive)
            .ToListAsync();

        if (seats.Count != request.SeatIds.Count)
        {
            return BadRequest(ApiResponse<BookingResponseDto>.ErrorResponse("One or more seats do not belong to this hall or are unavailable."));
        }

        decimal totalAmount = 0;
        foreach (var seat in seats)
        {
            totalAmount += showTime.BasePrice + seat.Category.FixedExtraPrice;
        }

        // Create payment
        var payment = new Payment
        {
            UserId = userId,
            PaymentType = "TICKET",
            ReferenceType = "TicketOrder",
            ReferenceId = 0,
            Amount = totalAmount,
            Method = "DummyCard",
            Status = "Success",
            CreatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Generate booking code
        var bookingCode = "CG" + Guid.NewGuid().ToString("N")[..8].ToUpper();

        // Create ticket order
        var order = new TicketOrder
        {
            UserId = userId,
            ShowTimeId = request.ShowTimeId,
            BookingCode = bookingCode,
            TotalAmount = totalAmount,
            PaymentId = payment.Id,
            Status = "Paid",
            CreatedAt = DateTime.UtcNow
        };
        _context.TicketOrders.Add(order);
        await _context.SaveChangesAsync();
        payment.ReferenceId = order.Id;

        // Create order items and seat reservations
        foreach (var seat in seats)
        {
            var unitPrice = showTime.BasePrice + seat.Category.FixedExtraPrice;

            _context.TicketOrderItems.Add(new TicketOrderItem
            {
                TicketOrderId = order.Id,
                SeatId = seat.Id,
                SeatLabel = seat.SeatLabel,
                UnitPrice = unitPrice
            });

            _context.SeatReservations.Add(new SeatReservation
            {
                ShowTimeId = request.ShowTimeId,
                SeatId = seat.Id,
                UserId = userId,
                Status = "Booked",
                Price = unitPrice,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        var response = new BookingResponseDto
        {
            OrderId = order.Id,
            BookingCode = bookingCode,
            TotalAmount = totalAmount,
            ShowTimeId = request.ShowTimeId,
            MovieTitle = showTime.Movie.Title,
            ShowDate = showTime.ShowDate,
            SeatLabels = seats.Select(s => s.SeatLabel).ToList()
        };

        return Ok(ApiResponse<BookingResponseDto>.SuccessResponse("Booking successful", response));
    }
}
