using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Api.DTOs;

// Auth DTOs
public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
}

// Movie DTOs
public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
    public string? Genre { get; set; }
    public string? AgeRating { get; set; }
    public double? IMDbLikeScore { get; set; }
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string MovieType { get; set; } = string.Empty;
    public decimal OttPrice { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int? UserRating { get; set; }
    public int RatingCount { get; set; }
}

public class MovieDetailDto : MovieDto
{
    public bool IsPurchased { get; set; }
}

public class CreateMovieRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public string? Language { get; set; }
    public string? Country { get; set; }
    public string? Genre { get; set; }
    public string? AgeRating { get; set; }
    public double? IMDbLikeScore { get; set; }
    public string? PosterUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? TrailerUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string MovieType { get; set; } = "CINEMA";
    public decimal OttPrice { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublished { get; set; } = true;
}

public class UpdateMovieRequest : CreateMovieRequest
{
}

public class MovieSearchRequest
{
    public string? Search { get; set; }
    public string? MovieType { get; set; }
    public string? Genre { get; set; }
    public bool? IsFeatured { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class HomeDataDto
{
    public List<MovieDto> Featured { get; set; } = new();
    public List<MovieDto> OttMovies { get; set; } = new();
    public List<MovieDto> CinemaMovies { get; set; } = new();
    public List<MovieDto> NowShowing { get; set; } = new();
    public List<MovieDto> ComingSoon { get; set; } = new();
}

// Cinema DTOs
public class ShowTimeDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePosterUrl { get; set; } = string.Empty;
    public int HallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AvailableSeats { get; set; }
}

public class ShowTimeWithSeatsDto : ShowTimeDto
{
    public List<SeatDto> Seats { get; set; } = new();
    public List<SeatCategoryDto> Categories { get; set; } = new();
    public List<int> ReservedSeatIds { get; set; } = new();
}

public class SeatDto
{
    public int Id { get; set; }
    public string SeatLabel { get; set; } = string.Empty;
    public string RowLabel { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal ExtraPrice { get; set; }
    public bool IsAvailable { get; set; }
}

public class SeatCategoryDto
{
    public int Id { get; set; }
    public int HallId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal FixedExtraPrice { get; set; }
}

public class CreateShowTimeRequest
{
    public int MovieId { get; set; }
    public int HallId { get; set; }
    public DateTime ShowDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public decimal BasePrice { get; set; }
}

public class BookingRequest
{
    public int ShowTimeId { get; set; }
    public List<int> SeatIds { get; set; } = new();
    public string PaymentMethod { get; set; } = "DummyCard";
}

public class BookingResponse
{
    public int OrderId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<TicketItemDto> Tickets { get; set; } = new();
    public int ShowTimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public List<string> SeatLabels { get; set; } = new();
}

public class TicketItemDto
{
    public string SeatLabel { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}

// Hall DTOs
public class HallDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int TotalSeatsPerRow { get; set; }
    public bool IsActive { get; set; }
    public List<SeatCategoryDto> SeatCategories { get; set; } = new();
}

public class CreateHallRequest
{
    public string Name { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public int TotalRows { get; set; }
    public int TotalSeatsPerRow { get; set; }
    public List<CreateSeatCategoryRequest> SeatCategories { get; set; } = new();
}

public class CreateSeatCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal FixedExtraPrice { get; set; }
}

public class UpdateHallRequest
{
    public string Name { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public int TotalRows { get; set; }
    public int TotalSeatsPerRow { get; set; }
    public List<CreateSeatCategoryRequest> SeatCategories { get; set; } = new();
}

// Rating DTOs
public class RatingDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int MovieId { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AddRatingRequest
{
    public int MovieId { get; set; }
    public int Score { get; set; }
}

// OTT DTOs
public class OttPurchaseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePosterUrl { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PurchasedAt { get; set; }
}

public class WatchDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string PosterUrl { get; set; } = string.Empty;
}

public class PurchaseOttRequest
{
    public int MovieId { get; set; }
    public string PaymentMethod { get; set; } = "DummyCard";
}

// Profile DTOs
public class ProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class ProfilePurchasesDto
{
    public List<OttPurchaseDto> OttPurchases { get; set; } = new();
    public List<TicketOrderDto> TicketOrders { get; set; } = new();
}

public class TicketOrderDto
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string MoviePosterUrl { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public List<string> Seats { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Booking DTOs (aliases for backward compatibility)
public class BookingRequestDto
{
    public int ShowTimeId { get; set; }
    public List<int> SeatIds { get; set; } = new();
    public string PaymentMethod { get; set; } = "DummyCard";
}

public class BookingResponseDto
{
    public int OrderId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<TicketItemDto> Tickets { get; set; } = new();
    public int ShowTimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public List<string> SeatLabels { get; set; } = new();
}

// Admin Dashboard DTOs
public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalMovies { get; set; }
    public int TotalHalls { get; set; }
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TodayBookings { get; set; }
    public decimal TodayRevenue { get; set; }
}

public class AdminBookingDto
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Seats { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionCode { get; set; }
}

public class AdminShowTimeDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int HallId { get; set; }
    public string HallName { get; set; } = string.Empty;
    public DateTime ShowDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BookedSeats { get; set; }
}

public class UpdateShowTimeRequest
{
    public string? Status { get; set; }
    public decimal? BasePrice { get; set; }
}