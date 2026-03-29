using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Data;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
public class ImdbController : ControllerBase
{
    private readonly CinemaDbContext _context;
    private readonly HttpClient _httpClient;

    public ImdbController(CinemaDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient();
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMovies([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { success = false, message = "Query is required" });

        try
        {
            // Use IMDb suggestion API (unofficial but reliable)
            var response = await _httpClient.GetAsync($"https://v2.sg.media-imdb.com/suggestion/x/{query}.json");
            
            if (!response.IsSuccessStatusCode)
                return StatusCode(500, new { success = false, message = $"Failed to fetch: {response.StatusCode}" });

            var data = await response.Content.ReadFromJsonAsync<IMDbApiResponse>();
            
            if (data?.d == null)
                return Ok(new { success = true, data = new List<object>() });

            var movies = data.d.Select(m => new
            {
                id = m.id,
                title = m.l,
                image = m.i?.imageUrl,
                year = m.y
            }).Where(m => !string.IsNullOrEmpty(m.title)).ToList();

            return Ok(new { success = true, data = movies });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("movie/{imdbId}")]
    public async Task<IActionResult> GetMovieDetails(string imdbId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://v2.sg.media-imdb.com/suggestion/x/{imdbId}.json");
            
            if (!response.IsSuccessStatusCode)
                return StatusCode(500, new { success = false, message = "Failed to fetch movie details" });

            var data = await response.Content.ReadFromJsonAsync<IMDbApiResponse>();
            
            if (data?.d == null || data.d.Count == 0)
                return NotFound(new { success = false, message = "Movie not found" });

            var m = data.d[0];
            var movieData = new
            {
                id = m.id,
                title = m.l,
                image = m.i?.imageUrl,
                year = m.y,
                plot = m.s,
                type = m.qid
            };

            return Ok(new { success = true, data = movieData });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddMovie([FromBody] AddMovieRequest request)
    {
        try
        {
            // Get movie details from IMDb suggestion API
            var response = await _httpClient.GetAsync($"https://v2.sg.media-imdb.com/suggestion/x/{request.ImdbId}.json");
            
            if (!response.IsSuccessStatusCode)
                return StatusCode(500, new { success = false, message = "Failed to fetch movie details from IMDb" });

            var data = await response.Content.ReadFromJsonAsync<IMDbApiResponse>();
            
            if (data?.d == null || data.d.Count == 0)
                return NotFound(new { success = false, message = "Movie not found on IMDb" });

            var imdbMovie = data.d[0];

            // Generate slug
            string slug = imdbMovie.l.ToLower()
                .Replace(" ", "-")
                .Replace(":", "")
                .Replace("'", "")
                .Replace("(", "")
                .Replace(")", "");

            // Extract year
            int year = imdbMovie.y ?? DateTime.Now.Year;
            var releaseDate = DateTime.SpecifyKind(new DateTime(year, 1, 1), DateTimeKind.Utc);

            // Parse runtime from id (approximate)
            // The id format is like "tt1234567" - we can't get runtime from this API
            
            // Create movie entity
            var movie = new Movie
            {
                Title = imdbMovie.l,
                Slug = slug,
                Description = imdbMovie.s ?? "", // Plot/synopsis
                DurationMinutes = 120, // Default, admin can update
                ReleaseDate = releaseDate,
                Language = "English",
                Country = "USA",
                Genre = "",
                AgeRating = "PG-13",
                IMDbLikeScore = null,
                PosterUrl = imdbMovie.i?.imageUrl ?? "",
                BannerUrl = imdbMovie.i?.imageUrl ?? "",
                TrailerUrl = "",
                VideoUrl = "",
                MovieType = request.MovieType,
                OttPrice = request.OttPrice,
                IsPublished = true,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Added '{movie.Title}' to database", data = new { movie.Id, movie.Title, movie.MovieType } });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

// Request model
public class AddMovieRequest
{
    public string ImdbId { get; set; } = "";
    public string MovieType { get; set; } = "CINEMA";
    public decimal OttPrice { get; set; } = 4.99m;
}