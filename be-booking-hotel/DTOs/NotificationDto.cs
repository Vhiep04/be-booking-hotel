namespace be_booking_hotel.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Type { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public int? ReservationId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
