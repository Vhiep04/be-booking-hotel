namespace be_booking_hotel.DTOs.Hotel
{
    public class HotelDto
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImgUrl { get; set; }

        // City info
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        // Pricing info
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        // Rating info
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }

        // Room availability
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
