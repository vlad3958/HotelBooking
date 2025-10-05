using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HotelBooking.Infrastructure.Identity;
using D = HotelBooking.Domain;

namespace HotelBooking.Infrastructure;

public class HotelBookingDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public HotelBookingDbContext(DbContextOptions<HotelBookingDbContext> options) : base(options)
    {
    }

    // Provider is configured externally in Program.cs (MySQL via Pomelo)

    // Explicit DbSets for Identity entities (sometimes useful for clarity / migrations on some setups)
    public DbSet<ApplicationUser> AppUsers => Users; // inherited Users from IdentityDbContext
    public DbSet<ApplicationRole> AppRoles => Roles; // inherited Roles from IdentityDbContext
    public DbSet<D.Hotel> Hotels { get; set; } = null!;
    public DbSet<D.Booking> Bookings { get; set; } = null!;
    public DbSet<D.Room> Rooms { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<D.Hotel>(b =>
        {
            b.Property(p => p.Name).HasMaxLength(200);
            b.Property(p => p.Address).HasMaxLength(300);
        });

        modelBuilder.Entity<D.Room>(b =>
        {
            b.HasOne(r => r.Hotel)
                .WithMany(h => h.Rooms)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<D.Booking>(b =>
        {
            // Identity ApplicationUser relation can be mapped later if needed
            b.HasOne(x => x.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.RoomId, x.StartDate, x.EndDate });
        });
    }
}
