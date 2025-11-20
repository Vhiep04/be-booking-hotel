using System;
using System.Collections.Generic;

namespace be_booking_hotel.Models;

public partial class Favourite
{
    public int FavouriteId { get; set; }

    public string UserId { get; set; } = null!;

    public int HotelId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Hotel Hotel { get; set; } = null!;
}
