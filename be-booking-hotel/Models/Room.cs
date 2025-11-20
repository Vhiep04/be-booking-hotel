using System;
using System.Collections.Generic;

namespace be_booking_hotel.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int HotelId { get; set; }

    public string RoomType { get; set; } = null!;

    public decimal PricePerNight { get; set; }

    public int Capacity { get; set; }

    public string? ImgUrl { get; set; }

    public virtual Hotel Hotel { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<Facility> Facilities { get; set; } = new List<Facility>();
}
