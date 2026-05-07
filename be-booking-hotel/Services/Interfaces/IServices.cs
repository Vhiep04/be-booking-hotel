using be_booking_hotel.DTOs.Admin;

namespace be_booking_hotel.Services.Admin.Interfaces;

public interface IAdminUserService
{
    Task<AdminPagedResult<AdminUserResponse>> GetUsersAsync(int page, int pageSize, string? search, string? role);
    Task<AdminUserResponse?> GetUserByIdAsync(string id);
    Task<AdminApiResponse<AdminUserResponse>> CreateUserAsync(AdminCreateUserRequest request);
    Task<AdminApiResponse<AdminUserResponse>> UpdateUserAsync(string id, AdminUpdateUserRequest request);
    Task<AdminApiResponse<bool>> DeleteUserAsync(string id);
    Task<AdminApiResponse<bool>> ChangePasswordAsync(string id, AdminChangePasswordRequest request);
    Task<IEnumerable<string>> GetAllRolesAsync();
}

public interface IAdminCityService
{
    Task<AdminPagedResult<AdminCityResponse>> GetCitiesAsync(int page, int pageSize, string? search, string? country);
    Task<AdminCityResponse?> GetCityByIdAsync(int id);
    Task<AdminApiResponse<AdminCityResponse>> CreateCityAsync(AdminCityRequest request);
    Task<AdminApiResponse<AdminCityResponse>> UpdateCityAsync(int id, AdminCityRequest request);
    Task<AdminApiResponse<bool>> DeleteCityAsync(int id);
    Task<IEnumerable<string>> GetCountriesAsync();
    Task<AdminApiResponse<AdminImageResponse>> AddCityImageAsync(int cityId, AdminImageRequest request);
    Task<AdminApiResponse<bool>> DeleteCityImageAsync(int cityId, int imageId);
    Task<AdminApiResponse<bool>> SetPrimaryCityImageAsync(int cityId, int imageId);
}

public interface IAdminHotelService
{
    Task<AdminPagedResult<AdminHotelResponse>> GetHotelsAsync(int page, int pageSize, string? search, int? cityId);
    Task<AdminHotelResponse?> GetHotelByIdAsync(int id);
    Task<AdminApiResponse<AdminHotelResponse>> CreateHotelAsync(AdminHotelRequest request);
    Task<AdminApiResponse<AdminHotelResponse>> UpdateHotelAsync(int id, AdminHotelRequest request);
    Task<AdminApiResponse<bool>> DeleteHotelAsync(int id);
    Task<AdminApiResponse<AdminImageResponse>> AddHotelImageAsync(int hotelId, AdminImageRequest request);
    Task<AdminApiResponse<bool>> DeleteHotelImageAsync(int hotelId, int imageId);
    Task<AdminApiResponse<bool>> SetPrimaryHotelImageAsync(int hotelId, int imageId);
    Task<AdminApiResponse<bool>> ReorderHotelImagesAsync(int hotelId, List<int> imageIds);
    Task<AdminApiResponse<List<AdminImageResponse>>> AddHotelImagesBulkAsync(int hotelId, AdminImageBulkRequest request);
}
public interface IAdminRoomTypeService
{
    Task<AdminPagedResult<AdminRoomTypeResponse>> GetRoomTypesAsync(
        int page, int pageSize, string? search, int? hotelId);

    Task<IEnumerable<AdminRoomTypeResponse>> GetByHotelAsync(int hotelId);

    Task<AdminRoomTypeResponse?> GetByIdAsync(int id);

    Task<AdminApiResponse<AdminRoomTypeResponse>> CreateAsync(AdminRoomTypeRequest request);
    Task<AdminApiResponse<AdminRoomTypeResponse>> UpdateAsync(int id, AdminRoomTypeRequest request);
    Task<AdminApiResponse<bool>> DeleteAsync(int id);
}


public interface IAdminRoomService
{
    Task<AdminPagedResult<AdminRoomResponse>> GetRoomsAsync(
        int page, int pageSize, string? search, int? hotelId, string? roomType);

    Task<AdminRoomResponse?> GetRoomByIdAsync(int id);

    Task<IEnumerable<AdminRoomResponse>> GetRoomsByHotelAsync(int hotelId);

    Task<AdminApiResponse<AdminRoomResponse>> CreateRoomAsync(AdminRoomRequest request);

    /// <summary>Tạo nhiều Room cùng lúc với cùng RoomType</summary>
    Task<AdminApiResponse<List<AdminRoomResponse>>> BulkCreateRoomsAsync(AdminBulkCreateRoomRequest request);

    Task<AdminApiResponse<AdminRoomResponse>> UpdateRoomAsync(int id, AdminRoomRequest request);

    /// <summary>Chỉ đổi trạng thái (Available / Occupied / Maintenance)</summary>
    Task<AdminApiResponse<AdminRoomResponse>> UpdateRoomStatusAsync(int id, AdminUpdateRoomStatusRequest request);

    Task<AdminApiResponse<bool>> DeleteRoomAsync(int id);
}

public interface IAdminFacilityService
{
    Task<AdminPagedResult<AdminFacilityResponse>> GetFacilitiesAsync(int page, int pageSize, string? search);
    Task<IEnumerable<AdminFacilityResponse>> GetAllAsync();
    Task<AdminFacilityResponse?> GetByIdAsync(int id);
    Task<AdminApiResponse<AdminFacilityResponse>> CreateAsync(AdminFacilityRequest request);
    Task<AdminApiResponse<AdminFacilityResponse>> UpdateAsync(int id, AdminFacilityRequest request);
    Task<AdminApiResponse<bool>> DeleteAsync(int id);
}

public interface IAdminReservationService
{
    Task<AdminPagedResult<AdminReservationResponse>> GetReservationsAsync(int page, int pageSize,
        string? search, string? status, int? hotelId, DateTime? fromDate, DateTime? toDate);
    Task<AdminReservationResponse?> GetReservationByIdAsync(int id);
    Task<IEnumerable<AdminReservationResponse>> GetByUserAsync(string userId);
    Task<AdminApiResponse<bool>> UpdateStatusAsync(int id, string status);
}

public interface IAdminFeedbackService
{
    Task<AdminPagedResult<AdminFeedbackResponse>> GetFeedbacksAsync(int page, int pageSize,
        string? search, int? hotelId, int? rating);
    Task<AdminFeedbackResponse?> GetFeedbackByIdAsync(int id);
    Task<AdminApiResponse<AdminFeedbackResponse>> UpdateAsync(int id, AdminUpdateFeedbackRequest request);
    Task<AdminApiResponse<bool>> DeleteAsync(int id);
}

public interface IAdminDashboardService
{
    Task<AdminDashboardStats> GetStatsAsync();
}
