namespace be_booking_hotel.DTOs.Hotel
{
    /// <summary>
    /// DTO cho chi tiết hotel (detailed view)
    /// </summary>
    public class HotelDetailDto
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImgUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // City details
        public CityDto City { get; set; } = null!;

        // Pricing
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        // Ratings
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public RatingDistribution RatingDistribution { get; set; } = new();

        // Rooms summary
        public int TotalRooms { get; set; }
        public List<string> RoomTypes { get; set; } = new();

        // Facilities (unique từ tất cả rooms)
        public List<string> Facilities { get; set; } = new();

        // Recent feedbacks
        public List<FeedbackDto> RecentFeedbacks { get; set; } = new();
    }

    public class CityDto
    {
        public int CityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }

    public class RatingDistribution
    {
        public int FiveStar { get; set; }
        public int FourStar { get; set; }
        public int ThreeStar { get; set; }
        public int TwoStar { get; set; }
        public int OneStar { get; set; }
    }

    public class FeedbackDto
    {
        public int FeedbackId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}