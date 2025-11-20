using System;
using System.Collections.Generic;

namespace be_booking_hotel.Models;

public partial class HotelImage
{
    public int ImageId { get; set; }

    public int HotelId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsPrimary { get; set; }

    public int? DisplayOrder { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Hotel Hotel { get; set; } = null!;
}
