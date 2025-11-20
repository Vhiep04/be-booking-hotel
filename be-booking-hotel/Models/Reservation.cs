using System;
using System.Collections.Generic;

namespace be_booking_hotel.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public string UserId { get; set; } = null!;

    public int RoomId { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public decimal TotalPrice { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Room Room { get; set; } = null!;
}
