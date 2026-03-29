using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Data;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
public class SeedController : ControllerBase
{
    private readonly CinemaDbContext _context;

    public SeedController(CinemaDbContext context)
    {
        _context = context;
    }

    [HttpPost("recreate")]
    public async Task<IActionResult> RecreateDatabase()
    {
        try
        {
            // Clear existing data and re-seed
            await ClearAndSeedAsync();
            
            return Ok(new { success = true, message = "Database cleared and reseeded successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private async Task ClearAndSeedAsync()
    {
        // Clear all data in correct order (respecting foreign keys)
        _context.TicketOrderItems.RemoveRange(_context.TicketOrderItems);
        _context.TicketOrders.RemoveRange(_context.TicketOrders);
        _context.SeatReservations.RemoveRange(_context.SeatReservations);
        _context.OttPurchases.RemoveRange(_context.OttPurchases);
        _context.Payments.RemoveRange(_context.Payments);
        _context.Ratings.RemoveRange(_context.Ratings);
        _context.ShowTimes.RemoveRange(_context.ShowTimes);
        _context.HallSeats.RemoveRange(_context.HallSeats);
        _context.SeatCategories.RemoveRange(_context.SeatCategories);
        _context.Movies.RemoveRange(_context.Movies);
        _context.Halls.RemoveRange(_context.Halls);
        _context.Users.RemoveRange(_context.Users.Where(u => u.Role == "User"));
        
        await _context.SaveChangesAsync();
        
        // Now seed fresh data
        var initializer = new DatabaseInitializer();
        await initializer.SeedDataAsync(_context);
    }
}