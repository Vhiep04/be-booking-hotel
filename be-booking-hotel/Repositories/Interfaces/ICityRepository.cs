using be_booking_hotel.Models;

namespace be_booking_hotel.Repositories.Interfaces
{
    public interface ICityRepository
    {
        Task<List<City>> GetAllAsync();
        Task<City?> GetByIdAsync(int id);
        Task<List<City>> SearchByNameOrCountryAsync(string searchTerm);
        Task<List<Hotel>> GetHotelsByCityIdAsync(int cityId);
        Task<Hotel?> GetHotelInCityAsync(int cityId, int hotelId);
        Task<bool> CityExistsAsync(int cityId);
        Task<List<Room>> GetRoomsByHotelIdAsync(int hotelId);
        Task<List<Facility>> GetRoomFacilitiesAsync(int roomId);
        Task<List<CityImage>> GetCityImagesAsync(int cityId);
        Task<List<HotelImage>> GetHotelImagesAsync(int hotelId);
        Task<List<Hotel>> GetAllHotelsAsync();
    }
}