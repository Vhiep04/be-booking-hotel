using be_booking_hotel.DTOs.Hotel;

namespace be_booking_hotel.Services.Interfaces
{
    public interface IHotelService
    {
        
        /// Lấy danh sách hotels với filter
        Task<(List<HotelDto> Hotels, int TotalCount, int TotalPages)> GetHotelsAsync(HotelFilterDto filter);
        
        /// Lấy chi tiết hotel
        Task<HotelDetailDto?> GetHotelDetailAsync(int hotelId);

        /// Lấy danh sách rooms của hotel
        Task<RoomListDto?> GetHotelRoomsAsync(int hotelId);

        Task<RoomDto?> GetRoomByIdAsync(int hotelId, int roomId);
    }
}