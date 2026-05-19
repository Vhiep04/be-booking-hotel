using be_booking_hotel.DTOs.Hotel;
using be_booking_hotel.Models;

namespace be_booking_hotel.Repositories.Interfaces
{
    public interface IHotelRepository
    {
        
        /// Lấy danh sách hotels với filter và pagination
        Task<(List<Hotel> Hotels, int TotalCount)> GetHotelsAsync(HotelFilterDto filter);
        
        /// Lấy chi tiết hotel theo ID
        Task<Hotel?> GetHotelByIdAsync(int hotelId);

        /// Lấy danh sách rooms của hotel
        Task<List<Room>> GetHotelRoomsAsync(int hotelId, DateOnly? checkIn = null, DateOnly? checkOut = null, string? roomType = null);

        /// Kiểm tra hotel có tồn tại không
        Task<bool> HotelExistsAsync(int hotelId);
        
        /// Lấy feedbacks của hotel
        Task<List<Feedback>> GetHotelFeedbacksAsync(int hotelId, int limit = 5);

        /// Tính rating distribution
        Task<Dictionary<int, int>> GetRatingDistributionAsync(int hotelId);
        Task<Room?> GetRoomByIdAsync(int hotelId, int roomId);
    }
}