using be_booking_hotel.DTOs.Facility;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;

namespace be_booking_hotel.Services.Implementations
{
    public class FacilityService : IFacilityService
    {
        private readonly IFacilityRepository _facilityRepository;
        private readonly ILogger<FacilityService> _logger;

        public FacilityService(
            IFacilityRepository facilityRepository,
            ILogger<FacilityService> logger)
        {
            _facilityRepository = facilityRepository;
            _logger = logger;
        }

        public async Task<List<FacilityDto>> GetAllFacilitiesAsync()
        {
            try
            {
                var facilities = await _facilityRepository.GetAllFacilitiesAsync();

                return facilities.Select(f => new FacilityDto
                {
                    FacilityId = f.FacilityId,
                    Name = f.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllFacilitiesAsync");
                throw;
            }
        }

        public async Task<FacilityDto?> GetFacilityByIdAsync(int facilityId)
        {
            try
            {
                var facility = await _facilityRepository.GetFacilityByIdAsync(facilityId);

                if (facility == null)
                    return null;

                return new FacilityDto
                {
                    FacilityId = facility.FacilityId,
                    Name = facility.Name
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFacilityByIdAsync for facility {FacilityId}", facilityId);
                throw;
            }
        }

        public async Task<List<FacilityDto>> GetRoomFacilitiesAsync(int roomId)
        {
            try
            {
                var facilities = await _facilityRepository.GetRoomFacilitiesAsync(roomId);

                return facilities.Select(f => new FacilityDto
                {
                    FacilityId = f.FacilityId,
                    Name = f.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRoomFacilitiesAsync for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<List<FacilityDto>> SearchFacilitiesAsync(string searchTerm)
        {
            try
            {
                var facilities = await _facilityRepository.SearchFacilitiesByNameAsync(searchTerm);

                return facilities.Select(f => new FacilityDto
                {
                    FacilityId = f.FacilityId,
                    Name = f.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchFacilitiesAsync with term '{SearchTerm}'", searchTerm);
                throw;
            }
        }
    }
}