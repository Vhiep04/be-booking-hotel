using be_booking_hotel.DTOs.Admin;
using be_booking_hotel.Models;

namespace be_booking_hotel.Repositories.Admin.Interfaces;

// ===== BASE =====
public interface IAdminRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

// ===== CITY =====
public interface IAdminCityRepository : IAdminRepository<City>
{
    Task<(List<City> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? country);
    Task<City?> GetWithDetailsAsync(int id);
    Task<List<string>> GetAllCountriesAsync();
    Task<bool> HasHotelsAsync(int cityId);
}

// ===== HOTEL =====
public interface IAdminHotelRepository : IAdminRepository<Hotel>
{
    Task<(List<Hotel> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, int? cityId);
    Task<Hotel?> GetWithDetailsAsync(int id);
}

public interface IAdminRoomTypeRepository
{
    Task<(List<RoomType> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search, int? hotelId);

    Task<RoomType?> GetByIdAsync(int id);

    /// <summary>Load ??y ??: Hotel, Rooms, Facilities</summary>
    Task<RoomType?> GetWithDetailsAsync(int id);

    Task<List<RoomType>> GetByHotelAsync(int hotelId);

    Task<RoomType> CreateAsync(RoomType roomType);
    Task UpdateAsync(RoomType roomType);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    /// <summary>Ki?m tra TypeName ?ã t?n t?i trong hotel ch?a (tránh trùng)</summary>
    Task<bool> ExistsByNameAsync(int hotelId, string typeName, int? excludeId = null);

    /// <summary>??ng b? toàn b? facilities c?a m?t RoomType (replace)</summary>
    Task SyncFacilitiesAsync(int roomTypeId, List<int> facilityIds);

    /// <summary>Ki?m tra còn Room instance hay không tr??c khi xóa</summary>
    Task<bool> HasRoomsAsync(int id);
}

// ===== ROOM =====
public interface IAdminRoomRepository
{
    Task<(List<Room> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search, int? hotelId, string? roomType);

    Task<Room?> GetByIdAsync(int id);
    Task<Room?> GetWithFacilitiesAsync(int id);   // load Hotel + RoomType + Facilities
    Task<List<Room>> GetByHotelAsync(int hotelId);
    Task<List<Room>> GetByRoomTypeAsync(int roomTypeId);

    Task<Room> CreateAsync(Room room);
    Task<List<Room>> CreateBulkAsync(List<Room> rooms);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);

    Task<bool> HasActiveReservationsAsync(int roomId);

    /// <summary>Ki?m tra RoomNumber ?ã t?n t?i trong hotel ch?a</summary>
    Task<bool> RoomNumberExistsAsync(int hotelId, string roomNumber, int? excludeRoomId = null);
}

// ===== FACILITY =====
public interface IAdminFacilityRepository : IAdminRepository<Facility>
{
    Task<(List<Facility> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search);
    Task<List<Facility>> GetAllAsync();
    Task<bool> IsInUseAsync(int facilityId);
}

// ===== RESERVATION =====
public interface IAdminReservationRepository
{
    Task<(List<Reservation> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search,
        string? status, int? hotelId, DateTime? fromDate, DateTime? toDate);
    Task<Reservation?> GetByIdAsync(int id);
    Task<Reservation?> GetWithDetailsAsync(int id);
    Task<List<Reservation>> GetByUserAsync(string userId);
    Task<bool> UpdateStatusAsync(int reservationId, string status);
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueThisMonthAsync();
    Task<List<AdminRevenueByMonthDto>> GetRevenueChartAsync(int months);
    Task<int> CountByStatusAsync(string status);
    // Thêm vào interface
    Task<int?> GetRoomIdByReservationAsync(int reservationId);
    // ? ??i tên method c? thành SyncRoomStatusAsync
    Task SyncRoomStatusAsync(int reservationId, string newPaymentStatus);
}

// ===== FEEDBACK =====
public interface IAdminFeedbackRepository
{
    Task<(List<Feedback> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, int? hotelId, int? rating);
    Task<Feedback?> GetByIdAsync(int id);
    Task<Feedback?> GetWithDetailsAsync(int id);
    Task<bool> UpdateAsync(int id, int rating, string? comment);
    Task<bool> DeleteAsync(int id);
    Task<double> GetAverageRatingAsync(int? hotelId = null);
}

// ===== HOTEL IMAGE =====
public interface IAdminHotelImageRepository
{
    Task<List<HotelImage>> GetByHotelAsync(int hotelId);
    Task<HotelImage?> GetByIdAsync(int id);
    Task<HotelImage> CreateAsync(HotelImage image);
    Task<bool> DeleteAsync(int id);
    Task SetPrimaryAsync(int hotelId, int imageId);
    Task ReorderAsync(int hotelId, List<int> imageIds);
    Task<List<HotelImage>> CreateBulkAsync(List<HotelImage> images);
}

// ===== CITY IMAGE =====
public interface IAdminCityImageRepository
{
    Task<List<CityImage>> GetByCityAsync(int cityId);
    Task<CityImage?> GetByIdAsync(int id);
    Task<CityImage> CreateAsync(CityImage image);
    Task<bool> DeleteAsync(int id);
    Task SetPrimaryAsync(int cityId, int imageId);
}

// ===== DASHBOARD =====
public interface IAdminDashboardRepository
{
    Task<int> CountUsersAsync();
    Task<int> CountHotelsAsync();
    Task<int> CountCitiesAsync();
    Task<int> CountRoomsAsync();
    Task<int> CountReservationsAsync();
    Task<List<AdminTopHotelDto>> GetTopHotelsAsync(int count);
    Task<List<AdminRecentBookingDto>> GetRecentBookingsAsync(int count);
    Task<List<AdminPopularCityDto>> GetPopularCitiesAsync(int count);
    Task<List<AdminActivityDto>> GetRecentActivitiesAsync(int count);
}
