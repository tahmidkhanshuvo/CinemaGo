using Microsoft.EntityFrameworkCore;
using CinemaGo.Api.Models;

namespace CinemaGo.Api.Data;

public class CinemaDbContext : DbContext
{
    public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<SeatCategory> SeatCategories => Set<SeatCategory>();
    public DbSet<HallSeat> HallSeats => Set<HallSeat>();
    public DbSet<ShowTime> ShowTimes => Set<ShowTime>();
    public DbSet<SeatReservation> SeatReservations => Set<SeatReservation>();
    public DbSet<OttPurchase> OttPurchases => Set<OttPurchase>();
    public DbSet<TicketOrder> TicketOrders => Set<TicketOrder>();
    public DbSet<TicketOrderItem> TicketOrderItems => Set<TicketOrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).HasDefaultValue("User");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
        
        // Movie configuration
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.MovieType).HasDefaultValue("CINEMA");
            entity.Property(e => e.IsPublished).HasDefaultValue(false);
            entity.Property(e => e.IsFeatured).HasDefaultValue(false);
        });
        
        // Rating configuration
        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.MovieId }).IsUnique();
        });
        
        // Hall configuration
        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
        
        // SeatCategory configuration
        modelBuilder.Entity<SeatCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });
        
        // HallSeat configuration
        modelBuilder.Entity<HallSeat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SeatLabel).IsRequired();
            entity.HasIndex(e => new { e.HallId, e.SeatLabel }).IsUnique();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
        
        // ShowTime configuration
        modelBuilder.Entity<ShowTime>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasDefaultValue("Scheduled");
        });
        
        // SeatReservation configuration
        modelBuilder.Entity<SeatReservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ShowTimeId, e.SeatId, e.UserId });
            entity.Property(e => e.Status).HasDefaultValue("Reserved");
        });
        
        // OttPurchase configuration
        modelBuilder.Entity<OttPurchase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.MovieId });
        });
        
        // TicketOrder configuration
        modelBuilder.Entity<TicketOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingCode).IsRequired();
            entity.HasIndex(e => e.BookingCode).IsUnique();
            entity.Property(e => e.Status).HasDefaultValue("Pending");
        });
        
        // TicketOrderItem configuration
        modelBuilder.Entity<TicketOrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        
        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasDefaultValue("Pending");
            entity.Property(e => e.Method).HasDefaultValue("DummyCard");
        });
        
        // Relationships
        modelBuilder.Entity<Rating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<HallSeat>()
            .HasOne(s => s.Hall)
            .WithMany(h => h.Seats)
            .HasForeignKey(s => s.HallId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<SeatCategory>()
            .HasOne(sc => sc.Hall)
            .WithMany(h => h.SeatCategories)
            .HasForeignKey(sc => sc.HallId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<HallSeat>()
            .HasOne(s => s.Category)
            .WithMany(c => c.Seats)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}