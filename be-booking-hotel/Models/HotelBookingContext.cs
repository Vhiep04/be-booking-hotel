using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Models;

public partial class HotelBookingContext : IdentityDbContext<ApplicationUser>
{
    public HotelBookingContext()
    {
    }

    public HotelBookingContext(DbContextOptions<HotelBookingContext> options)
        : base(options)
    {
    }

    // DbSets
    public virtual DbSet<City> Cities { get; set; }
    public virtual DbSet<Facility> Facilities { get; set; }
    public virtual DbSet<Favourite> Favourites { get; set; }
    public virtual DbSet<Feedback> Feedbacks { get; set; }
    public virtual DbSet<Hotel> Hotels { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }
    public virtual DbSet<Reservation> Reservations { get; set; }
    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<HotelImage> HotelImages { get; set; }
    public virtual DbSet<CityImage> CityImages { get; set; }
    public virtual DbSet<RoomType> RoomTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Phải gọi base trước để Identity tạo các bảng
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("PK__Cities__F2D21B76D8EFF858");
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Country).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
        });
        modelBuilder.Entity<CityImage>(entity =>
        {
            entity.ToTable("CityImages");
            entity.HasKey(e => e.ImageId).HasName("PK__CityImag__7516F70C2DAB3335");

            entity.HasIndex(e => e.CityId, "IX_CityImages_CityId");
            entity.HasIndex(e => e.IsPrimary, "IX_CityImages_IsPrimary");

            entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            // FK: CityImage -> City
            entity.HasOne(d => d.City)
                .WithMany(p => p.CityImages)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CityImages_Cities");
        });

        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.FacilityId).HasName("PK__Faciliti__5FB08A7401B4C70B");
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Favourite>(entity =>
        {
            entity.HasKey(e => e.FavouriteId).HasName("PK__Favourit__5944B59A44B0F984");

            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            // FK: Favourite -> Hotel
            entity.HasOne(d => d.Hotel)
                .WithMany(p => p.Favourites)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Favourites_Hotels");

            // FK: Favourite -> ApplicationUser
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Favourites_AspNetUsers");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD6048ECA40");

            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            // FK: Feedback -> Hotel
            entity.HasOne(d => d.Hotel)
                .WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Feedbacks_Hotels");

            // FK: Feedback -> Reservation
            entity.HasOne(d => d.Reservation)
                .WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ReservationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Feedbacks_Reservations");

            // FK: Feedback -> ApplicationUser
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Feedbacks_AspNetUsers");
        });

        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.HotelId).HasName("PK__Hotels__46023BDFBE637F10");

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ImgUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            // FK: Hotel -> City
            entity.HasOne(d => d.City)
                .WithMany(p => p.Hotels)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Hotels_Cities");
        });

        modelBuilder.Entity<HotelImage>(entity =>
        {
            entity.ToTable("HotelImages");
            entity.HasKey(e => e.ImageId).HasName("PK__HotelIma__7516F70C97C96987");

            entity.HasIndex(e => e.HotelId, "IX_HotelImages_HotelId");
            entity.HasIndex(e => e.IsPrimary, "IX_HotelImages_IsPrimary");

            entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Hotel)
                .WithMany(p => p.HotelImages)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HotelImages_Hotels");
        });
        // HotelImages configuration
        modelBuilder.Entity<HotelImage>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.HasOne(e => e.Hotel)
                .WithMany(h => h.HotelImages)
                .HasForeignKey(e => e.HotelId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CityImages configuration
        modelBuilder.Entity<CityImage>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.HasOne(e => e.City)
                .WithMany(c => c.CityImages)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A3881D1B46A");

            entity.Property(e => e.PaymentMethod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Success");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            // FK: Payment -> Reservation
            entity.HasOne(d => d.Reservation)
                .WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReservationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Payments_Reservations");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__Reservat__B7EE5F244535920B");

            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            // FK: Reservation -> Room
            entity.HasOne(d => d.Room)
                .WithMany(p => p.Reservations)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Reservations_Rooms");

            // FK: Reservation -> ApplicationUser
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Reservations_AspNetUsers");
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.RoomTypeId);
            entity.Property(e => e.TypeName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImgUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            // FK: RoomType -> Hotel
            entity.HasOne(d => d.Hotel)
                .WithMany(p => p.RoomTypes)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RoomTypes_Hotels");

            // Many-to-Many: RoomType <-> Facility
            entity.HasMany(d => d.Facilities)
                .WithMany(p => p.RoomTypes)
                .UsingEntity<Dictionary<string, object>>(
                    "RoomTypeFacility",
                    r => r.HasOne<Facility>().WithMany()
                        .HasForeignKey("FacilityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_RoomTypeFacilities_Facilities"),
                    l => l.HasOne<RoomType>().WithMany()
                        .HasForeignKey("RoomTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FK_RoomTypeFacilities_RoomTypes"),
                    j => j.ToTable("RoomTypeFacilities"));
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__328639399C07FB46");
            entity.Property(e => e.RoomNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Available");

            entity.HasOne(d => d.Hotel)
                .WithMany(p => p.Rooms)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Rooms_Hotels");

            entity.HasOne(d => d.RoomType)
                .WithMany(p => p.Rooms)
                .HasForeignKey(d => d.RoomTypeId)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Rooms_RoomTypes");
        });

        // ====================================
        // ApplicationUser Configuration
        // ====================================
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}