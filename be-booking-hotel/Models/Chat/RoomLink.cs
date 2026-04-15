namespace be_booking_hotel.Models.Chat
{
    public class RoomLink
    {
        public int RoomId { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = "";
        public string RoomType { get; set; } = "";
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? ImgUrl { get; set; }
        public List<string> Facilities { get; set; } = new();
        public bool IsAvailable { get; set; }

        // URLs dẫn về web
        public string DetailUrl { get; set; } = "";
        public string BookingUrl { get; set; } = "";
    }
}
