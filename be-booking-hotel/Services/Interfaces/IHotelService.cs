using be_booking_hotel.DTOs.Hotel;

namespace be_booking_hotel.Services.Interfaces
{
    public interface IHotelService
    {
        /// <summary>
        /// Lấy danh sách hotels với filter
        /// </summary>
        Task<(List<HotelDto> Hotels, int TotalCount, int TotalPages)> GetHotelsAsync(HotelFilterDto filter);

        /// <summary>
        /// Lấy chi tiết hotel với room filtering
        /// </summary>
        Task<HotelDetailDto?> GetHotelDetailAsync(int hotelId, RoomFilterDto filter);

        /// <summary>
        /// Lấy danh sách rooms của hotel
        /// </summary>
        Task<RoomListDto?> GetHotelRoomsAsync(int hotelId);
    }
}