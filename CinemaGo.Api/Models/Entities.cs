using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Api.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "User";
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<OttPurchase> OttPurchases { get; set; } = new List<OttPurchase>();
    public ICollection<TicketOrder> TicketOrders { get; set; } = new List<TicketOrder>();
    public ICollection<SeatReservation> SeatReservations { get; set; } = new List<SeatReservation>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class Movie
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    public int? DurationMinutes { get; set; }
    
    public DateTime? ReleaseDate { get; set; }
    
    [MaxLength(50)]
    public string? Language { get; set; }
    
    [MaxLength(50)]
    public string? Country { get; set; }
    
    [MaxLength(200)]
    public string? Genre { get; set; }
    
    [MaxLength(20)]
    public string? AgeRating { get; set; }
    
    public double? IMDbLikeScore { get; set; }
    
    [MaxLength(500)]
    public string? PosterUrl { get; set; }
    
    [MaxLength(500)]
    public string? BannerUrl { get; set; }
    
    [MaxLength(500)]
    public string? TrailerUrl { get; set; }
    
    [MaxLength(500)]
    public string? VideoUrl { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string MovieType { get; set; } = "CINEMA";
    
    public decimal OttPrice { get; set; }
    
    public bool IsPublished { get; set; } = false;
    
    public bool IsFeatured { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
    public ICollection<OttPurchase> OttPurchases { get; set; } = new List<OttPurchase>();
}

public class Rating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}

public class Hall
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? BranchName { get; set; }
    
    public int TotalRows { get; set; }
    
    public int TotalSeatsPerRow { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<SeatCategory> SeatCategories { get; set; } = new List<SeatCategory>();
    public ICollection<HallSeat> Seats { get; set; } = new List<HallSeat>();
    public ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
}

public class SeatCategory
{
    public int Id { get; set; }
    public int HallId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public decimal FixedExtraPrice { get; set; }
    
    // Navigation properties
    public Hall Hall { get; set; } = null!;
    public ICollection<HallSeat> Seats { get; set; } = new List<HallSeat>();
}

public class HallSeat
{
    public int Id { get; set; }
    public int HallId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string SeatLabel { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(5)]
    public string RowLabel { get; set; } = string.Empty;
    
    public int SeatNumber { get; set; }
    
    public int CategoryId { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Hall Hall { get; set; } = null!;
    public SeatCategory Category { get; set; } = null!;
    public ICollection<SeatReservation> Reservations { get; set; } = new List<SeatReservation>();
}

public class ShowTime
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int HallId { get; set; }
    public DateTime ShowDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal BasePrice { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "Scheduled";
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Movie Movie { get; set; } = null!;
    public Hall Hall { get; set; } = null!;
    public ICollection<SeatReservation> Reservations { get; set; } = new List<SeatReservation>();
    public ICollection<TicketOrder> Orders { get; set; } = new List<TicketOrder>();
}

public class SeatReservation
{
    public int Id { get; set; }
    public int ShowTimeId { get; set; }
    public int SeatId { get; set; }
    public int UserId { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "Reserved";
    
    public DateTime? LockedUntil { get; set; }
    
    public decimal Price { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ShowTime ShowTime { get; set; } = null!;
    public HallSeat Seat { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class OttPurchase
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public decimal Amount { get; set; }
    public int? PaymentId { get; set; }
    public DateTime PurchasedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
    public Payment? Payment { get; set; }
}

public class TicketOrder
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ShowTimeId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string BookingCode { get; set; } = string.Empty;
    
    public decimal TotalAmount { get; set; }
    
    public int? PaymentId { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ShowTime ShowTime { get; set; } = null!;
    public Payment? Payment { get; set; }
    public ICollection<TicketOrderItem> OrderItems { get; set; } = new List<TicketOrderItem>();
}

public class TicketOrderItem
{
    public int Id { get; set; }
    public int TicketOrderId { get; set; }
    public int SeatId { get; set; }
    
    [Required]
    [MaxLength(10)]
    public string SeatLabel { get; set; } = string.Empty;
    
    public decimal UnitPrice { get; set; }
    
    // Navigation properties
    public TicketOrder TicketOrder { get; set; } = null!;
    public HallSeat Seat { get; set; } = null!;
}

public class Payment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string PaymentType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ReferenceType { get; set; } = string.Empty;
    
    public int ReferenceId { get; set; }
    
    public decimal Amount { get; set; }
    
    [MaxLength(20)]
    public string Method { get; set; } = "DummyCard";
    
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";
    
    public string? TransactionCode { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}