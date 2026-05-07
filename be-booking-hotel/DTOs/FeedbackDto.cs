namespace be_booking_hotel.DTOs
{
    public class FeedbackDto
    {
        public int FeedbackId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public int? ReservationId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateFeedbackDto
    {
        public int HotelId { get; set; }
        public int ReservationId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateFeedbackDto
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}