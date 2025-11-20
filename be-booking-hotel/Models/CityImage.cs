using System;
using System.Collections.Generic;

namespace be_booking_hotel.Models;

public partial class CityImage
{
    public int ImageId { get; set; }

    public int CityId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsPrimary { get; set; }

    public int? DisplayOrder { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual City City { get; set; } = null!;
}
