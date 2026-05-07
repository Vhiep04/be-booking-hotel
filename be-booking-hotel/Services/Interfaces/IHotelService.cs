using be_booking_hotel.DTOs.Hotel;
namespace be_booking_hotel.Services.Interfaces
{
    public interface IHotelService
    {
        Task<(List<HotelDto> Hotels, int TotalCount, int TotalPages)> GetHotelsAsync(HotelFilterDto filter);

        Task<HotelDetailDto?> GetHotelDetailAsync(int hotelId);

        // ✅ Thêm checkIn, checkOut với default null (optional)
        Task<RoomListDto?> GetHotelRoomsAsync(int hotelId, DateOnly? checkIn = null, DateOnly? checkOut = null);

        Task<RoomDto?> GetRoomByIdAsync(int hotelId, int roomId, DateOnly? checkIn = null, DateOnly? checkOut = null);
    }
}