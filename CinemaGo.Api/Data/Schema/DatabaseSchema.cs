namespace CinemaGo.Api.Data.Schema;

// Base schema definitions following Drizzle-like patterns
public static class Tables
{
    public const string Users = "users";
    public const string Movies = "movies";
    public const string Ratings = "ratings";
    public const string Halls = "halls";
    public const string SeatCategories = "seat_categories";
    public const string HallSeats = "hall_seats";
    public const string ShowTimes = "show_times";
    public const string SeatReservations = "seat_reservations";
    public const string OttPurchases = "ott_purchases";
    public const string TicketOrders = "ticket_orders";
    public const string TicketOrderItems = "ticket_order_items";
    public const string Payments = "payments";
}

// User table schema
public static class UserSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS users (
            id SERIAL PRIMARY KEY,
            ""FullName"" VARCHAR(100) NOT NULL,
            ""Email"" VARCHAR(100) NOT NULL UNIQUE,
            ""PasswordHash"" VARCHAR(255) NOT NULL,
            ""Phone"" VARCHAR(20),
            ""Role"" VARCHAR(20) NOT NULL DEFAULT 'User',
            ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW(),
            ""UpdatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_users_email ON users(""Email"");
    ";
}

// Movie table schema
public static class MovieSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS movies (
            id SERIAL PRIMARY KEY,
            ""Title"" VARCHAR(200) NOT NULL,
            ""Slug"" VARCHAR(200) NOT NULL UNIQUE,
            ""Description"" VARCHAR(2000),
            ""DurationMinutes"" INTEGER,
            ""ReleaseDate"" DATE,
            ""Language"" VARCHAR(50),
            ""Country"" VARCHAR(50),
            ""Genre"" VARCHAR(200),
            ""AgeRating"" VARCHAR(20),
            ""IMDbLikeScore"" DOUBLE PRECISION,
            ""PosterUrl"" VARCHAR(500),
            ""BannerUrl"" VARCHAR(500),
            ""TrailerUrl"" VARCHAR(500),
            ""VideoUrl"" VARCHAR(500),
            ""MovieType"" VARCHAR(20) NOT NULL DEFAULT 'CINEMA',
            ""OttPrice"" DECIMAL(10,2) NOT NULL DEFAULT 0,
            ""IsPublished"" BOOLEAN NOT NULL DEFAULT false,
            ""IsFeatured"" BOOLEAN NOT NULL DEFAULT false,
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW(),
            ""UpdatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS idx_movies_slug ON movies(""Slug"");
        CREATE INDEX IF NOT EXISTS idx_movies_movie_type ON movies(""MovieType"");
    ";
}

// Rating table schema
public static class RatingSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS ratings (
            id SERIAL PRIMARY KEY,
            ""UserId"" INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            ""MovieId"" INTEGER NOT NULL REFERENCES movies(id) ON DELETE CASCADE,
            ""Score"" INTEGER NOT NULL CHECK (""Score"" >= 1 AND ""Score"" <= 10),
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW(),
            ""UpdatedAt"" TIMESTAMP NOT NULL DEFAULT NOW(),
            UNIQUE(""UserId"", ""MovieId"")
        );
    ";
}

// Hall table schema
public static class HallSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS halls (
            id SERIAL PRIMARY KEY,
            ""Name"" VARCHAR(100) NOT NULL,
            ""BranchName"" VARCHAR(100),
            ""TotalRows"" INTEGER NOT NULL,
            ""TotalSeatsPerRow"" INTEGER NOT NULL,
            ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
    ";
}

// SeatCategory table schema
public static class SeatCategorySchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS seat_categories (
            id SERIAL PRIMARY KEY,
            ""HallId"" INTEGER NOT NULL REFERENCES halls(id) ON DELETE CASCADE,
            ""Name"" VARCHAR(50) NOT NULL,
            ""FixedExtraPrice"" DECIMAL(10,2) NOT NULL DEFAULT 0
        );
    ";
}

