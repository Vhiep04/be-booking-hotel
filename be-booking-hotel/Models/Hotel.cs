using System;
using System.Collections.Generic;

namespace be_booking_hotel.Models;

public partial class Hotel
{
    public int HotelId { get; set; }

    public int CityId { get; set; }

    public string Name { get; set; } = null!;

    public string Location { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImgUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual City City { get; set; } = null!;

    public virtual ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<HotelImage> HotelImages { get; set; } = new List<HotelImage>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
