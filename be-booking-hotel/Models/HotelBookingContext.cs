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

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Facility> Facilities { get; set; }

    public virtual DbSet<Favourite> Favourites { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Hotel> Hotels { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // QUAN TRỌNG: Gọi base method để Identity tạo các bảng
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("PK__Cities__F2D21B76D8EFF858");

            entity.Property(e => e.Country).HasMaxLength(200);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<Facility>(entity =>
        {
            entity.HasKey(e => e.FacilityId).HasName("PK__Faciliti__5FB08A7401B4C70B");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Favourite>(entity =>
        {
            entity.HasKey(e => e.FavouriteId).HasName("PK__Favourit__5944B59A44B0F984");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Hotel).WithMany(p => p.Favourites)
                .HasForeignKey(d => d.HotelId)
                .HasConstraintName("FK_Favourites_Hotels");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD6048ECA40");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Hotel).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.HotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Feedbacks_Hotels");

            entity.HasOne(d => d.Reservation).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ReservationId)
                .HasConstraintName("FK_Feedbacks_Reservations");
        });

        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(e => e.HotelId).HasName("PK__Hotels__46023BDFBE637F10");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImgUrl).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.City).WithMany(p => p.Hotels)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_Hotels_Cities");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A3881D1B46A");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Success");

            entity.HasOne(d => d.Reservation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReservationId)
                .HasConstraintName("FK_Payments_Reservations");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__Reservat__B7EE5F244535920B");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Room).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_Reservations_Rooms");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__328639399C07FB46");

            entity.Property(e => e.ImgUrl).HasMaxLength(500);
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RoomType).HasMaxLength(100);

            entity.HasOne(d => d.Hotel).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.HotelId)
                .HasConstraintName("FK_Rooms_Hotels");

            entity.HasMany(d => d.Facilities).WithMany(p => p.Rooms)
                .UsingEntity<Dictionary<string, object>>(
                    "RoomFacility",
                    r => r.HasOne<Facility>().WithMany()
                        .HasForeignKey("FacilityId")
                        .HasConstraintName("FK_RoomFacilities_Facilities"),
                    l => l.HasOne<Room>().WithMany()
                        .HasForeignKey("RoomId")
                        .HasConstraintName("FK_RoomFacilities_Rooms"),
                    j =>
                    {
                        j.HasKey("RoomId", "FacilityId").HasName("PK__RoomFaci__677D319E84C7FF45");
                        j.ToTable("RoomFacilities");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
