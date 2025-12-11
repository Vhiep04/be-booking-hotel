using System;
using be_booking_hotel.Data;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories.Implementations
{
    public class FacilityRepository : IFacilityRepository
    {
        private readonly HotelBookingContext _context;
        private readonly ILogger<FacilityRepository> _logger;

        public FacilityRepository(
            HotelBookingContext context,
            ILogger<FacilityRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Facility>> GetAllFacilitiesAsync()
        {
            try
            {
                return await _context.Facilities
                    .OrderBy(f => f.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all facilities");
                throw;
            }
        }

        public async Task<Facility?> GetFacilityByIdAsync(int facilityId)
        {
            try
            {
                return await _context.Facilities
                    .FirstOrDefaultAsync(f => f.FacilityId == facilityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility {FacilityId}", facilityId);
                throw;
            }
        }

        public async Task<List<Facility>> GetFacilitiesByIdsAsync(List<int> facilityIds)
        {
            try
            {
                if (facilityIds == null || !facilityIds.Any())
                    return new List<Facility>();

                return await _context.Facilities
                    .Where(f => facilityIds.Contains(f.FacilityId))
                    .OrderBy(f => f.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facilities by IDs");
                throw;
            }
        }

        public async Task<List<Facility>> GetRoomFacilitiesAsync(int roomId)
        {
            try
            {
                // Query trực tiếp từ bảng Facilities qua SQL
                var query = @"
                    SELECT f.*
                    FROM Facilities f
                    INNER JOIN RoomFacilities rf ON f.FacilityId = rf.FacilityId
                    WHERE rf.RoomId = @p0
                    ORDER BY f.Name";

                return await _context.Facilities
                    .FromSqlRaw(query, roomId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facilities for room {RoomId}", roomId);
                throw;
            }
        }

        public async Task<bool> FacilityExistsAsync(int facilityId)
        {
            try
            {
                return await _context.Facilities
                    .AnyAsync(f => f.FacilityId == facilityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if facility {FacilityId} exists", facilityId);
                throw;
            }
        }

        public async Task<List<Facility>> SearchFacilitiesByNameAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllFacilitiesAsync();

                return await _context.Facilities
                    .Where(f => f.Name.Contains(searchTerm))
                    .OrderBy(f => f.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching facilities with term '{SearchTerm}'", searchTerm);
                throw;
            }
        }
    }
}