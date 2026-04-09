namespace be_booking_hotel.DTOs.Hotel
{
    /// <summary>
    /// DTO cho thông tin phòng
    /// </summary>
    public class RoomDto
    {
        public int RoomId { get; set; }
        public int HotelId { get; set; }
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? ImgUrl { get; set; }

        // Facilities của phòng này
        public List<string> Facilities { get; set; } = new();

        // Availability status
        public bool IsAvailable { get; set; }
        public int BookedDays { get; set; } // Số ngày đã được đặt trong tháng này
    }

    /// <summary>
    /// DTO cho danh sách rooms với filter
    /// </summary>
    public class RoomListDto
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string? HotelLocation { get; set; }
        public string? HotelDescription { get; set; }
        public decimal? HotelLatitude { get; set; }
        public decimal? HotelLongitude { get; set; }
        public List<RoomDto> Rooms { get; set; } = new();
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }
    public class RoomTypeDto
    {
        public int RoomTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? ImgUrl { get; set; }
    }
}