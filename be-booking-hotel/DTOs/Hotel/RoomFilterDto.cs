namespace be_booking_hotel.DTOs.Hotel
{
    /// <summary>
    /// DTO để filter rooms trong hotel detail page
    /// </summary>
    public class RoomFilterDto
    {
        // ===== SEARCH FORM =====
        public DateOnly? CheckIn { get; set; }
        public DateOnly? CheckOut { get; set; }

        /// <summary>
        /// Bed type filter: "Single", "Double", "King", "Twin", "Suite"
        /// </summary>
        public string? BedType { get; set; }

        // ===== FACILITIES (checkboxes) =====
        /// <summary>
        /// Danh sách FacilityId hoặc Facility Names được chọn từ checkboxes
        /// VD: ["WiFi", "Pool", "Breakfast Included", "Sea View"]
        /// </summary>
        public List<string>? Facilities { get; set; }

        // ===== SORTING =====
        /// <summary>
        /// Sort options: "price_asc", "price_desc", "rating" (default: "price_asc")
        /// </summary>
        public string? SortBy { get; set; } = "price_asc";

        // ===== PRICE RANGE (optional) =====
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}