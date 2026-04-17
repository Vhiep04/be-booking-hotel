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

// ===== ROOM =====
public interface IAdminRoomRepository : IAdminRepository<Room>
{
    Task<(List<Room> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, int? hotelId, string? roomType);
    Task<Room?> GetWithFacilitiesAsync(int id);
    Task<List<Room>> GetByHotelAsync(int hotelId);
    Task UpdateFacilitiesAsync(int roomId, List<int> facilityIds);
    Task<bool> HasActiveReservationsAsync(int roomId);
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
