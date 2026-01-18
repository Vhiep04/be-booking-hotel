using be_booking_hotel.DTOs;

namespace be_booking_hotel.Services.Interfaces
{
    public interface ICityService
    {
        Task<List<CityDto>> GetAllCitiesAsync();
        Task<CityDto?> GetCityByIdAsync(int id);
        Task<List<CityDto>> SearchCitiesAsync(string searchTerm);
        Task<CityHotelsResultDto> GetCityHotelsWithFilterAsync(int cityId, HotelFilterDto filter);
        Task<AllHotelsResultDto> GetAllHotelsWithFilterAsync(HotelFilterDto filter); // ← Method mới
        Task<HotelInCityDto?> GetHotelInCityAsync(int cityId, int hotelId);
        Task<CityStatsDto?> GetCityStatsAsync(int cityId);
    }
}