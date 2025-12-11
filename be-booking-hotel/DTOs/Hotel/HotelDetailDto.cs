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

        // City info
        public CityDto City { get; set; } = null!;

        // Rating
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public RatingDistribution RatingDistribution { get; set; } = new();

        // Hotel-level facilities (unique từ tất cả rooms)
        public List<string> HotelFacilities { get; set; } = new();

        // ✅ ROOMS LIST - Hiển thị kiểu Booking.com
        public List<RoomDetailDto> Rooms { get; set; } = new();

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

    public class RoomDetailDto
    {
        public int RoomId { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public decimal? OriginalPrice { get; set; } // For showing discount
        public int Capacity { get; set; }
        public string? ImgUrl { get; set; }

        // Room size
        public int? RoomSizeM2 { get; set; }

        // Facilities grouped by category
        public RoomFacilitiesGroup Facilities { get; set; } = new();

        // Availability
        public bool IsAvailable { get; set; }
        public int RoomsLeft { get; set; } // "Chỉ còn 2 phòng"

        // Booking conditions
        public BookingConditions Conditions { get; set; } = new();
    }

    public class RoomFacilitiesGroup
    {
        public List<string> BedTypes { get; set; } = new(); // "1 giường đôi", "2 giường đơn"
        public List<string> RoomFeatures { get; set; } = new(); // "Điều hòa", "WiFi"
        public List<string> Bathroom { get; set; } = new(); // "Bồn tắm", "Vòi sen"
        public List<string> View { get; set; } = new(); // "Sea View", "City View"
    }

    public class BookingConditions
    {
        public bool FreeCancellation { get; set; }
        public DateTime? CancellationDeadline { get; set; }
        public bool BreakfastIncluded { get; set; }
        public bool PrepaymentRequired { get; set; }
    }
}