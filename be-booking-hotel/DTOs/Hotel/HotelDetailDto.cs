namespace be_booking_hotel.DTOs.Hotel
{
    public class HotelDetailDto
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImgUrl { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime CreatedAt { get; set; }

        public CityDto City { get; set; } = new();

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public RatingDistribution RatingDistribution { get; set; } = new();

        public int TotalRooms { get; set; }
        public List<string> RoomTypes { get; set; } = new();
        public List<string> Facilities { get; set; } = new();

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