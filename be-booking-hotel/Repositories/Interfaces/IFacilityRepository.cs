using be_booking_hotel.Models;

namespace be_booking_hotel.Repositories.Interfaces
{
    public interface IFacilityRepository
    {
        /// <summary>
        /// Lấy tất cả facilities
        /// </summary>
        Task<List<Facility>> GetAllFacilitiesAsync();

        /// <summary>
        /// Lấy facility theo ID
        /// </summary>
        Task<Facility?> GetFacilityByIdAsync(int facilityId);

        /// <summary>
        /// Lấy facilities theo danh sách IDs
        /// </summary>
        Task<List<Facility>> GetFacilitiesByIdsAsync(List<int> facilityIds);

        /// <summary>
        /// Lấy facilities của một room
        /// </summary>
        Task<List<Facility>> GetRoomFacilitiesAsync(int roomId);

        /// <summary>
        /// Kiểm tra facility có tồn tại không
        /// </summary>
        Task<bool> FacilityExistsAsync(int facilityId);

        /// <summary>
        /// Tìm kiếm facilities theo tên
        /// </summary>
        Task<List<Facility>> SearchFacilitiesByNameAsync(string searchTerm);
    }
}