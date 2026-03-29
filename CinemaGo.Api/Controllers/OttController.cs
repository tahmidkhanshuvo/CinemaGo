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
public class OttController : ControllerBase
{
    private readonly CinemaDbContext _context;
    
    public OttController(CinemaDbContext context)
    {
        _context = context;
    }
    
    [HttpPost("{movieId}/purchase")]
    public async Task<ActionResult<ApiResponse<OttPurchaseDto>>> PurchaseMovie(int movieId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResponse<OttPurchaseDto>.ErrorResponse("Unauthorized"));
        }
        
        // Get movie
        var movie = await _context.Movies.FindAsync(movieId);
        
        if (movie == null)
        {
            return NotFound(ApiResponse<OttPurchaseDto>.ErrorResponse("Movie not found"));
        }
        
        // Check if already purchased
        var existingPurchase = await _context.OttPurchases
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MovieId == movieId);
        
        if (existingPurchase != null)
        {
            return BadRequest(ApiResponse<OttPurchaseDto>.ErrorResponse("Movie already purchased"));
        }
        
        var amount = movie.OttPrice;
        
        // Create payment
        var payment = new Payment
        {
            UserId = userId,
            PaymentType = "OTT",
            ReferenceType = "OttPurchase",
            ReferenceId = 0,
            Amount = amount,
            Method = "DummyCard",
            Status = "Success",
            CreatedAt = DateTime.UtcNow
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        
        // Create purchase
        var purchase = new OttPurchase
        {
            UserId = userId,
            MovieId = movieId,
            Amount = amount,
            PaymentId = payment.Id,
            PurchasedAt = DateTime.UtcNow
        };
        _context.OttPurchases.Add(purchase);
        await _context.SaveChangesAsync();
        
        var purchaseDto = new OttPurchaseDto
        {
            Id = purchase.Id,
            MovieId = movieId,
            MovieTitle = movie.Title,
            MoviePosterUrl = movie.PosterUrl ?? "",
            Amount = amount,
            PurchasedAt = purchase.PurchasedAt
        };
        
        return Ok(ApiResponse<OttPurchaseDto>.SuccessResponse("Purchase successful", purchaseDto));
    }
    
    [HttpGet("purchases")]
    public async Task<ActionResult<ApiResponse<List<OttPurchaseDto>>>> GetPurchases()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResponse<List<OttPurchaseDto>>.ErrorResponse("Unauthorized"));
        }
        
        var purchases = await _context.OttPurchases
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
        
        return Ok(ApiResponse<List<OttPurchaseDto>>.SuccessResponse("Purchases fetched", purchases));
    }
    
    [HttpGet("{movieId}/watch")]
    public async Task<ActionResult<ApiResponse<WatchDto>>> WatchMovie(int movieId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResponse<WatchDto>.ErrorResponse("Unauthorized"));
        }
        
        // Check if purchased
        var purchase = await _context.OttPurchases
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MovieId == movieId);
        
        if (purchase == null)
        {
            return BadRequest(ApiResponse<WatchDto>.ErrorResponse("Movie not purchased"));
        }
        
        var movie = await _context.Movies.FindAsync(movieId);
        
        var watchDto = new WatchDto
        {
            MovieId = movieId,
            MovieTitle = movie?.Title ?? "",
            Title = movie?.Title ?? "",
            VideoUrl = movie?.VideoUrl ?? "",
            PosterUrl = movie?.PosterUrl ?? ""
        };
        
        return Ok(ApiResponse<WatchDto>.SuccessResponse("Watch URL fetched", watchDto));
    }
}