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
public class RatingsController : ControllerBase
{
    private readonly CinemaDbContext _context;
    
    public RatingsController(CinemaDbContext context)
    {
        _context = context;
    }
    
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RatingDto>>> AddRating([FromBody] AddRatingRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized(ApiResponse<RatingDto>.ErrorResponse("Unauthorized"));
        }
        
        // Get movie
        var movie = await _context.Movies.FindAsync(request.MovieId);
        
        if (movie == null)
        {
            return NotFound(ApiResponse<RatingDto>.ErrorResponse("Movie not found"));
        }
        
        // Check for existing rating
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == request.MovieId);
        
        if (existingRating != null)
        {
            existingRating.Score = request.Score;
            existingRating.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var rating = new Rating
            {
                UserId = userId,
                MovieId = request.MovieId,
                Score = request.Score,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Ratings.Add(rating);
        }
        
        await _context.SaveChangesAsync();
        
        // Update movie average score
        var ratings = await _context.Ratings
            .Where(r => r.MovieId == request.MovieId)
            .ToListAsync();
        
        if (ratings.Any())
        {
            movie.IMDbLikeScore = ratings.Average(r => r.Score);
            movie.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        
        var ratingDto = new RatingDto
        {
            Id = existingRating?.Id ?? 0,
            UserId = userId,
            MovieId = request.MovieId,
            Score = request.Score,
            CreatedAt = DateTime.UtcNow
        };
        
        return Ok(ApiResponse<RatingDto>.SuccessResponse("Rating added", ratingDto));
    }
    
    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<ApiResponse<List<RatingDto>>>> GetMovieRatings(int movieId)
    {
        var ratings = await _context.Ratings
            .Include(r => r.User)
            .Where(r => r.MovieId == movieId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User.FullName,
                MovieId = r.MovieId,
                Score = r.Score,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
        
        return Ok(ApiResponse<List<RatingDto>>.SuccessResponse("Ratings fetched", ratings));
    }
}