using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nomadic.Models;

namespace Nomadic.Data;

public class NomadicUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class NomadicDbContext : IdentityDbContext<NomadicUser>
{
    public NomadicDbContext(DbContextOptions<NomadicDbContext> options) : base(options) { }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<TravelCatalogRecord> TravelCatalog => Set<TravelCatalogRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<TravelCatalogRecord>(entity =>
        {
            entity.HasKey(x => new { x.Category, x.RecordKey });
            entity.Property(x => x.Category).HasMaxLength(30);
            entity.Property(x => x.RecordKey).HasMaxLength(50);
            entity.Property(x => x.JsonData).IsRequired();
        });
        builder.Entity<Booking>(entity =>
        {
            entity.HasKey(x => x.BookingId);
            entity.Property(x => x.BookingId).HasMaxLength(50);
            entity.Property(x => x.PackageName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Destination).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CustomerName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CustomerEmail).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CustomerPhone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CustomerAddress).HasMaxLength(300).IsRequired();
            entity.Property(x => x.PackagePrice).HasPrecision(18, 2);
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.TaxesAndFees).HasPrecision(18, 2);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
        });
    }
}
