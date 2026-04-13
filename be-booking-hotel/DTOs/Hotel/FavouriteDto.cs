namespace be_booking_hotel.DTOs.Hotel
{
    public class FavouriteDto
    {
        public int FavouriteId { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string HotelLocation { get; set; } = string.Empty;
        public string? HotelImgUrl { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddFavouriteDto
    {
        public int HotelId { get; set; }
    }
}
