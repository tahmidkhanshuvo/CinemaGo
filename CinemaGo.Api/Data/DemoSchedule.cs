using CinemaGo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Api.Data;

public static class DemoSchedule
{
    // Explicit opt-in for the hosted educational demo. Existing halls and history
    // are never changed. A lock and date checks make repeated cold starts safe.
    public static async Task<int> EnsureUpcomingAsync(CinemaDbContext database)
    {
        await using var transaction = await database.Database.BeginTransactionAsync();
        await database.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(817234991)");
        var movies = await database.Movies.Where(movie => movie.IsPublished && (movie.MovieType == "CINEMA" || movie.MovieType == "BOTH"))
            .OrderBy(movie => movie.Id).Take(3).Select(movie => movie.Id).ToListAsync();
        if (movies.Count == 0) return 0;
        var hall = await database.Halls.SingleOrDefaultAsync(item => item.Name == "CinemaGo Demo Screen" && item.BranchName == "Online demo");
        if (hall == null)
        {
            hall = new Hall { Name = "CinemaGo Demo Screen", BranchName = "Online demo", TotalRows = 2, TotalSeatsPerRow = 8, IsActive = true };
            database.Halls.Add(hall);
            await database.SaveChangesAsync();
            var category = new SeatCategory { HallId = hall.Id, Name = "Demo standard", FixedExtraPrice = 0 };
            database.SeatCategories.Add(category);
            await database.SaveChangesAsync();
            foreach (var row in new[] { "A", "B" })
                for (var number = 1; number <= 8; number++)
                    database.HallSeats.Add(new HallSeat { HallId = hall.Id, CategoryId = category.Id, SeatLabel = row + number, RowLabel = row, SeatNumber = number, IsActive = true });
            await database.SaveChangesAsync();
        }
        if (!hall.IsActive) return 0;
        var start = DateTime.UtcNow.Date.AddDays(1);
        var end = start.AddDays(30);
        var dates = await database.ShowTimes.Where(show => show.HallId == hall.Id && show.ShowDate >= start && show.ShowDate < end)
            .Select(show => show.ShowDate).ToListAsync();
        var added = 0;
        for (var day = 0; day < 30; day++)
        {
            var date = start.AddDays(day);
            if (dates.Any(existing => existing.Date == date)) continue;
            database.ShowTimes.Add(new ShowTime { MovieId = movies[day % movies.Count], HallId = hall.Id, ShowDate = date, StartTime = TimeSpan.FromHours(13), EndTime = TimeSpan.FromHours(16), BasePrice = 150, Status = "Scheduled", CreatedAt = DateTime.UtcNow });
            added++;
        }
        await database.SaveChangesAsync();
        await transaction.CommitAsync();
        return added;
    }
}
