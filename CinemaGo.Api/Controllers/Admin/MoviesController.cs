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
public class MoviesController : ControllerBase
{
    private readonly CinemaDbContext _context;

    public MoviesController(CinemaDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MovieDto>>>> GetMovies()
    {
        var movies = await _context.Movies
            .OrderByDescending(m => m.CreatedAt)
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
                PosterUrl = m.PosterUrl ?? "",
                BannerUrl = m.BannerUrl ?? "",
                TrailerUrl = m.TrailerUrl ?? "",
                VideoUrl = m.VideoUrl ?? "",
                MovieType = m.MovieType,
                OttPrice = m.OttPrice,
                IsPublished = m.IsPublished,
                IsFeatured = m.IsFeatured,
                RatingCount = m.Ratings.Count()
            })
            .ToListAsync();

        return Ok(ApiResponse<List<MovieDto>>.SuccessResponse("Movies fetched", movies));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MovieDto>>> GetMovie(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
        {
            return NotFound(ApiResponse<MovieDto>.ErrorResponse("Movie not found"));
        }

        var movieDto = new MovieDto
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
            VideoUrl = movie.VideoUrl ?? "",
            MovieType = movie.MovieType,
            OttPrice = movie.OttPrice,
            IsPublished = movie.IsPublished,
            IsFeatured = movie.IsFeatured,
            RatingCount = movie.Ratings.Count()
        };

        return Ok(ApiResponse<MovieDto>.SuccessResponse("Movie fetched", movieDto));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MovieDto>>> CreateMovie([FromBody] CreateMovieRequest request)
    {
        // Generate slug
        var slug = request.Title.ToLower().Replace(" ", "-").Replace(":", "").Replace("'", "");

        // Check for duplicate slug
        var existingMovie = await _context.Movies.FirstOrDefaultAsync(m => m.Slug == slug);

        if (existingMovie != null)
        {
            return BadRequest(ApiResponse<MovieDto>.ErrorResponse("A movie with similar title already exists"));
        }

        var movie = new Movie
        {
            Title = request.Title,
            Slug = slug,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            ReleaseDate = request.ReleaseDate,
            Language = request.Language,
            Country = request.Country,
            Genre = request.Genre,
            AgeRating = request.AgeRating,
            IMDbLikeScore = request.IMDbLikeScore,
            PosterUrl = request.PosterUrl,
            BannerUrl = request.BannerUrl,
            TrailerUrl = request.TrailerUrl,
            VideoUrl = request.VideoUrl,
            MovieType = request.MovieType,
            OttPrice = request.OttPrice,
            IsPublished = request.IsPublished,
            IsFeatured = request.IsFeatured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        var movieDto = new MovieDto
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
            VideoUrl = movie.VideoUrl ?? "",
            MovieType = movie.MovieType,
            OttPrice = movie.OttPrice,
            IsPublished = movie.IsPublished,
            IsFeatured = movie.IsFeatured,
            RatingCount = 0
        };

        return Ok(ApiResponse<MovieDto>.SuccessResponse("Movie created", movieDto));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MovieDto>>> UpdateMovie(int id, [FromBody] UpdateMovieRequest request)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
        {
            return NotFound(ApiResponse<MovieDto>.ErrorResponse("Movie not found"));
        }

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.DurationMinutes = request.DurationMinutes;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Language = request.Language;
        movie.Country = request.Country;
        movie.Genre = request.Genre;
        movie.AgeRating = request.AgeRating;
        movie.IMDbLikeScore = request.IMDbLikeScore;
        movie.PosterUrl = request.PosterUrl;
        movie.BannerUrl = request.BannerUrl;
        movie.TrailerUrl = request.TrailerUrl;
        movie.VideoUrl = request.VideoUrl;
        movie.MovieType = request.MovieType;
        movie.OttPrice = request.OttPrice;
        movie.IsPublished = request.IsPublished;
        movie.IsFeatured = request.IsFeatured;
        movie.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var movieDto = new MovieDto
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
            VideoUrl = movie.VideoUrl ?? "",
            MovieType = movie.MovieType,
            OttPrice = movie.OttPrice,
            IsPublished = movie.IsPublished,
            IsFeatured = movie.IsFeatured,
            RatingCount = movie.Ratings.Count()
        };

        return Ok(ApiResponse<MovieDto>.SuccessResponse("Movie updated", movieDto));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteMovie(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
        {
            return NotFound(ApiResponse<string>.ErrorResponse("Movie not found"));
        }

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse("Movie deleted", ""));
    }
}
