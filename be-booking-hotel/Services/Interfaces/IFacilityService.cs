using be_booking_hotel.DTOs.Facility;

namespace be_booking_hotel.Services.Interfaces
{
    public interface IFacilityService
    {
        /// <summary>
        /// Lấy tất cả facilities
        /// </summary>
        Task<List<FacilityDto>> GetAllFacilitiesAsync();

        /// <summary>
        /// Lấy facility theo ID
        /// </summary>
        Task<FacilityDto?> GetFacilityByIdAsync(int facilityId);

        /// <summary>
        /// Lấy facilities của một room
        /// </summary>
        Task<List<FacilityDto>> GetRoomFacilitiesAsync(int roomId);

        /// <summary>
        /// Tìm kiếm facilities theo tên
        /// </summary>
        Task<List<FacilityDto>> SearchFacilitiesAsync(string searchTerm);
        
    }
}