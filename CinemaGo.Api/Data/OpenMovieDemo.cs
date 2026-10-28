using CinemaGo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Api.Data;

public static class OpenMovieDemo
{
    public const string Slug = "big-buck-bunny-demo-excerpt";

    public static async Task<bool> EnsureAsync(CinemaDbContext database)
    {
        await using var transaction = await database.Database.BeginTransactionAsync();
        await database.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(817234992)");
        if (await database.Movies.AnyAsync(movie => movie.Slug == Slug)) return false;
        database.Movies.Add(new Movie {
            Title = "Big Buck Bunny · Demo excerpt", Slug = Slug,
            Description = "A 15-second excerpt for trying the streaming player. © 2008 Blender Foundation / www.bigbuckbunny.org. Licensed CC BY 3.0; excerpt shortened from the original film. https://peach.blender.org/about/",
            DurationMinutes = 0, Language = "No dialogue", Genre = "Animation", Country = "Netherlands", AgeRating = "Demo clip",
            ReleaseDate = new DateTime(2008, 5, 30, 0, 0, 0, DateTimeKind.Utc),
            MovieType = "OTT", OttPrice = 0, IsPublished = true, IsFeatured = false,
            PosterUrl = "/demo/big-buck-bunny.svg", BannerUrl = "/demo/big-buck-bunny.svg",
            VideoUrl = "/demo/big-buck-bunny-clip.mp4"
        });
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
}
