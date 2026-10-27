using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Data;
using CinemaGo.Api.DTOs;
using CinemaGo.Api.Helpers;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly CinemaDbContext _context;

    public MoviesController(CinemaDbContext context)
    {
        _context = context;
    }

    [HttpGet("home")]
    public async Task<ActionResult<ApiResponse<HomeDataDto>>> GetHomeData()
    {
        var movies = await _context.Movies
            .Where(m => m.IsPublished)
            .ToListAsync();

        var homeData = new HomeDataDto
        {
            Featured = movies.Where(m => m.IsFeatured).Take(10).Select(MapToDto).ToList(),
            OttMovies = movies.Where(m => m.MovieType == "OTT" || m.MovieType == "BOTH").Take(10).Select(MapToDto).ToList(),
            CinemaMovies = movies.Where(m => m.MovieType == "CINEMA" || m.MovieType == "BOTH").Take(10).Select(MapToDto).ToList(),
            NowShowing = movies.Where(m => m.MovieType == "CINEMA" || m.MovieType == "BOTH").Take(10).Select(MapToDto).ToList(),
            ComingSoon = movies.Where(m => m.ReleaseDate != null && m.ReleaseDate > DateTime.UtcNow).Take(10).Select(MapToDto).ToList()
        };

        return Ok(ApiResponse<HomeDataDto>.SuccessResponse("Home data fetched", homeData));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MovieDto>>>> GetMovies([FromQuery] MovieSearchRequest request)
    {
        var query = _context.Movies.Where(m => m.IsPublished);

        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(m => m.Title.ToLower().Contains(search) || (m.Description != null && m.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrEmpty(request.MovieType))
        {
            query = query.Where(m => m.MovieType == request.MovieType || m.MovieType == "BOTH");
        }

        if (!string.IsNullOrEmpty(request.Genre))
        {
            query = query.Where(m => m.Genre != null && m.Genre.Contains(request.Genre));
        }

        if (request.IsFeatured == true)
        {
            query = query.Where(m => m.IsFeatured);
        }

        var movies = await query
            .OrderByDescending(m => m.IsFeatured)
            .ThenByDescending(m => m.ReleaseDate)
            .Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Slug = m.Slug,
                Description = m.Description,
                DurationMinutes = m.DurationMinutes,
                ReleaseDate = m.ReleaseDate,
                Language = m.Language,
                Country = m.Country,
                Genre = m.Genre,
                AgeRating = m.AgeRating,
                IMDbLikeScore = m.IMDbLikeScore,
                PosterUrl = m.PosterUrl,
                BannerUrl = m.BannerUrl,
                TrailerUrl = m.TrailerUrl,
                VideoUrl = null,
                MovieType = m.MovieType,
                OttPrice = m.OttPrice,
                IsPublished = m.IsPublished,
                IsFeatured = m.IsFeatured,
                RatingCount = m.Ratings.Count()
            })
            .ToListAsync();

        return Ok(ApiResponse<List<MovieDto>>.SuccessResponse("Movies fetched", movies));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<MovieDetailDto>>> GetMovieBySlug(string slug)
    {
        // Try to find by slug first
        var movie = await _context.Movies
            .Include(m => m.Ratings)
            .FirstOrDefaultAsync(m => m.Slug == slug);

        // If not found by slug, try to find by ID (if slug is a number)
        if (movie == null && int.TryParse(slug, out int movieId))
        {
            movie = await _context.Movies
                .Include(m => m.Ratings)
                .FirstOrDefaultAsync(m => m.Id == movieId);
        }

        // If still not found, try partial slug match
        if (movie == null)
        {
            movie = await _context.Movies
                .Include(m => m.Ratings)
                .FirstOrDefaultAsync(m => m.Slug.Contains(slug));
        }

        // If still not found, try title contains (case insensitive)
        if (movie == null)
        {
            var lowerSlug = slug.ToLower();
            movie = await _context.Movies
                .Include(m => m.Ratings)
                .FirstOrDefaultAsync(m => m.Title.ToLower().Contains(lowerSlug));
        }

        if (movie == null)
        {
            return NotFound(ApiResponse<MovieDetailDto>.ErrorResponse("Movie not found"));
        }

        // Get user ID if authenticated
        int? userId = null;
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int id))
        {
            userId = id;
        }

        bool isPurchased = false;
        int? userRating = null;

        if (userId.HasValue)
        {
            var purchase = await _context.OttPurchases
                .FirstOrDefaultAsync(p => p.UserId == userId && p.MovieId == movie.Id);
            isPurchased = purchase != null;

            var rating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movie.Id);
            if (rating != null)
            {
                userRating = rating.Score;
            }
        }

        var movieDto = new MovieDetailDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Slug = movie.Slug,
            Description = movie.Description,
            DurationMinutes = movie.DurationMinutes,
            ReleaseDate = movie.ReleaseDate,
            Language = movie.Language,
            Country = movie.Country,
            Genre = movie.Genre,
            AgeRating = movie.AgeRating,
            IMDbLikeScore = movie.IMDbLikeScore,
            PosterUrl = movie.PosterUrl ?? "",
            BannerUrl = movie.BannerUrl ?? "",
            TrailerUrl = movie.TrailerUrl ?? "",
            VideoUrl = "",
            MovieType = movie.MovieType,
            OttPrice = movie.OttPrice,
            IsPublished = movie.IsPublished,
            IsFeatured = movie.IsFeatured,
            IsPurchased = isPurchased,
            UserRating = userRating,
            RatingCount = movie.Ratings.Count
        };

        return Ok(ApiResponse<MovieDetailDto>.SuccessResponse("Movie fetched", movieDto));
    }

    private static MovieDto MapToDto(Movie m)
    {
        return new MovieDto
        {
            Id = m.Id,
            Title = m.Title,
            Slug = m.Slug,
            Description = m.Description,
            DurationMinutes = m.DurationMinutes,
            ReleaseDate = m.ReleaseDate,
            Language = m.Language,
            Country = m.Country,
            Genre = m.Genre,
            AgeRating = m.AgeRating,
            IMDbLikeScore = m.IMDbLikeScore,
            PosterUrl = m.PosterUrl,
            BannerUrl = m.BannerUrl,
            TrailerUrl = m.TrailerUrl,
            VideoUrl = null,
            MovieType = m.MovieType,
            OttPrice = m.OttPrice,
            IsPublished = m.IsPublished,
            IsFeatured = m.IsFeatured,
            RatingCount = 0
        };
    }
}
