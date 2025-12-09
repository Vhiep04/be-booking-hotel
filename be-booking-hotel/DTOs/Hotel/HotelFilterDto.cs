namespace be_booking_hotel.DTOs.Hotel
{
    /// <summary>
    /// DTO cho filter và pagination
    /// </summary>
    public class HotelFilterDto
    {
        public string? Search { get; set; } // Search by name or location
        public int? CityId { get; set; }
        public string? Country { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinRating { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;

        // Sorting
        public string? SortBy { get; set; } // "price_asc", "price_desc", "rating", "name"
    }
}