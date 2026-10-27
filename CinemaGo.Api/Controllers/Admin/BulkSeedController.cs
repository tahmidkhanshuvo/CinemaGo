using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Data;
using CinemaGo.Api.DTOs;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class BulkSeedController : ControllerBase
{
    private readonly CinemaDbContext _context;
    private readonly HttpClient _httpClient;

    // Popular movie IDs for seeding (IMDb IDs)
    private static readonly string[] OTT_MOVIE_QUERIES = new[]
    {
        "action", "comedy", "drama", "thriller", "horror", "sci fi", "romance",
        "animation", "adventure", "fantasy", "crime", "mystery", "documentary",
        "marvel", "dc", "batman", "spider man", "iron man", "superhero",
        "star wars", "harry potter", "lord of the rings", "jurassic", "transformers",
        "mission impossible", "fast and furious", "john wick", "bond", "x men",
        "avatar", "inception", "interstellar", "dark knight", "pulp fiction",
        "forrest gump", "shawshank", "fight club", "matrix", "gladiator",
        "titanic", "avatar", "frozen", "toy story", "finding nemo",
        "spider verse", "dark", "walking dead", "game of thrones",
        "breaking bad", "stranger things", "house of dragon", "wednesday",
        "money heist", "squid game", "Witcher", "narcos", "breaking",
        "demon slayer", "anime", "dragon ball", "one piece"
    };

    private static readonly string[] CINEMA_MOVIE_QUERIES = new[]
    {
        "avengers", "dune", "batman", "superhero", "mission impossible",
        "fast x", "john wick", "transformers", "spider man", "no way home",
        "avatar way of water", "oppenheimer", "barbie", "interstellar",
        "inception", "the dark knight", "gladiator", "iron man", "thor",
        "guardians of the galaxy", "doctor strange", "ant man", "black panther"
    };

    // Bangladeshi movie queries
    private static readonly string[] BANGLADESHI_MOVIE_QUERIES = new[]
    {
        "bangladeshi movie 2024", "bangladeshi drama", "bangla movie",
        "hawa", "putro", "shongram", "monpura", "bapjaner bioscope",
        "dhakar baishakh", "bangladeshi comedy", "bangladeshi action"
    };

    // Classic IMDB movies for OTT
    private static readonly string[] CLASSIC_MOVIE_QUERIES = new[]
    {
        "the godfather", "scarface", "goodfellas", "casino", "the departed",
        "heat", "reservoir dogs", "pulp fiction", "kill bill", "inglourious basterds",
        "forrest gump", "shawshank redemption", "the green mile", "fight club",
        "inception", "interstellar", "the dark knight", "gladiator",
        "the matrix", "jurassic park", "back to the future", "star wars",
        "lord of the rings", "harry potter", "the silence of the lambs",
        "the usual suspects", "braveheart", "titanic", "avatar",
        "the lion king", "frozen", "toy story", "finding nemo",
        "the godfather part 2", "schindler list", "12 angry men",
        "one flew over cuckoos nest", "clockwork orange", "apocalypse now"
    };

    public BulkSeedController(CinemaDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient();
    }

    [HttpPost("ott")]
    public async Task<IActionResult> SeedOttMovies([FromQuery] int count = 200)
    {
        try
        {
            var addedMovies = new List<string>();
            var seenIds = new HashSet<string>();

            // Search and add movies from different queries
            foreach (var query in OTT_MOVIE_QUERIES)
            {
                if (addedMovies.Count >= count) break;

                var response = await _httpClient.GetAsync($"https://v2.sg.media-imdb.com/suggestion/x/{query}.json");

                if (!response.IsSuccessStatusCode) continue;

                var data = await response.Content.ReadFromJsonAsync<IMDbApiResponse>();
                if (data?.d == null) continue;

                foreach (var movie in data.d)
                {
                    if (addedMovies.Count >= count) break;
                    if (string.IsNullOrEmpty(movie.id) || movie.id.StartsWith("/")) continue;
                    if (seenIds.Contains(movie.id)) continue;
                    if (movie.y == null || movie.y < 2000) continue;

                    seenIds.Add(movie.id);

                    var newMovie = await AddMovieFromIMDB(movie.id, "OTT", 3.99m);
                    if (newMovie != null)
                    {
                        addedMovies.Add(newMovie);
                    }
                }
            }

            return Ok(new {
                success = true,
                message = $"Added {addedMovies.Count} OTT movies from IMDb",
                movies = addedMovies.Take(20)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("cinema")]
    public async Task<IActionResult> SeedCinemaMovies([FromQuery] int count = 20)
    {
        try
        {
            var addedMovies = new List<string>();
            var seenIds = new HashSet<string>();

            foreach (var query in CINEMA_MOVIE_QUERIES)
            {
                if (addedMovies.Count >= count) break;

                var response = await _httpClient.GetAsync($"https://v2.sg.media-imdb.com/suggestion/x/{query}.json");

                if (!response.IsSuccessStatusCode) continue;

                var data = await response.Content.ReadFromJsonAsync<IMDbApiResponse>();
                if (data?.d == null) continue;

                foreach (var movie in data.d)
                {
                    if (addedMovies.Count >= count) break;
                    if (string.IsNullOrEmpty(movie.id) || movie.id.StartsWith("/")) continue;
                    if (seenIds.Contains(movie.id)) continue;
                    if (movie.y == null || movie.y < 2010) continue;

                    seenIds.Add(movie.id);

                    var newMovie = await AddMovieFromIMDB(movie.id, "CINEMA", 0);
                    if (newMovie != null)
                    {
                        addedMovies.Add(newMovie);
                    }
                }
            }

            return Ok(new {
                success = true,
                message = $"Added {addedMovies.Count} CINEMA movies from IMDb",
                movies = addedMovies
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("bangladeshi")]
    public async Task<IActionResult> SeedBangladeshiMovies([FromQuery] int count = 15)
    {
        try
        {
            var addedMovies = new List<string>();
            var seenIds = new HashSet<string>();

            foreach (var query in BANGLADESHI_MOVIE_QUERIES)
            {
                if (addedMovies.Count >= count) break;

                var response = await _httpClient.GetAsync($"https://v2.sg.media-imdb.com/suggestion/x/{query}.json");

                if (!response.IsSuccessStatusCode) continue;

                var data = await response.Content.ReadFromJsonAsync<IMDbApiResponse>();
                if (data?.d == null) continue;

                foreach (var movie in data.d)
                {
                    if (addedMovies.Count >= count) break;
                    if (string.IsNullOrEmpty(movie.id) || movie.id.StartsWith("/")) continue;
                    if (seenIds.Contains(movie.id)) continue;
                    if (movie.y == null || movie.y < 2015) continue;

                    seenIds.Add(movie.id);

                    var newMovie = await AddMovieFromIMDB(movie.id, "CINEMA", 0);
                    if (newMovie != null)
                    {
                        // Update language and country for Bangladeshi movies
                        var movieToUpdate = await _context.Movies.FirstOrDefaultAsync(m => m.Title == newMovie);
                        if (movieToUpdate != null)
                        {
                            movieToUpdate.Language = "Bengali";
                            movieToUpdate.Country = "Bangladesh";
                            await _context.SaveChangesAsync();
                        }
                        addedMovies.Add(newMovie);
                    }
                }
            }

            return Ok(new {
                success = true,
                message = $"Added {addedMovies.Count} Bangladeshi movies from IMDb",
                movies = addedMovies
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("classic")]
    public async Task<IActionResult> SeedClassicMovies([FromQuery] int count = 30)
    {
        try
        {
            var addedMovies = new List<string>();
            var seenIds = new HashSet<string>();

            foreach (var query in CLASSIC_MOVIE_QUERIES)
            {
                if (addedMovies.Count >= count) break;

                var response = await _httpClient.GetAsync($"https://v2.sg.media-imdb.com/suggestion/x/{query}.json");

                if (!response.IsSuccessStatusCode) continue;

                var data = await response.Content.ReadFromJsonAsync<IMDbApiResponse>();
                if (data?.d == null) continue;

                foreach (var movie in data.d)
                {
                    if (addedMovies.Count >= count) break;
                    if (string.IsNullOrEmpty(movie.id) || movie.id.StartsWith("/")) continue;
                    if (seenIds.Contains(movie.id)) continue;
                    if (movie.y == null || movie.y < 1970 || movie.y > 2010) continue;

                    seenIds.Add(movie.id);

                    var newMovie = await AddMovieFromIMDB(movie.id, "OTT", 4.99m);
                    if (newMovie != null)
                    {
                        addedMovies.Add(newMovie);
                    }
                }
            }

            return Ok(new {
                success = true,
                message = $"Added {addedMovies.Count} Classic movies from IMDb",
                movies = addedMovies
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("halls")]
    public async Task<IActionResult> CreateBulkHalls([FromBody] BulkCreateHallsRequest request)
    {
        try
        {
            var createdHalls = new List<object>();

            foreach (var hallRequest in request.Halls)
            {
                var hall = new Hall
                {
                    Name = hallRequest.Name,
                    BranchName = hallRequest.BranchName,
                    TotalRows = hallRequest.TotalRows,
                    TotalSeatsPerRow = hallRequest.TotalSeatsPerRow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Halls.Add(hall);
                await _context.SaveChangesAsync();

                var categoryIds = new List<int>();
                var categoriesToCreate = hallRequest.SeatCategories?.Any() == true
                    ? hallRequest.SeatCategories
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

                var rowLabels = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

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

                for (int row = 0; row < hallRequest.TotalRows; row++)
                {
                    string rowLabel = rowLabels[row].ToString();
                    int categoryIndex = categoryIds.Count > 0 ? row * categoryIds.Count / Math.Max(1, hallRequest.TotalRows) : 0;
                    int categoryId = categoryIds.Any() ? categoryIds[categoryIndex] : categoryIds.First();

                    for (int seat = 1; seat <= hallRequest.TotalSeatsPerRow; seat++)
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

                createdHalls.Add(new { hall.Id, hall.Name, hall.BranchName });
            }

            return Ok(new {
                success = true,
                message = $"Created {createdHalls.Count} halls",
                halls = createdHalls
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost("showtimes")]
    public async Task<IActionResult> CreateBulkShowTimes([FromBody] BulkCreateShowTimesRequest request)
    {
        try
        {
            var createdShowTimes = new List<object>();

            foreach (var showTimeRequest in request.ShowTimes)
            {
                var movie = await _context.Movies.FindAsync(showTimeRequest.MovieId);
                if (movie == null) continue;

                var hall = await _context.Halls.FindAsync(showTimeRequest.HallId);
                if (hall == null) continue;

                var startTime = showTimeRequest.StartTime;
                var endTime = startTime.Add(TimeSpan.FromMinutes(showTimeRequest.DurationMinutes));

                var showDateUtc = showTimeRequest.ShowDate.Kind == DateTimeKind.Utc
                    ? showTimeRequest.ShowDate
                    : showTimeRequest.ShowDate.ToUniversalTime();

                var showTime = new ShowTime
                {
                    MovieId = showTimeRequest.MovieId,
                    HallId = showTimeRequest.HallId,
                    ShowDate = showDateUtc,
                    StartTime = startTime,
                    EndTime = endTime,
                    BasePrice = showTimeRequest.BasePrice,
                    Status = showTimeRequest.Status,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ShowTimes.Add(showTime);
                await _context.SaveChangesAsync();

                createdShowTimes.Add(new { showTime.Id, showTime.MovieId, showTime.HallId, showTime.ShowDate, showTime.StartTime });
            }

            return Ok(new {
                success = true,
                message = $"Created {createdShowTimes.Count} showtimes",
                showTimes = createdShowTimes
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    private async Task<string?> AddMovieFromIMDB(string imdbId, string movieType, decimal ottPrice)
    {
        try
        {
            // Skip duplicate check for now - movies will be added regardless
            var response = await _httpClient.GetAsync($"https://v2.sg.media-imdb.com/suggestion/x/{imdbId}.json");
            if (!response.IsSuccessStatusCode) return null;

            var data = await response.Content.ReadFromJsonAsync<IMDbApiResponse>();
            if (data?.d == null || data.d.Count == 0) return null;

            var movie = data.d[0];

            string slug = movie.l.ToLower()
                .Replace(" ", "-")
                .Replace(":", "")
                .Replace("'", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("tt", "");

            int year = movie.y ?? DateTime.Now.Year;
            var releaseDate = DateTime.SpecifyKind(new DateTime(year, 1, 1), DateTimeKind.Utc);

            var newMovie = new Movie
            {
                Title = movie.l,
                Slug = slug,
                Description = movie.s ?? "",
                DurationMinutes = 120,
                ReleaseDate = releaseDate,
                Language = "English",
                Country = "USA",
                Genre = "",
                AgeRating = "PG-13",
                IMDbLikeScore = null,
                PosterUrl = movie.i?.imageUrl ?? "",
                BannerUrl = movie.i?.imageUrl ?? "",
                TrailerUrl = "",
                VideoUrl = "",
                MovieType = movieType,
                OttPrice = ottPrice,
                IsPublished = true,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Movies.Add(newMovie);
            await _context.SaveChangesAsync();

            return movie.l;
        }
        catch
        {
            return null;
        }
    }
}

// Request/Response classes for bulk operations
public class BulkCreateHallsRequest
{
    public List<CreateHallWithBranchRequest> Halls { get; set; } = new();
}

public class CreateHallWithBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int TotalSeatsPerRow { get; set; }
    public List<CreateSeatCategoryRequest> SeatCategories { get; set; } = new();
}

public class BulkCreateShowTimesRequest
{
    public List<CreateShowTimeBulkRequest> ShowTimes { get; set; } = new();
}

public class CreateShowTimeBulkRequest
{
    public int MovieId { get; set; }
    public int HallId { get; set; }
    public DateTime ShowDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = "Scheduled";
}

// IMDb API Response models (shared)
public class IMDbApiResponse
{
    public List<IMDBMovie> d { get; set; } = new();
}

public class IMDBMovie
{
    public string id { get; set; } = "";
    public string l { get; set; } = "";
    public int? y { get; set; }
    public string s { get; set; } = "";
    public string qid { get; set; } = "";
    public IMDBImage? i { get; set; }
}

public class IMDBImage
{
    public string imageUrl { get; set; } = "";
}
