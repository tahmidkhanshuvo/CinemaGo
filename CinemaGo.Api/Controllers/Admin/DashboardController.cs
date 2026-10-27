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
public class DashboardController : ControllerBase
{
    private readonly CinemaDbContext _context;

    public DashboardController(CinemaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetDashboard()
    {
        var totalMovies = await _context.Movies.CountAsync();
        var totalHalls = await _context.Halls.Where(h => h.IsActive).CountAsync();
        var totalUsers = await _context.Users.CountAsync();

        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var todayStart = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);
        var todayEnd = todayStart.AddDays(1);

        var todayOrders = await _context.TicketOrders
            .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd && o.Status == "Paid")
            .SumAsync(o => o.TotalAmount);

        var monthRevenue = await _context.TicketOrders
            .Where(o => o.CreatedAt >= monthStart && o.Status == "Paid")
            .SumAsync(o => o.TotalAmount);

        var monthOttRevenue = await _context.OttPurchases
            .Where(o => o.PurchasedAt >= monthStart)
            .SumAsync(o => o.Amount);

        var recentOrders = await _context.TicketOrders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                BookingCode = o.BookingCode,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UserName = o.User.FullName
            })
            .ToListAsync();

        var dashboard = new DashboardDto
        {
            TotalMovies = totalMovies,
            TotalHalls = totalHalls,
            TotalUsers = totalUsers,
            TodayTicketRevenue = todayOrders,
            MonthTicketRevenue = monthRevenue,
            MonthOttRevenue = monthOttRevenue,
            MonthTotalRevenue = monthRevenue + monthOttRevenue,
            RecentOrders = recentOrders
        };

        return Ok(ApiResponse<DashboardDto>.SuccessResponse("Dashboard data fetched", dashboard));
    }
}

public class DashboardDto
{
    public int TotalMovies { get; set; }
    public int TotalHalls { get; set; }
    public int TotalUsers { get; set; }
    public decimal TodayTicketRevenue { get; set; }
    public decimal MonthTicketRevenue { get; set; }
    public decimal MonthOttRevenue { get; set; }
    public decimal MonthTotalRevenue { get; set; }
    public List<OrderSummaryDto> RecentOrders { get; set; } = new();
}

public class OrderSummaryDto
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = "";
}