// HallSeat table schema
public static class HallSeatSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS hall_seats (
            id SERIAL PRIMARY KEY,
            ""HallId"" INTEGER NOT NULL REFERENCES halls(id) ON DELETE CASCADE,
            ""SeatLabel"" VARCHAR(10) NOT NULL,
            ""RowLabel"" VARCHAR(5) NOT NULL,
            ""SeatNumber"" INTEGER NOT NULL,
            ""CategoryId"" INTEGER NOT NULL REFERENCES seat_categories(id) ON DELETE RESTRICT,
            ""IsActive"" BOOLEAN NOT NULL DEFAULT true,
            UNIQUE(""HallId"", ""SeatLabel"")
        );
    ";
}

// ShowTime table schema
public static class ShowTimeSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS show_times (
            id SERIAL PRIMARY KEY,
            ""MovieId"" INTEGER NOT NULL REFERENCES movies(id) ON DELETE CASCADE,
            ""HallId"" INTEGER NOT NULL REFERENCES halls(id) ON DELETE CASCADE,
            ""ShowDate"" DATE NOT NULL,
            ""StartTime"" TIME NOT NULL,
            ""EndTime"" TIME NOT NULL,
            ""BasePrice"" DECIMAL(10,2) NOT NULL,
            ""Status"" VARCHAR(20) NOT NULL DEFAULT 'Scheduled',
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
    ";
}

// SeatReservation table schema
public static class SeatReservationSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS seat_reservations (
            id SERIAL PRIMARY KEY,
            ""ShowTimeId"" INTEGER NOT NULL REFERENCES show_times(id) ON DELETE CASCADE,
            ""SeatId"" INTEGER NOT NULL REFERENCES hall_seats(id) ON DELETE CASCADE,
            ""UserId"" INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            ""Status"" VARCHAR(20) NOT NULL DEFAULT 'Reserved',
            ""LockedUntil"" TIMESTAMP,
            ""Price"" DECIMAL(10,2) NOT NULL,
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
    ";
}

// OttPurchase table schema
public static class OttPurchaseSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS ott_purchases (
            id SERIAL PRIMARY KEY,
            ""UserId"" INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            ""MovieId"" INTEGER NOT NULL REFERENCES movies(id) ON DELETE CASCADE,
            ""Amount"" DECIMAL(10,2) NOT NULL,
            ""PaymentId"" INTEGER REFERENCES payments(id) ON DELETE SET NULL,
            ""PurchasedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
    ";
}

// Payment table schema
public static class PaymentSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS payments (
            id SERIAL PRIMARY KEY,
            ""UserId"" INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            ""PaymentType"" VARCHAR(20) NOT NULL,
            ""ReferenceType"" VARCHAR(50) NOT NULL,
            ""ReferenceId"" INTEGER NOT NULL,
            ""Amount"" DECIMAL(10,2) NOT NULL,
            ""Method"" VARCHAR(20) NOT NULL DEFAULT 'DummyCard',
            ""Status"" VARCHAR(20) NOT NULL DEFAULT 'Pending',
            ""TransactionCode"" VARCHAR(100),
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
    ";
}

// TicketOrder table schema
public static class TicketOrderSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS ticket_orders (
            id SERIAL PRIMARY KEY,
            ""UserId"" INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
            ""ShowTimeId"" INTEGER NOT NULL REFERENCES show_times(id) ON DELETE CASCADE,
            ""BookingCode"" VARCHAR(20) NOT NULL UNIQUE,
            ""TotalAmount"" DECIMAL(10,2) NOT NULL,
            ""PaymentId"" INTEGER REFERENCES payments(id) ON DELETE SET NULL,
            ""Status"" VARCHAR(20) NOT NULL DEFAULT 'Pending',
            ""CreatedAt"" TIMESTAMP NOT NULL DEFAULT NOW()
        );
    ";
}

// TicketOrderItem table schema
public static class TicketOrderItemSchema
{
    public const string CreateTable = @"
        CREATE TABLE IF NOT EXISTS ticket_order_items (
            id SERIAL PRIMARY KEY,
            ""TicketOrderId"" INTEGER NOT NULL REFERENCES ticket_orders(id) ON DELETE CASCADE,
            ""SeatId"" INTEGER NOT NULL REFERENCES hall_seats(id) ON DELETE CASCADE,
            ""SeatLabel"" VARCHAR(10) NOT NULL,
            ""UnitPrice"" DECIMAL(10,2) NOT NULL
        );
    ";
}