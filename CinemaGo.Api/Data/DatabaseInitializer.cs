using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Data;

/// <summary>
/// Database initializer that creates tables and seeds demo data using EF Core
/// </summary>
public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;

    public DatabaseInitializer(IConfiguration configuration) => _configuration = configuration;

    public async Task InitializeAsync(CinemaDbContext context)
    {
        // Ensure database is created and migrations are applied
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (_configuration.GetValue<bool>("Database:SeedDemoData") && !await context.Users.AnyAsync())
        {
            // Seed demo data
            await SeedDataAsync(context);
        }
    }

    public async Task SeedDataAsync(CinemaDbContext context)
    {
        // Always seed users (but check for admin)
        if (!await context.Users.AnyAsync(u => u.Role == "Admin"))
        {
            await SeedUsersAsync(context);
        }

        // Always seed halls if not exist
        if (!await context.Halls.AnyAsync())
        {
            await SeedHallsAsync(context);
        }

        // Always seed movies if not exist
        if (!await context.Movies.AnyAsync())
        {
            await SeedMoviesAsync(context);
        }

        // Always seed showtimes
        if (!await context.ShowTimes.AnyAsync())
        {
            await SeedShowTimesAsync(context);
        }

        // Add more data even if initial data exists (for expansion)
        await SeedAdditionalHallsAsync(context);
        await SeedAdditionalMoviesAsync(context);
        await SeedAdditionalShowTimesAsync(context);

        await context.SaveChangesAsync();
    }

    private async Task SeedUsersAsync(CinemaDbContext context)
    {
        var email = _configuration["Bootstrap:AdminEmail"];
        var password = _configuration["Bootstrap:AdminPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 12)
            throw new InvalidOperationException("Demo seeding requires Bootstrap__AdminEmail and a password of at least 12 characters.");
        // Admin user
        context.Users.Add(new User
        {
            FullName = "Admin User",
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Phone = "+1234567890",
            Role = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    private async Task SeedHallsAsync(CinemaDbContext context)
    {
        // Hall 1 - Main Hall
        context.Halls.Add(new Hall
        {
            Name = "Main Hall",
            BranchName = "CinemaGo Central",
            TotalRows = 8,
            TotalSeatsPerRow = 12,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Hall 2 - Premium Hall
        context.Halls.Add(new Hall
        {
            Name = "Premium Hall",
            BranchName = "CinemaGo Central",
            TotalRows = 6,
            TotalSeatsPerRow = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Hall 3 - VIP Hall
        context.Halls.Add(new Hall
        {
            Name = "VIP Hall",
            BranchName = "CinemaGo Central",
            TotalRows = 5,
            TotalSeatsPerRow = 8,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        // Create seat categories for each hall
        var categories = new[]
        {
            new { HallId = 1, Name = "Regular", Price = 0m },
            new { HallId = 1, Name = "Premium", Price = 150m },
            new { HallId = 1, Name = "VIP", Price = 300m },
            new { HallId = 2, Name = "Regular", Price = 100m },
            new { HallId = 2, Name = "Premium", Price = 250m },
            new { HallId = 2, Name = "VIP", Price = 400m },
            new { HallId = 3, Name = "Regular", Price = 200m },
            new { HallId = 3, Name = "Premium", Price = 350m },
            new { HallId = 3, Name = "VIP", Price = 500m }
        };

        foreach (var cat in categories)
        {
            context.SeatCategories.Add(new SeatCategory
            {
                HallId = cat.HallId,
                Name = cat.Name,
                FixedExtraPrice = cat.Price
            });
        }

        await context.SaveChangesAsync();

        // Generate seats for each hall
        var hallConfigs = new[] { (HallId: 1, Rows: 8, SeatsPerRow: 12), (HallId: 2, Rows: 6, SeatsPerRow: 10), (HallId: 3, Rows: 5, SeatsPerRow: 8) };

        var rowLabels = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        foreach (var (hallId, rows, seatsPerRow) in hallConfigs)
        {
            for (int row = 0; row < rows; row++)
            {
                string rowLabel = rowLabels[row].ToString();
                int categoryId = hallId switch
                {
                    1 when row >= 6 => 3, // VIP rows
                    1 when row >= 4 => 2, // Premium rows
                    1 => 1, // Regular
                    2 when row >= 4 => 6, // VIP
                    2 when row >= 2 => 5, // Premium
                    2 => 4, // Regular
                    3 when row >= 3 => 9, // VIP
                    3 when row >= 1 => 8, // Premium
                    3 => 7, // Regular
                    _ => 1
                };

                for (int seat = 1; seat <= seatsPerRow; seat++)
                {
                    string seatLabel = $"{rowLabel}{seat}";
                    context.HallSeats.Add(new HallSeat
                    {
                        HallId = hallId,
                        SeatLabel = seatLabel,
                        RowLabel = rowLabel,
                        SeatNumber = seat,
                        CategoryId = categoryId,
                        IsActive = true
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedMoviesAsync(CinemaDbContext context)
    {
        // OTT Movies
        var ottMovies = new[]
        {
            new
            {
                Title = "The Last Journey",
                Description = "A thrilling sci-fi adventure about humanity's first interstellar voyage to find a new home.",
                DurationMinutes = 148,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 6, 15), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Sci-Fi, Adventure",
                AgeRating = "PG-13",
                IMDbLikeScore = 8.5,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster1.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner1.jpg",
                TrailerUrl = "https://youtube.com/watch?v=example1",
                VideoUrl = "https://stream.example.com/last-journey",
                OttPrice = 4.99m
            },
            new
            {
                Title = "Love in Paris",
                Description = "A romantic drama about two strangers who meet in the city of lights and find unexpected love.",
                DurationMinutes = 122,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 2, 10), DateTimeKind.Utc),
                Language = "English",
                Country = "France",
                Genre = "Romance, Drama",
                AgeRating = "PG-13",
                IMDbLikeScore = 7.8,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster2.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner2.jpg",
                TrailerUrl = "https://youtube.com/watch?v=example2",
                VideoUrl = "https://stream.example.com/love-in-paris",
                OttPrice = 3.99m
            },
            new
            {
                Title = "Mystery at Midnight",
                Description = "A gripping mystery thriller where a detective must solve a series of crimes before midnight.",
                DurationMinutes = 135,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 8, 22), DateTimeKind.Utc),
                Language = "English",
                Country = "UK",
                Genre = "Mystery, Thriller",
                AgeRating = "R",
                IMDbLikeScore = 8.2,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster3.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner3.jpg",
                TrailerUrl = "https://youtube.com/watch?v=example3",
                VideoUrl = "https://stream.example.com/mystery-midnight",
                OttPrice = 4.49m
            },
            new
            {
                Title = "Comedy Night Live",
                Description = "A hilarious comedy featuring stand-up performances from the world's best comedians.",
                DurationMinutes = 98,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 5, 1), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Comedy",
                AgeRating = "PG-13",
                IMDbLikeScore = 7.5,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster4.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner4.jpg",
                TrailerUrl = "https://youtube.com/watch?v=example4",
                VideoUrl = "https://stream.example.com/comedy-night",
                OttPrice = 2.99m
            },
            new
            {
                Title = "Documentary: Ocean Depths",
                Description = "An breathtaking documentary exploring the mysterious depths of our oceans.",
                DurationMinutes = 112,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 3, 15), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Documentary",
                AgeRating = "G",
                IMDbLikeScore = 9.0,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster5.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner5.jpg",
                TrailerUrl = "https://youtube.com/watch?v=example5",
                VideoUrl = "https://stream.example.com/ocean-depths",
                OttPrice = 3.49m
            },
            new
            {
                Title = "Action Hero Returns",
                Description = "The action-packed sequel where the hero returns to save the city from destruction.",
                DurationMinutes = 156,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 7, 4), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Action, Adventure",
                AgeRating = "R",
                IMDbLikeScore = 7.9,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster6.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner6.jpg",
                TrailerUrl = "https://youtube.com/watch?v=example6",
                VideoUrl = "https://stream.example.com/action-hero",
                OttPrice = 4.99m
            }
        };

        int movieId = 1;
        foreach (var movie in ottMovies)
        {
            string slug = movie.Title.ToLower().Replace(" ", "-").Replace(":", "").Replace("'", "");
            context.Movies.Add(new Movie
            {
                Title = movie.Title,
                Slug = slug,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes,
                ReleaseDate = movie.ReleaseDate,
                Language = movie.Language,
                Country = movie.Country,
                Genre = movie.Genre,
                AgeRating = movie.AgeRating,
                IMDbLikeScore = movie.IMDbLikeScore,
                PosterUrl = movie.PosterUrl,
                BannerUrl = movie.BannerUrl,
                TrailerUrl = movie.TrailerUrl,
                VideoUrl = movie.VideoUrl,
                MovieType = "OTT",
                OttPrice = movie.OttPrice,
                IsPublished = true,
                IsFeatured = movieId <= 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            movieId++;
        }

        // Cinema Movies
        var cinemaMovies = new[]
        {
            new
            {
                Title = "Avengers: Endgame 2",
                Description = "The epic conclusion to the superhero saga as Earth's mightiest heroes face their greatest threat yet.",
                DurationMinutes = 182,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2025, 5, 1), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Superhero, Action",
                AgeRating = "PG-13",
                IMDbLikeScore = 9.2,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster7.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner7.jpg",
                TrailerUrl = "https://youtube.com/watch?v=avengers2"
            },
            new
            {
                Title = "Dune: Part Three",
                Description = "Paul Atreides continues his journey through the desert planet of Arrakis.",
                DurationMinutes = 165,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2025, 11, 15), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Sci-Fi, Adventure",
                AgeRating = "PG-13",
                IMDbLikeScore = 8.9,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster8.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner8.jpg",
                TrailerUrl = "https://youtube.com/watch?v=dune3"
            },
            new
            {
                Title = "The Batman Returns",
                Description = "The Dark Knight returns to fight crime in Gotham City with new allies and enemies.",
                DurationMinutes = 175,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2025, 3, 20), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Superhero, Action",
                AgeRating = "PG-13",
                IMDbLikeScore = 8.7,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster9.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner9.jpg",
                TrailerUrl = "https://youtube.com/watch?v=batman2"
            },
            new
            {
                Title = "Fast & Furious 12",
                Description = "Dom and his crew face their most dangerous adversary yet in this high-octane action film.",
                DurationMinutes = 145,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2025, 8, 15), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Action, Adventure",
                AgeRating = "PG-13",
                IMDbLikeScore = 7.5,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster10.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner10.jpg",
                TrailerUrl = "https://youtube.com/watch?v=fast12"
            },
            new
            {
                Title = "Avatar 3: The Way of Water",
                Description = "Jake Sully continues his journey on Pandora in this visually stunning sequel.",
                DurationMinutes = 190,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2025, 12, 20), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Sci-Fi, Fantasy",
                AgeRating = "PG-13",
                IMDbLikeScore = 9.0,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster11.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner11.jpg",
                TrailerUrl = "https://youtube.com/watch?v=avatar3"
            },
            new
            {
                Title = "Spider-Man: Across the Multiverse",
                Description = "Spider-Man travels across dimensions to save the multiverse from destruction.",
                DurationMinutes = 140,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2025, 7, 10), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Superhero, Animation",
                AgeRating = "PG",
                IMDbLikeScore = 8.8,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster12.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner12.jpg",
                TrailerUrl = "https://youtube.com/watch?v=spiderman3"
            },
            new
            {
                Title = "Star Wars: Rogue Squadron",
                Description = "A new generation of star pilots fight in the iconic space battles.",
                DurationMinutes = 155,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2025, 6, 5), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Sci-Fi, Action",
                AgeRating = "PG-13",
                IMDbLikeScore = 8.4,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster13.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner13.jpg",
                TrailerUrl = "https://youtube.com/watch?v=starwars"
            },
            new
            {
                Title = "Jurassic World: Dominion",
                Description = "Dinosaurs roam freely on Earth as humans and prehistoric creatures coexist.",
                DurationMinutes = 148,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2025, 4, 25), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = "Adventure, Sci-Fi",
                AgeRating = "PG-13",
                IMDbLikeScore = 7.6,
                PosterUrl = "https://image.tmdb.org/t/p/w500/poster14.jpg",
                BannerUrl = "https://image.tmdb.org/t/p/original/banner14.jpg",
                TrailerUrl = "https://youtube.com/watch?v=jurassic3"
            }
        };

        foreach (var movie in cinemaMovies)
        {
            string slug = movie.Title.ToLower().Replace(" ", "-").Replace(":", "").Replace("'", "");
            context.Movies.Add(new Movie
            {
                Title = movie.Title,
                Slug = slug,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes,
                ReleaseDate = movie.ReleaseDate,
                Language = movie.Language,
                Country = movie.Country,
                Genre = movie.Genre,
                AgeRating = movie.AgeRating,
                IMDbLikeScore = movie.IMDbLikeScore,
                PosterUrl = movie.PosterUrl,
                BannerUrl = movie.BannerUrl,
                TrailerUrl = movie.TrailerUrl,
                MovieType = "CINEMA",
                OttPrice = 0,
                IsPublished = true,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task SeedShowTimesAsync(CinemaDbContext context)
    {
        // Get cinema movies (ids 7-14)
        var cinemaMovies = await context.Movies.Where(m => m.MovieType == "CINEMA").ToListAsync();

        // Get all halls
        var halls = await context.Halls.ToListAsync();

        // Create showtimes for the next 7 days
        var baseDate = DateTime.UtcNow.Date;

        foreach (var movie in cinemaMovies)
        {
            foreach (var hall in halls)
            {
                // Multiple showtimes per day
                var showTimes = new[]
                {
                    new TimeSpan(10, 30, 0),
                    new TimeSpan(14, 0, 0),
                    new TimeSpan(17, 30, 0),
                    new TimeSpan(21, 0, 0)
                };

                for (int day = 0; day < 7; day++)
                {
                    foreach (var startTime in showTimes)
                    {
                        var showDate = baseDate.AddDays(day);
                        var duration = movie.DurationMinutes ?? 120;
                        var endTime = startTime.Add(TimeSpan.FromMinutes(duration));

                        // Base price depends on hall
                        var basePrice = hall.Name switch
                        {
                            "Main Hall" => 300m,
                            "Premium Hall" => 400m,
                            "VIP Hall" => 500m,
                            _ => 300m
                        };

                        context.ShowTimes.Add(new ShowTime
                        {
                            MovieId = movie.Id,
                            HallId = hall.Id,
                            ShowDate = showDate,
                            StartTime = startTime,
                            EndTime = endTime,
                            BasePrice = basePrice,
                            Status = day == 0 && startTime <= DateTime.UtcNow.TimeOfDay ? "Running" : "Scheduled",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }

    // Additional seeding methods for expanding data
    private async Task SeedAdditionalHallsAsync(CinemaDbContext context)
    {
        // Add Star Cineplex branches
        var starCineplexHalls = new[]
        {
            new { Name = "Gold Hall 1", BranchName = "Star Cineplex Bashundhara", Rows = 8, SeatsPerRow = 12 },
            new { Name = "Gold Hall 2", BranchName = "Star Cineplex Bashundhara", Rows = 8, SeatsPerRow = 12 },
            new { Name = "Platinum Hall", BranchName = "Star Cineplex Bashundhara", Rows = 6, SeatsPerRow = 10 },
            new { Name = "Diamond Hall", BranchName = "Star Cineplex Jamuna", Rows = 6, SeatsPerRow = 10 },
            new { Name = "Gold Hall", BranchName = "Star Cineplex Jamuna", Rows = 8, SeatsPerRow = 12 },
            new { Name = "Ruby Hall", BranchName = "Star Cineplex New Market", Rows = 5, SeatsPerRow = 10 },
            new { Name = "Emerald Hall", BranchName = "Star Cineplex New Market", Rows = 6, SeatsPerRow = 10 },
            new { Name = "Sapphire Hall", BranchName = "CineStar Mirpur", Rows = 7, SeatsPerRow = 10 },
            new { Name = "Prime Hall", BranchName = "CineStar Mirpur", Rows = 8, SeatsPerRow = 12 }
        };

        var newHallIds = new List<int>();
        foreach (var hallConfig in starCineplexHalls)
        {
            // Check if hall already exists
            if (await context.Halls.AnyAsync(h => h.Name == hallConfig.Name && h.BranchName == hallConfig.BranchName))
                continue;

            var hall = new Hall
            {
                Name = hallConfig.Name,
                BranchName = hallConfig.BranchName,
                TotalRows = hallConfig.Rows,
                TotalSeatsPerRow = hallConfig.SeatsPerRow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Halls.Add(hall);
            await context.SaveChangesAsync();
            newHallIds.Add(hall.Id);
        }

        // Now add seat categories for new halls
        foreach (var hallId in newHallIds)
        {
            var hall = await context.Halls.FindAsync(hallId);
            if (hall == null) continue;

            var priceMultiplier = hallId switch
            {
                4 or 5 or 6 => 1.2m,  // Star Cineplex
                7 or 8 => 1.1m,  // Star Cineplex New Market
                9 or 10 => 1.0m,  // CineStar Mirpur
                _ => 1.0m
            };

            var categories = new[]
            {
                new SeatCategory { HallId = hallId, Name = "Regular", FixedExtraPrice = (int)(100 * priceMultiplier) },
                new SeatCategory { HallId = hallId, Name = "Premium", FixedExtraPrice = (int)(200 * priceMultiplier) },
                new SeatCategory { HallId = hallId, Name = "VIP", FixedExtraPrice = (int)(350 * priceMultiplier) }
            };

            foreach (var cat in categories)
            {
                context.SeatCategories.Add(cat);
            }
        }

        await context.SaveChangesAsync();

        // Generate seats for new halls
        var rowLabels = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        foreach (var hallId in newHallIds)
        {
            var hall = await context.Halls.Include(h => h.SeatCategories).FirstOrDefaultAsync(h => h.Id == hallId);
            if (hall == null) continue;

            var categories = hall.SeatCategories.ToList();
            if (!categories.Any()) continue;

            for (int row = 0; row < hall.TotalRows; row++)
            {
                string rowLabel = rowLabels[row].ToString();
                int categoryIndex = categories.Count > 0 ? row * categories.Count / Math.Max(1, hall.TotalRows) : 0;
                int categoryId = categories[categoryIndex].Id;

                for (int seat = 1; seat <= hall.TotalSeatsPerRow; seat++)
                {
                    string seatLabel = $"{rowLabel}{seat}";
                    context.HallSeats.Add(new HallSeat
                    {
                        HallId = hall.Id,
                        SeatLabel = seatLabel,
                        RowLabel = rowLabel,
                        SeatNumber = seat,
                        CategoryId = categoryId,
                        IsActive = true
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedAdditionalMoviesAsync(CinemaDbContext context)
    {
        // Add Bangladeshi movies
        var bangladeshiMovies = new[]
        {
            new { Title = "Hawa", Description = "A story about the relationship between a house owner and her tenants", DurationMinutes = 122, Genre = "Drama", AgeRating = "PG", Poster = "https://m.media-amazon.com/images/M/MV5BMDIzZWYxOTgtY2VjNy00MzFlLWIxZjMtNGE1Yjg3ODI3ZjE3XkEyXkFqcGc@._V1_.jpg" },
            new { Title = "Putro", Description = "A family drama about a mysterious child", DurationMinutes = 115, Genre = "Drama, Thriller", AgeRating = "PG-13", Poster = "https://m.media-amazon.com/images/M/MV5BNjE5ZTIyZjUtMTUyNy00YjQ5LTk3MjktZGU2YTBjN2M3MmE4XkEyXkFqcGc@._V1_.jpg" },
            new { Title = "Shongram", Description = "A historical drama set during the Liberation War", DurationMinutes = 105, Genre = "Drama, War", AgeRating = "PG-13", Poster = "https://m.media-amazon.com/images/M/MV5BMjMwNTEyZTktYjMyNy00MjdkLWFjNDctYjZmODkwNTkwZTgyXkEyXkFqcGc@._V1_.jpg" },
            new { Title = "Monpura", Description = "A story about an island and its people", DurationMinutes = 120, Genre = "Drama", AgeRating = "PG", Poster = "https://m.media-amazon.com/images/M/MV5BNjA3MTk5NjYtNDlhNy00M2E5LWFjNzktNWFjNWE4ZDY1MTQwXkEyXkFqcGc@._V1_.jpg" },
            new { Title = "Bapjaner Bioscope", Description = "A nostalgic story about cinema", DurationMinutes = 108, Genre = "Drama, Comedy", AgeRating = "PG", Poster = "https://m.media-amazon.com/images/M/MV5BMjI4NjM0NDYyNV5BMl5BanBnXkFtZTcwNjA5Mzg5OQ@@._V1_.jpg" },
            new { Title = "Gondi", Description = "A thriller about a woman trapped in a difficult situation", DurationMinutes = 130, Genre = "Thriller", AgeRating = "PG-13", Poster = "https://m.media-amazon.com/images/M/MV5BMTc5MDE2ODcwNV5BMl5BanBnXkFtZTgwMzI2NzQ2NzM@._V1_.jpg" },
            new { Title = "Shonali Puroshkar", Description = "A comedy about a award ceremony", DurationMinutes = 95, Genre = "Comedy", AgeRating = "PG", Poster = "https://m.media-amazon.com/images/M/MV5BNDk3ODk0OTEtMGIyMy00N2EwLTlmMzgtNDgyYjdmM2I1NWUxXkEyXkFqcGc@._V1_.jpg" },
            new { Title = "Dupur Thakurpo", Description = "A classic drama about zamindar family", DurationMinutes = 140, Genre = "Drama", AgeRating = "PG", Poster = "https://m.media-amazon.com/images/M/MV5BMTAyNjU4MzEyMzNeQTJeQWpwZ15BbWU4MDc1NDQ2MzIx._V1_.jpg" }
        };

        int movieId = await context.Movies.CountAsync() + 1;
        foreach (var movie in bangladeshiMovies)
        {
            if (await context.Movies.AnyAsync(m => m.Title == movie.Title))
                continue;

            string slug = movie.Title.ToLower().Replace(" ", "-");
            context.Movies.Add(new Movie
            {
                Title = movie.Title,
                Slug = slug,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc),
                Language = "Bengali",
                Country = "Bangladesh",
                Genre = movie.Genre,
                AgeRating = movie.AgeRating,
                IMDbLikeScore = 7.5,
                PosterUrl = movie.Poster,
                BannerUrl = movie.Poster,
                MovieType = "CINEMA",
                OttPrice = 0,
                IsPublished = true,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            movieId++;
        }

        // Add Classic OTT movies
        var classicMovies = new[]
        {
            new { Title = "The Godfather", Description = "The aging patriarch of an organized crime dynasty", DurationMinutes = 175, Genre = "Crime, Drama", IMDbScore = 9.2, Poster = "https://m.media-amazon.com/images/M/MV5BM2MyNjYxNmUtYTAwNi00MTYxLWJmNWYtYzZlODY3ZTk3OTFlXkEyXkFqcGc@._V1_.jpg" },
            new { Title = "The Shawshank Redemption", Description = "Two imprisoned men bond over several years", DurationMinutes = 142, Genre = "Drama", IMDbScore = 9.3, Poster = "https://m.media-amazon.com/images/M/MV5BNDE3ODcxYzMtY2YzZC00NmNlLWJiNDMtZDViZWM2MzIxZDYwXkEyXkFqcGc@._V1_.jpg" },
            new { Title = "The Dark Knight", Description = "Batman fights the Joker in Gotham City", DurationMinutes = 152, Genre = "Action, Crime", IMDbScore = 9.0, Poster = "https://m.media-amazon.com/images/M/MV5BMTMxNTMwODM0NF5BMl5BanBnXkFtZTcwODAyNTk5Mw@@._V1_.jpg" },
            new { Title = "Pulp Fiction", Description = "The lives of two mob hitmen intertwine", DurationMinutes = 154, Genre = "Crime, Drama", IMDbScore = 8.9, Poster = "https://m.media-amazon.com/images/M/MV5BNGNhMDIzZTUtNTBlZi00MTRlLWFjM2ItYzdiMjkwNmU3NjhhXkEyXkFqcGc@._V1_.jpg" },
            new { Title = "Schindler's List", Description = "The true story of Oskar Schindler", DurationMinutes = 195, Genre = "Biography, Drama", IMDbScore = 9.0, Poster = "https://m.media-amazon.com/images/M/MV5BNDE4OTk1MTk2Nl5BMl5BanBnXkFtZTcwMzA0OTk1OA@@._V1_.jpg" },
            new { Title = "Forrest Gump", Description = "A man with a low IQ experiences American history", DurationMinutes = 142, Genre = "Drama, Romance", IMDbScore = 8.8, Poster = "https://m.media-amazon.com/images/M/MV5BNWIwODRlZTUtY2U3ZS00Yzg1LWJhNzYtMmZiYmEyNmU1NjMzXkEyXkFqcGc@._V1_.jpg" },
            new { Title = "Inception", Description = "A thief who steals corporate secrets through dream technology", DurationMinutes = 148, Genre = "Action, Sci-Fi", IMDbScore = 8.8, Poster = "https://m.media-amazon.com/images/M/MV5BMjAxMzY3NjcxNF5BMl5BanBnXkFtZTcwNTI5OTM1Mw@@._V1_.jpg" },
            new { Title = "The Matrix", Description = "A computer hacker learns about reality", DurationMinutes = 136, Genre = "Action, Sci-Fi", IMDbScore = 8.7, Poster = "https://m.media-amazon.com/images/M/MV5BNzQzOTk3OTAtNDQ0Zi00ZTVkLWI0MTEtMDllZjNkYzNjNTc5L2ltYWdlXkEyXkFqcGc@._V1_.jpg" },
            new { Title = "Goodfellas", Description = "The story of Henry Hill and his mob associates", DurationMinutes = 145, Genre = "Biography, Crime", IMDbScore = 8.7, Poster = "https://m.media-amazon.com/images/M/MV5BNjNhZTk0ZmEtMzg0OS00ZTVkLWIxMzktODMwNTNjZmE2ZGU1XkEyXkFqcGc@._V1_.jpg" },
            new { Title = "Interstellar", Description = "A team of explorers travel through a wormhole", DurationMinutes = 169, Genre = "Adventure, Drama", IMDbScore = 8.6, Poster = "https://m.media-amazon.com/images/M/MV5BYzdjMDAxZGItMjQ2NC00NGM5LWFiYWYtZTBjNTg5ZGI1NzhkXkEyXkFqcGc@._V1_.jpg" }
        };

        foreach (var movie in classicMovies)
        {
            if (await context.Movies.AnyAsync(m => m.Title == movie.Title))
                continue;

            string slug = movie.Title.ToLower().Replace(" ", "-").Replace("'", "");
            context.Movies.Add(new Movie
            {
                Title = movie.Title,
                Slug = slug,
                Description = movie.Description,
                DurationMinutes = movie.DurationMinutes,
                ReleaseDate = DateTime.SpecifyKind(new DateTime(1995, 1, 1), DateTimeKind.Utc),
                Language = "English",
                Country = "USA",
                Genre = movie.Genre,
                AgeRating = "R",
                IMDbLikeScore = movie.IMDbScore,
                PosterUrl = movie.Poster,
                BannerUrl = movie.Poster,
                MovieType = "OTT",
                OttPrice = 4.99m,
                IsPublished = true,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    private async Task SeedAdditionalShowTimesAsync(CinemaDbContext context)
    {
        var allMovies = await context.Movies.Where(m => m.MovieType == "CINEMA" && m.IsPublished).ToListAsync();
        var allHalls = await context.Halls.Where(h => h.IsActive).ToListAsync();

        if (!allMovies.Any() || !allHalls.Any()) return;

        var baseDate = DateTime.UtcNow.Date;

        // Add showtimes for new Bangladeshi movies
        var bangladeshiMovies = allMovies.Where(m => m.Language == "Bengali").ToList();

        foreach (var movie in bangladeshiMovies)
        {
            foreach (var hall in allHalls.Take(5))  // First 5 halls
            {
                // Check if showtime already exists
                var exists = await context.ShowTimes.AnyAsync(s => s.MovieId == movie.Id && s.HallId == hall.Id && s.ShowDate >= baseDate);
                if (exists) continue;

                var showTimes = new[] { new TimeSpan(12, 0, 0), new TimeSpan(15, 0, 0), new TimeSpan(18, 0, 0), new TimeSpan(21, 0, 0) };

                for (int day = 0; day < 5; day++)
                {
                    foreach (var startTime in showTimes)
                    {
                        var showDate = baseDate.AddDays(day);
                        var duration = movie.DurationMinutes ?? 120;
                        var endTime = startTime.Add(TimeSpan.FromMinutes(duration));

                        var basePrice = hall.TotalRows * 50m;  // Dynamic pricing

                        context.ShowTimes.Add(new ShowTime
                        {
                            MovieId = movie.Id,
                            HallId = hall.Id,
                            ShowDate = showDate,
                            StartTime = startTime,
                            EndTime = endTime,
                            BasePrice = basePrice,
                            Status = day == 0 ? "Scheduled" : "Upcoming",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        // Add upcoming showtimes for some movies
        var upcomingMovies = allMovies.Where(m => m.ReleaseDate > DateTime.UtcNow).ToList();

        foreach (var movie in upcomingMovies.Take(3))
        {
            foreach (var hall in allHalls.Take(3))
            {
                var showDate = (movie.ReleaseDate ?? DateTime.UtcNow).Date;
                var startTime = new TimeSpan(18, 0, 0);
                var duration = movie.DurationMinutes ?? 120;
                var endTime = startTime.Add(TimeSpan.FromMinutes(duration));

                var exists = await context.ShowTimes.AnyAsync(s => s.MovieId == movie.Id && s.HallId == hall.Id);
                if (!exists)
                {
                    context.ShowTimes.Add(new ShowTime
                    {
                        MovieId = movie.Id,
                        HallId = hall.Id,
                        ShowDate = showDate,
                        StartTime = startTime,
                        EndTime = endTime,
                        BasePrice = 400m,
                        Status = "Upcoming",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
