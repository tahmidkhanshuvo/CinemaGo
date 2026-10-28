using System.Security.Claims;
using System.Reflection;
using CinemaGo.Api.Controllers;
using CinemaGo.Api.Data;
using CinemaGo.Api.DTOs;
using CinemaGo.Api.Helpers;
using CinemaGo.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

// Integration checks use their own schema. No application tables or accounts are changed.
var baseConnection = Environment.GetEnvironmentVariable("CINEMAGO_TEST_CONNECTION")
    ?? throw new InvalidOperationException("Set CINEMAGO_TEST_CONNECTION to a PostgreSQL test connection.");
var schema = "cinemago_test_" + Guid.NewGuid().ToString("N");
var connection = new NpgsqlConnectionStringBuilder(baseConnection) { SearchPath = schema };
CinemaDbContext Database() => new IsolatedCinemaContext(new DbContextOptionsBuilder<CinemaDbContext>().UseNpgsql(connection.ConnectionString).Options, schema);
void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS " + message); }
T SignedIn<T>(T controller, int userId) where T : ControllerBase
{
    controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "test")) } };
    return controller;
}
await using var admin = new NpgsqlConnection(baseConnection);
await admin.OpenAsync();
await using (var create = new NpgsqlCommand($"CREATE SCHEMA \"{schema}\"", admin)) await create.ExecuteNonQueryAsync();
try
{
    await using var setup = Database();
    await setup.Database.ExecuteSqlRawAsync(setup.Database.GenerateCreateScript());
    var buyer = new User { FullName = "Workflow buyer", Email = "buyer@example.test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Isolated-current-password"), Role = "User" };
    var other = new User { FullName = "Other buyer", Email = "other@example.test", PasswordHash = "not-a-login", Role = "User" };
    var film = new Movie { Title = "Workflow film", Slug = "workflow-film", MovieType = "CINEMA", IsPublished = true };
    var stream = new Movie { Title = "Workflow stream", Slug = "workflow-stream", MovieType = "OTT", IsPublished = true, OttPrice = 75, VideoUrl = "https://example.test/video.mp4" };
    var hidden = new Movie { Title = "Unpublished", Slug = "unpublished", MovieType = "OTT", IsPublished = false, OttPrice = 1 };
    var hall = new Hall { Name = "Workflow hall", TotalRows = 1, TotalSeatsPerRow = 3 };
    var otherHall = new Hall { Name = "Other hall", TotalRows = 1, TotalSeatsPerRow = 1 };
    setup.AddRange(buyer, other, film, stream, hidden, hall, otherHall);
    await setup.SaveChangesAsync();
    var category = new SeatCategory { HallId = hall.Id, Name = "Regular", FixedExtraPrice = 25 };
    var otherCategory = new SeatCategory { HallId = otherHall.Id, Name = "Regular" };
    setup.AddRange(category, otherCategory);
    await setup.SaveChangesAsync();
    var seats = Enumerable.Range(1, 3).Select(i => new HallSeat { HallId = hall.Id, CategoryId = category.Id, SeatLabel = "A" + i, RowLabel = "A", SeatNumber = i }).ToArray();
    var wrongSeat = new HallSeat { HallId = otherHall.Id, CategoryId = otherCategory.Id, SeatLabel = "B1", RowLabel = "B", SeatNumber = 1, IsActive = false };
    var show = new ShowTime { MovieId = film.Id, HallId = hall.Id, ShowDate = DateTime.UtcNow.Date.AddDays(2), StartTime = TimeSpan.FromHours(18), EndTime = TimeSpan.FromHours(20), BasePrice = 100, CreatedAt = DateTime.UtcNow };
    setup.AddRange(seats); setup.AddRange(wrongSeat, show);
    var pastShow = new ShowTime { MovieId = film.Id, HallId = hall.Id, ShowDate = DateTime.UtcNow.Date.AddDays(-1), StartTime = TimeSpan.FromHours(18), EndTime = TimeSpan.FromHours(20), BasePrice = 100, CreatedAt = DateTime.UtcNow };
    var unavailableShow = new ShowTime { MovieId = film.Id, HallId = otherHall.Id, ShowDate = show.ShowDate, StartTime = show.StartTime, EndTime = show.EndTime, BasePrice = 100, CreatedAt = DateTime.UtcNow };
    setup.AddRange(pastShow, unavailableShow);
    await setup.SaveChangesAsync();
    await using (var db = Database())
    {
        var result = await new CinemaController(db).GetShowTimes();
        var upcoming = ((result.Result as OkObjectResult)?.Value as ApiResponse<List<ShowTimeDto>>)?.Data;
        Check(upcoming?.Count == 2 && upcoming.All(s=>s.Id != pastShow.Id), "public discovery defaults to upcoming showtimes");
        Check(upcoming!.Single(s=>s.Id == unavailableShow.Id).AvailableSeats == 0, "available-seat counts exclude disabled seats");
        var history = await new CinemaController(db).GetShowTimes(date: pastShow.ShowDate);
        var historical = ((history.Result as OkObjectResult)?.Value as ApiResponse<List<ShowTimeDto>>)?.Data;
        Check(historical?.Single().Id == pastShow.Id, "explicit historical date queries remain available");
    }

    async Task<int> Book(int userId, params int[] ids)
    {
        await using var db = Database();
        var result = await SignedIn(new CinemaController(db), userId).BookTickets(new BookingRequestDto { ShowTimeId = show.Id, SeatIds = ids.ToList(), PaymentMethod = "DummyCard" });
        return (result.Result as ObjectResult)?.StatusCode ?? 200;
    }
    Check(await Book(buyer.Id, wrongSeat.Id) == 400, "seats from a different hall are rejected");
    Check(await Book(buyer.Id, seats[0].Id, seats[0].Id) == 400, "duplicate seat IDs are rejected");
    var competing = await Task.WhenAll(Book(buyer.Id, seats[0].Id), Book(other.Id, seats[0].Id));
    Check(competing.Count(s => s == 200) == 1 && competing.Count(s => s == 400) == 1, "simultaneous bookings cannot sell the same seat twice");
    await using (var db = Database())
    {
        Check(await db.Payments.CountAsync() == 1 && await db.TicketOrders.CountAsync() == 1, "failed bookings leave no payment or order");
        var paid = await db.Payments.SingleAsync();
        Check(paid.Amount == 125 && paid.ReferenceId == (await db.TicketOrders.SingleAsync()).Id, "ticket price and payment reference come from server data");
    }
    Check(await Book(buyer.Id, seats[1].Id) == 200 && await Book(buyer.Id, seats[2].Id) == 200, "separate orders can be placed for the same showtime");
    await using (var db = Database())
    {
        var order = await db.TicketOrders.Include(o=>o.OrderItems).SingleAsync(o=>o.OrderItems.Any(i=>i.SeatId == seats[1].Id));
        await new CinemaGo.Api.Controllers.Admin.BookingsController(db).CancelBooking(order.Id);
        Check(await db.SeatReservations.AnyAsync(r=>r.SeatId == seats[1].Id && r.Status == "Cancelled") && await db.SeatReservations.AnyAsync(r=>r.SeatId == seats[2].Id && r.Status == "Booked"), "cancelling one order preserves the buyer's other seats");
    }
    await using (var db = Database())
    {
        var controller = new CinemaGo.Api.Controllers.Admin.HallsController(db);
        var changed = await controller.UpdateHall(hall.Id, new UpdateHallRequest { Name = "Updated hall", TotalRows = 2, TotalSeatsPerRow = 3, SeatCategories = new() { new() { Name = "Regular", FixedExtraPrice = 30 } } });
        Check(changed.Result is BadRequestObjectResult, "booked hall layouts cannot erase reservation history");
    }
    await using (var db = Database())
    {
        var changed = await new CinemaGo.Api.Controllers.Admin.HallsController(db).UpdateHall(hall.Id, new UpdateHallRequest { Name = "Updated hall", TotalRows = 1, TotalSeatsPerRow = 3, SeatCategories = new() { new() { Name = "Regular", FixedExtraPrice = 30 } } });
        Check(changed.Result is OkObjectResult && await db.HallSeats.CountAsync(s=>s.HallId == hall.Id) == 3, "hall details and prices update without replacing booked seats");
    }
    async Task<int> Purchase(int movieId)
    {
        await using var db = Database();
        var result = await SignedIn(new OttController(db), buyer.Id).PurchaseMovie(movieId);
        return (result.Result as ObjectResult)?.StatusCode ?? 200;
    }
    Check(await Purchase(hidden.Id) == 404 && await Purchase(film.Id) == 404, "unpublished and cinema-only films cannot be purchased as OTT");
    await using (var db = Database())
        Check((await SignedIn(new OttController(db), other.Id).WatchMovie(stream.Id)).Result is BadRequestObjectResult, "watch URLs require an owned purchase");
    var purchases = await Task.WhenAll(Purchase(stream.Id), Purchase(stream.Id));
    Check(purchases.Count(s=>s == 200) == 1 && purchases.Count(s=>s == 400) == 1, "simultaneous OTT purchases produce only one purchase");
    await using (var db = Database())
    {
        Check(await db.OttPurchases.CountAsync() == 1 && await db.Payments.CountAsync(p=>p.PaymentType == "OTT") == 1, "OTT payment and purchase are committed together");
        Check((await SignedIn(new OttController(db), buyer.Id).WatchMovie(stream.Id)).Result is OkObjectResult, "purchased stream is available to its owner");
        var hybrid = new Movie { Title = "Both formats", Slug = "both-formats", MovieType = "BOTH", IsPublished = true, OttPrice = 40, VideoUrl = "https://example.test/hybrid.mp4" };
        db.Movies.Add(hybrid);
        await db.SaveChangesAsync();
        Check(await Purchase(hybrid.Id) == 200, "titles available in both formats retain streaming purchases");
    }
    var adminControllers = typeof(CinemaController).Assembly.GetTypes().Where(t=>t.Namespace == "CinemaGo.Api.Controllers.Admin" && typeof(ControllerBase).IsAssignableFrom(t));
    Check(adminControllers.All(t=>t.GetCustomAttributes<AuthorizeAttribute>().Any(a=>a.Roles == "Admin")), "every administrator controller requires the Admin role");
    await using (var db = Database())
    {
        var profile = SignedIn(new ProfileController(db), buyer.Id);
        var before = JwtHelper.CredentialVersion(buyer);
        Check((await profile.ChangePassword(new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "Isolated-new-password", ConfirmPassword = "Isolated-new-password" })).Result is BadRequestObjectResult, "password changes require the current password");
        Check((await profile.ChangePassword(new ChangePasswordRequest { CurrentPassword = "Isolated-current-password", NewPassword = "short", ConfirmPassword = "short" })).Result is BadRequestObjectResult, "short replacement passwords are rejected");
        Check((await profile.ChangePassword(new ChangePasswordRequest { CurrentPassword = "Isolated-current-password", NewPassword = "Isolated-new-password", ConfirmPassword = "Isolated-new-password" })).Result is OkObjectResult, "owners can change their own password");
        var changed = await db.Users.FindAsync(buyer.Id);
        Check(BCrypt.Net.BCrypt.Verify("Isolated-new-password", changed!.PasswordHash) && !BCrypt.Net.BCrypt.Verify("Isolated-current-password", changed.PasswordHash) && JwtHelper.CredentialVersion(changed) != before, "password changes invalidate the previous credential version");
        Check((await db.Users.FindAsync(other.Id))!.PasswordHash == "not-a-login", "password changes preserve other accounts");
    }
    await using (var db = Database())
    {
        Check(await DemoSchedule.EnsureUpcomingAsync(db) == 30, "opt-in demo schedule creates a bounded upcoming calendar");
        Check(await DemoSchedule.EnsureUpcomingAsync(db) == 0, "repeated demo scheduling does not duplicate performances");
        Check(await db.ShowTimes.AnyAsync(s => s.Id == pastShow.Id), "demo scheduling preserves existing history");
    }
    await using (var db = Database())
    {
        var missingStream = new Movie { Title = "No media", Slug = "no-media", MovieType = "OTT", IsPublished = true };
        db.Movies.Add(missingStream); await db.SaveChangesAsync();
        Check(await Purchase(missingStream.Id) == 400, "titles without media cannot be purchased");
        Check((await new MoviesController(db).GetMovieBySlug(hidden.Slug)).Result is NotFoundObjectResult, "unpublished movie details remain private");
        Check(await OpenMovieDemo.EnsureAsync(db) && !await OpenMovieDemo.EnsureAsync(db), "open-movie demo content is provisioned idempotently");
    }
    Console.WriteLine("All PostgreSQL workflow checks passed.");
}
finally
{
    // The generated identifier is the sole cleanup target, never public/application schemas.
    if (!System.Text.RegularExpressions.Regex.IsMatch(schema, "^cinemago_test_[a-f0-9]{32}$")) throw new Exception("Unsafe cleanup target");
    NpgsqlConnection.ClearAllPools();
    await using var drop = new NpgsqlCommand($"DROP SCHEMA \"{schema}\" CASCADE", admin);
    await drop.ExecuteNonQueryAsync();
    Console.WriteLine("Disposable workflow schema removed.");
}

sealed class IsolatedCinemaContext(DbContextOptions<CinemaDbContext> options, string schema) : CinemaDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Explicit qualification also works through transaction-pooling proxies
        // that do not preserve the connection's startup search_path setting.
        builder.HasDefaultSchema(schema);
    }
}
