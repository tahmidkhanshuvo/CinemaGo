using System.Net.Http.Json;
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
public class HallsController : ControllerBase
{
    private readonly CinemaDbContext _context;

    public HallsController(CinemaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<HallDto>>>> GetHalls()
    {
        var halls = await _context.Halls
            .Include(h => h.SeatCategories)
            .Where(h => h.IsActive)
            .OrderBy(h => h.Name)
            .ToListAsync();

        var hallDtos = halls.Select(h => new HallDto
        {
            Id = h.Id,
            Name = h.Name,
            BranchName = h.BranchName ?? "",
            TotalRows = h.TotalRows,
            TotalSeatsPerRow = h.TotalSeatsPerRow,
            IsActive = h.IsActive,
            SeatCategories = h.SeatCategories.Select(c => new SeatCategoryDto
            {
                Id = c.Id,
                HallId = c.HallId,
                Name = c.Name,
                FixedExtraPrice = c.FixedExtraPrice
            }).ToList()
        }).ToList();

        return Ok(ApiResponse<List<HallDto>>.SuccessResponse("Halls fetched", hallDtos));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<HallDto>>> GetHall(int id)
    {
        var hall = await _context.Halls
            .Include(h => h.SeatCategories)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hall == null)
        {
            return NotFound(ApiResponse<HallDto>.ErrorResponse("Hall not found"));
        }

        var hallDto = new HallDto
        {
            Id = hall.Id,
            Name = hall.Name,
            BranchName = hall.BranchName ?? "",
            TotalRows = hall.TotalRows,
            TotalSeatsPerRow = hall.TotalSeatsPerRow,
            IsActive = hall.IsActive,
            SeatCategories = hall.SeatCategories.Select(c => new SeatCategoryDto
            {
                Id = c.Id,
                HallId = c.HallId,
                Name = c.Name,
                FixedExtraPrice = c.FixedExtraPrice
            }).ToList()
        };

        return Ok(ApiResponse<HallDto>.SuccessResponse("Hall fetched", hallDto));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<HallDto>>> CreateHall([FromBody] CreateHallRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.TotalRows is < 1 or > 26 || request.TotalSeatsPerRow is < 1 or > 50 || request.SeatCategories?.Any(c => c.FixedExtraPrice < 0 || string.IsNullOrWhiteSpace(c.Name)) == true)
            return BadRequest(ApiResponse<HallDto>.ErrorResponse("Enter a hall name, 1–26 rows, 1–50 seats per row and non-negative category prices."));
        await using var transaction = await _context.Database.BeginTransactionAsync();
        // Create hall
        var hall = new Hall
        {
            Name = request.Name,
            BranchName = request.BranchName,
            TotalRows = request.TotalRows,
            TotalSeatsPerRow = request.TotalSeatsPerRow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Halls.Add(hall);
        await _context.SaveChangesAsync();

        // Create seat categories
        var categoryIds = new List<int>();
        var categoriesToCreate = request.SeatCategories?.Any() == true
            ? request.SeatCategories
            : new List<CreateSeatCategoryRequest>
            {
                new CreateSeatCategoryRequest { Name = "Regular", FixedExtraPrice = 0 }
            };

        foreach (var category in categoriesToCreate)
        {
            var seatCategory = new SeatCategory
            {
                HallId = hall.Id,
                Name = category.Name,
                FixedExtraPrice = category.FixedExtraPrice
            };
            _context.SeatCategories.Add(seatCategory);
            await _context.SaveChangesAsync();
            categoryIds.Add(seatCategory.Id);
        }

        // Generate seats
        var rowLabels = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        // Ensure we have at least one category
        if (!categoryIds.Any())
        {
            var defaultCategory = new SeatCategory
            {
                HallId = hall.Id,
                Name = "Regular",
                FixedExtraPrice = 0
            };
            _context.SeatCategories.Add(defaultCategory);
            await _context.SaveChangesAsync();
            categoryIds.Add(defaultCategory.Id);
        }

        for (int row = 0; row < request.TotalRows; row++)
        {
            string rowLabel = rowLabels[row].ToString();

            // Determine category for this row based on row index
            int categoryIndex = categoryIds.Count > 0 ? row * categoryIds.Count / Math.Max(1, request.TotalRows) : 0;
            int categoryId = categoryIds.Any() ? categoryIds[categoryIndex] : categoryIds.First();

            for (int seat = 1; seat <= request.TotalSeatsPerRow; seat++)
            {
                string seatLabel = $"{rowLabel}{seat}";
                var hallSeat = new HallSeat
                {
                    HallId = hall.Id,
                    SeatLabel = seatLabel,
                    RowLabel = rowLabel,
                    SeatNumber = seat,
                    CategoryId = categoryId,
                    IsActive = true
                };
                _context.HallSeats.Add(hallSeat);
            }
        }

        await _context.SaveChangesAsync();

        // Fetch the created hall with categories
        var createdHall = await _context.Halls
            .Include(h => h.SeatCategories)
            .FirstOrDefaultAsync(h => h.Id == hall.Id);

        var hallDto = new HallDto
        {
            Id = createdHall!.Id,
            Name = createdHall.Name,
            BranchName = createdHall.BranchName ?? "",
            TotalRows = createdHall.TotalRows,
            TotalSeatsPerRow = createdHall.TotalSeatsPerRow,
            IsActive = createdHall.IsActive,
            SeatCategories = createdHall.SeatCategories.Select(c => new SeatCategoryDto
            {
                Id = c.Id,
                HallId = c.HallId,
                Name = c.Name,
                FixedExtraPrice = c.FixedExtraPrice
            }).ToList()
        };

        await transaction.CommitAsync();
        return Ok(ApiResponse<HallDto>.SuccessResponse("Hall created", hallDto));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<HallDto>>> UpdateHall(int id, [FromBody] UpdateHallRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.TotalRows is < 1 or > 26 || request.TotalSeatsPerRow is < 1 or > 50 || request.SeatCategories == null || request.SeatCategories.Count == 0 || request.SeatCategories.Any(c => c.FixedExtraPrice < 0 || string.IsNullOrWhiteSpace(c.Name)))
            return BadRequest(ApiResponse<HallDto>.ErrorResponse("Enter a valid hall layout and at least one seat category."));
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var hall = await _context.Halls
            .Include(h => h.SeatCategories)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hall == null)
        {
            return NotFound(ApiResponse<HallDto>.ErrorResponse("Hall not found"));
        }

        // Update hall
        hall.Name = request.Name;
        hall.BranchName = request.BranchName;

        var hasBookingHistory = await _context.SeatReservations.AnyAsync(r => r.Seat.HallId == id)
            || await _context.TicketOrderItems.AnyAsync(i => i.Seat.HallId == id);
        if (hasBookingHistory)
        {
            var sameCategories = request.SeatCategories.Count == hall.SeatCategories.Count
                && request.SeatCategories.Select(c => c.Name).Distinct().Count() == request.SeatCategories.Count
                && request.SeatCategories.All(c => hall.SeatCategories.Any(existing => existing.Name == c.Name));
            if (request.TotalRows != hall.TotalRows || request.TotalSeatsPerRow != hall.TotalSeatsPerRow || !sameCategories)
                return BadRequest(ApiResponse<HallDto>.ErrorResponse("This hall has booking history. Keep its layout and category names; update its details or category prices instead."));
            foreach (var category in hall.SeatCategories)
                category.FixedExtraPrice = request.SeatCategories.Single(c => c.Name == category.Name).FixedExtraPrice;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(ApiResponse<HallDto>.SuccessResponse("Hall updated", new HallDto {
                Id = hall.Id, Name = hall.Name, BranchName = hall.BranchName ?? "", TotalRows = hall.TotalRows,
                TotalSeatsPerRow = hall.TotalSeatsPerRow, IsActive = hall.IsActive,
                SeatCategories = hall.SeatCategories.Select(c => new SeatCategoryDto { Id = c.Id, HallId = c.HallId, Name = c.Name, FixedExtraPrice = c.FixedExtraPrice }).ToList()
            }));
        }

        // Remove existing categories and seats only when no booking references them.
        var existingSeats = await _context.HallSeats.Where(s => s.HallId == id).ToListAsync();
        _context.HallSeats.RemoveRange(existingSeats);

        var existingCategories = await _context.SeatCategories.Where(c => c.HallId == id).ToListAsync();
        _context.SeatCategories.RemoveRange(existingCategories);

        await _context.SaveChangesAsync();

        // Create new seat categories
        var categoryIds = new List<int>();
        foreach (var category in request.SeatCategories)
        {
            var seatCategory = new SeatCategory
            {
                HallId = id,
                Name = category.Name,
                FixedExtraPrice = category.FixedExtraPrice
            };
            _context.SeatCategories.Add(seatCategory);
            await _context.SaveChangesAsync();
            categoryIds.Add(seatCategory.Id);
        }

        // Generate new seats
        var rowLabels = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        for (int row = 0; row < request.TotalRows; row++)
        {
            string rowLabel = rowLabels[row].ToString();
            int categoryIndex = row * categoryIds.Count / request.TotalRows;
            int categoryId = categoryIds[categoryIndex];

            for (int seat = 1; seat <= request.TotalSeatsPerRow; seat++)
            {
                string seatLabel = $"{rowLabel}{seat}";
                var hallSeat = new HallSeat
                {
                    HallId = id,
                    SeatLabel = seatLabel,
                    RowLabel = rowLabel,
                    SeatNumber = seat,
                    CategoryId = categoryId,
                    IsActive = true
                };
                _context.HallSeats.Add(hallSeat);
            }
        }

        hall.TotalRows = request.TotalRows;
        hall.TotalSeatsPerRow = request.TotalSeatsPerRow;

        await _context.SaveChangesAsync();

        var updatedHall = await _context.Halls
            .Include(h => h.SeatCategories)
            .FirstOrDefaultAsync(h => h.Id == id);

        var hallDto = new HallDto
        {
            Id = updatedHall!.Id,
            Name = updatedHall.Name,
            BranchName = updatedHall.BranchName ?? "",
            TotalRows = updatedHall.TotalRows,
            TotalSeatsPerRow = updatedHall.TotalSeatsPerRow,
            IsActive = updatedHall.IsActive,
            SeatCategories = updatedHall.SeatCategories.Select(c => new SeatCategoryDto
            {
                Id = c.Id,
                HallId = c.HallId,
                Name = c.Name,
                FixedExtraPrice = c.FixedExtraPrice
            }).ToList()
        };

        await transaction.CommitAsync();
        return Ok(ApiResponse<HallDto>.SuccessResponse("Hall updated", hallDto));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteHall(int id)
    {
        var hall = await _context.Halls.FindAsync(id);

        if (hall == null)
        {
            return NotFound(ApiResponse<string>.ErrorResponse("Hall not found"));
        }

        // Soft delete - just set IsActive to false
        hall.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse("Hall deleted", ""));
    }

    [HttpPost("seed-seats")]
    public async Task<ActionResult<ApiResponse<string>>> SeedMissingSeats()
    {
        var halls = await _context.Halls
            .Include(h => h.SeatCategories)
            .Where(h => h.IsActive)
            .ToListAsync();

        var seatsCreated = 0;

        foreach (var hall in halls)
        {
            // Check if hall has any seats
            var existingSeats = await _context.HallSeats.Where(s => s.HallId == hall.Id).AnyAsync();

            if (!existingSeats)
            {
                // Get or create seat categories
                var categoryIds = hall.SeatCategories.Select(c => c.Id).ToList();

                if (!categoryIds.Any())
                {
                    // Create default category
                    var defaultCategory = new SeatCategory
                    {
                        HallId = hall.Id,
                        Name = "Regular",
                        FixedExtraPrice = 0
                    };
                    _context.SeatCategories.Add(defaultCategory);
                    await _context.SaveChangesAsync();
                    categoryIds.Add(defaultCategory.Id);
                }

                // Generate seats
                var rowLabels = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

                for (int row = 0; row < hall.TotalRows; row++)
                {
                    string rowLabel = rowLabels[row].ToString();
                    int categoryIndex = categoryIds.Count > 0 ? row * categoryIds.Count / Math.Max(1, hall.TotalRows) : 0;
                    int categoryId = categoryIds.Any() ? categoryIds[categoryIndex] : categoryIds.First();

                    for (int seat = 1; seat <= hall.TotalSeatsPerRow; seat++)
                    {
                        string seatLabel = $"{rowLabel}{seat}";
                        var hallSeat = new HallSeat
                        {
                            HallId = hall.Id,
                            SeatLabel = seatLabel,
                            RowLabel = rowLabel,
                            SeatNumber = seat,
                            CategoryId = categoryId,
                            IsActive = true
                        };
                        _context.HallSeats.Add(hallSeat);
                        seatsCreated++;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse($"Seeding complete. Created {seatsCreated} seats.", ""));
    }
}
