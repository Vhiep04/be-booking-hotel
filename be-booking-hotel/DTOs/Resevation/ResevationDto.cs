namespace be_booking_hotel.DTOs.Resevation
{
    public class ReservationDto
    {
        public int ReservationId { get; set; }
        public string BookingCode { get; set; } = string.Empty;

        // Room & Hotel
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomTypeName { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string HotelLocation { get; set; } = string.Empty;
        public string? HotelImage { get; set; }
        public string CityName { get; set; } = string.Empty;

        // Booking
        public DateOnly CheckInDate { get; set; }
        public DateOnly CheckOutDate { get; set; }
        public int Nights { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Payment
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Feedback
        public bool HasFeedback { get; set; }
        public int? FeedbackId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class UserReservationFilterDto
    {
        public string? Status { get; set; }   // Pending | Confirmed | Completed | Cancelled
        public string? Sort { get; set; }     // newest (default) | oldest | checkin
    }
}