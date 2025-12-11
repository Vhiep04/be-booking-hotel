namespace be_booking_hotel.DTOs
{
    // DTO cho filtering hotels
    // FIXED: Đổi DateTime sang DateOnly để tương thích với DB type DATE
    public class HotelFilterDto
    {
        public string? CityName { get; set; }
        public DateOnly? CheckIn { get; set; }
        public DateOnly? CheckOut { get; set; }
        public string? BedType { get; set; }
        public List<int>? Facilities { get; set; }
        public string? SortBy { get; set; } // price_asc, price_desc, rating_desc, name_asc
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    // DTO cho City response
    public class CityDto
    {
        public int CityId { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string? Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public List<CityImageDto> Images { get; set; } = new List<CityImageDto>();
    }

    // DTO cho City Image
    public class CityImageDto
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string? Description { get; set; }
    }

    // DTO cho Hotel trong City
    public class HotelInCityDto
    {
        public int HotelId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string? Description { get; set; }
        public decimal MinPricePerNight { get; set; }
        public decimal MaxPricePerNight { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public List<HotelImageDto> Images { get; set; } = new List<HotelImageDto>();
        public List<string> AvailableRoomTypes { get; set; } = new List<string>();
        public List<FacilityDto> PopularFacilities { get; set; } = new List<FacilityDto>();
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    // DTO cho Hotel Image
    public class HotelImageDto
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string? Description { get; set; }
    }

    // DTO cho Room
    public class RoomDto
    {
        public int RoomId { get; set; }
        public string RoomType { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? ImgUrl { get; set; }
        public List<FacilityDto> Facilities { get; set; } = new List<FacilityDto>();
        public bool IsAvailable { get; set; }
    }

    // DTO cho Facility
    public class FacilityDto
    {
        public int FacilityId { get; set; }
        public string Name { get; set; }
    }

    // DTO cho kết quả filter hotels
    public class CityHotelsResultDto
    {
        public string CityName { get; set; }
        public List<HotelInCityDto> Hotels { get; set; } = new List<HotelInCityDto>();
    }

    // DTO cho City Statistics
    public class CityStatsDto
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int TotalHotels { get; set; }
        public int TotalRooms { get; set; }
        public decimal MinPricePerNight { get; set; }
        public decimal MaxPricePerNight { get; set; }
        public decimal AveragePricePerNight { get; set; }
        public Dictionary<string, int> RoomTypeDistribution { get; set; } = new Dictionary<string, int>();
        public List<PopularFacilityDto> PopularFacilities { get; set; } = new List<PopularFacilityDto>();
    }

    // DTO cho Popular Facility với count
    public class PopularFacilityDto
    {
        public int FacilityId { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}