using System;
using System.Collections.Generic;

namespace be_booking_hotel.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public string UserId { get; set; } = null!;

    public int HotelId { get; set; }

    public int? ReservationId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Hotel Hotel { get; set; } = null!;

    public virtual Reservation? Reservation { get; set; }
}
