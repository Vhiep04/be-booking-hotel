using System;
using System.Collections.Generic;

namespace be_booking_hotel.Models;

public partial class City
{
    public int CityId { get; set; }

    public string Name { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public virtual ICollection<CityImage> CityImages { get; set; } = new List<CityImage>();

    public virtual ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}
