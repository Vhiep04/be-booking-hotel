using be_booking_hotel.DTOs.Admin;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Admin.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories.Admin;

// ===== BASE =====
public abstract class AdminBaseRepository<T> : IAdminRepository<T> where T : class
{
    protected readonly HotelBookingContext _ctx;
    protected AdminBaseRepository(HotelBookingContext ctx) => _ctx = ctx;

    public virtual async Task<T?> GetByIdAsync(int id) => await _ctx.Set<T>().FindAsync(id);

    public virtual async Task<T> CreateAsync(T entity)
    {
        _ctx.Set<T>().Add(entity);
        await _ctx.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _ctx.Set<T>().Update(entity);
        await _ctx.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null) return false;
        _ctx.Set<T>().Remove(entity);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> ExistsAsync(int id) => await GetByIdAsync(id) != null;
}

// ===== CITY =====
public class AdminCityRepository : AdminBaseRepository<City>, IAdminCityRepository
{
    public AdminCityRepository(HotelBookingContext ctx) : base(ctx) { }

    public async Task<(List<City> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? country)
    {
        var query = _ctx.Cities
            .Include(c => c.Hotels)
            .Include(c => c.CityImages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || c.Country.Contains(search));
        if (!string.IsNullOrWhiteSpace(country))
            query = query.Where(c => c.Country == country);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<City?> GetWithDetailsAsync(int id) =>
        await _ctx.Cities
            .Include(c => c.Hotels).ThenInclude(h => h.RoomTypes)
            .Include(c => c.CityImages)
            .FirstOrDefaultAsync(c => c.CityId == id);

    public async Task<List<string>> GetAllCountriesAsync() =>
        await _ctx.Cities.Select(c => c.Country).Distinct().OrderBy(c => c).ToListAsync();

    public async Task<bool> HasHotelsAsync(int cityId) =>
        await _ctx.Hotels.AnyAsync(h => h.CityId == cityId);
}

// ===== HOTEL =====
public class AdminHotelRepository : AdminBaseRepository<Hotel>, IAdminHotelRepository
{
    public AdminHotelRepository(HotelBookingContext ctx) : base(ctx) { }

    public async Task<(List<Hotel> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, int? cityId)
    {
        var query = _ctx.Hotels
            .Include(h => h.City)
            .Include(h => h.RoomTypes)
            .Include(h => h.Feedbacks)
            .Include(h => h.HotelImages)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(h => h.Name.Contains(search) || h.Location.Contains(search));
        if (cityId.HasValue)
            query = query.Where(h => h.CityId == cityId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(h => h.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Hotel?> GetWithDetailsAsync(int id) =>
        await _ctx.Hotels
            .Include(h => h.City)
            .Include(h => h.RoomTypes).ThenInclude(r => r.Facilities)
            .Include(h => h.HotelImages)
            .Include(h => h.Feedbacks)
            .FirstOrDefaultAsync(h => h.HotelId == id);
}

// ===== ROOM =====
public class AdminRoomRepository : AdminBaseRepository<Room>, IAdminRoomRepository
{
    public AdminRoomRepository(HotelBookingContext ctx) : base(ctx) { }

    public async Task<(List<Room> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, int? hotelId, string? roomType)
    {
        var query = _ctx.Rooms
            .Include(r => r.Hotel).ThenInclude(h => h.City)
            .Include(r => r.RoomType).ThenInclude(rt => rt.Facilities)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.RoomType.TypeName.Contains(search) || r.Hotel.Name.Contains(search));
        if (hotelId.HasValue)
            query = query.Where(r => r.HotelId == hotelId.Value);
        if (!string.IsNullOrWhiteSpace(roomType))
            query = query.Where(r => r.RoomType.TypeName == roomType);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Hotel.Name).ThenBy(r => r.RoomType.PricePerNight)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Room?> GetWithFacilitiesAsync(int id) =>
        await _ctx.Rooms
            .Include(r => r.Hotel).ThenInclude(h => h.City)
            .Include(r => r.RoomType).ThenInclude(rt => rt.Facilities)
            .FirstOrDefaultAsync(r => r.RoomId == id);

    public async Task<List<Room>> GetByHotelAsync(int hotelId) =>
        await _ctx.Rooms
            .Include(r => r.RoomType).ThenInclude(rt => rt.Facilities)
            .Where(r => r.HotelId == hotelId)
            .OrderBy(r => r.RoomType.PricePerNight)
            .ToListAsync();

    public async Task UpdateFacilitiesAsync(int roomTypeId, List<int> facilityIds)
    {
        var roomType = await _ctx.RoomTypes
            .Include(rt => rt.Facilities)
            .FirstOrDefaultAsync(rt => rt.RoomTypeId == roomTypeId);
        if (roomType == null) return;

        roomType.Facilities.Clear();
        var facilities = await _ctx.Facilities
            .Where(f => facilityIds.Contains(f.FacilityId))
            .ToListAsync();
        foreach (var f in facilities)
            roomType.Facilities.Add(f);

        await _ctx.SaveChangesAsync();
    }

    public async Task<bool> HasActiveReservationsAsync(int roomId) =>
        await _ctx.Reservations.AnyAsync(r =>
            r.RoomId == roomId &&
            r.CheckOutDate >= DateOnly.FromDateTime(DateTime.Today) &&
            r.PaymentStatus != "Cancelled");
}

// ===== FACILITY =====
public class AdminFacilityRepository : AdminBaseRepository<Facility>, IAdminFacilityRepository
{
    public AdminFacilityRepository(HotelBookingContext ctx) : base(ctx) { }

    public async Task<(List<Facility> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search)
    {
        var query = _ctx.Facilities.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f => f.Name.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Facility>> GetAllAsync() =>
        await _ctx.Facilities.OrderBy(f => f.Name).ToListAsync();

    public async Task<bool> IsInUseAsync(int facilityId) =>
        await _ctx.Facilities
            .Where(f => f.FacilityId == facilityId)
            .AnyAsync(f => f.RoomTypes.Any());
}

// ===== RESERVATION =====
public class AdminReservationRepository : IAdminReservationRepository
{
    private readonly HotelBookingContext _ctx;
    public AdminReservationRepository(HotelBookingContext ctx) => _ctx = ctx;

    public async Task<(List<Reservation> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search,
        string? status, int? hotelId, DateTime? fromDate, DateTime? toDate)
    {
        var query = _ctx.Reservations
            .Include(r => r.Room).ThenInclude(room => room.Hotel).ThenInclude(h => h.City)
            .Include(r => r.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r =>
                r.UserId.Contains(search) || r.Room.Hotel.Name.Contains(search));
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.PaymentStatus == status);
        if (hotelId.HasValue)
            query = query.Where(r => r.Room.HotelId == hotelId.Value);
        if (fromDate.HasValue)
            query = query.Where(r => r.CheckInDate >= DateOnly.FromDateTime(fromDate.Value));
        if (toDate.HasValue)
            query = query.Where(r => r.CheckOutDate <= DateOnly.FromDateTime(toDate.Value));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Reservation?> GetByIdAsync(int id) =>
        await _ctx.Reservations.FindAsync(id);

    public async Task<Reservation?> GetWithDetailsAsync(int id) =>
        await _ctx.Reservations
            .Include(r => r.Room).ThenInclude(room => room.Hotel).ThenInclude(h => h.City)
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.ReservationId == id);

    public async Task<List<Reservation>> GetByUserAsync(string userId) =>
        await _ctx.Reservations
            .Include(r => r.Room).ThenInclude(room => room.Hotel)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<bool> UpdateStatusAsync(int reservationId, string status)
    {
        var r = await _ctx.Reservations.FindAsync(reservationId);
        if (r == null) return false;
        r.PaymentStatus = status;
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalRevenueAsync() =>
        await _ctx.Payments.Where(p => p.Status == "Success").SumAsync(p => p.Amount);

    public async Task<decimal> GetRevenueThisMonthAsync()
    {
        var now = DateTime.UtcNow;
        return await _ctx.Payments
            .Where(p => p.Status == "Success"
                
                && p.PaymentDate.Year == now.Year
                && p.PaymentDate.Month == now.Month)
            .SumAsync(p => p.Amount);
    }

    public async Task<List<AdminRevenueByMonthDto>> GetRevenueChartAsync(int months)
    {
        var from = DateTime.UtcNow.AddMonths(-months + 1);
        var data = await _ctx.Payments
            .Where(p => p.Status == "Success"
                
                && p.PaymentDate >= from)
            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
            .Select(g => new AdminRevenueByMonthDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(p => p.Amount),
                ReservationCount = g.Count()
            })
            .OrderBy(r => r.Year).ThenBy(r => r.Month)
            .ToListAsync();

        foreach (var d in data)
            d.MonthName = new DateTime(d.Year, d.Month, 1).ToString("MMM yyyy");

        return data;
    }

    public async Task<int> CountByStatusAsync(string status) =>
        await _ctx.Reservations.CountAsync(r => r.PaymentStatus == status);
}

// ===== FEEDBACK =====
public class AdminFeedbackRepository : IAdminFeedbackRepository
{
    private readonly HotelBookingContext _ctx;
    public AdminFeedbackRepository(HotelBookingContext ctx) => _ctx = ctx;

    public async Task<(List<Feedback> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, int? hotelId, int? rating)
    {
        var query = _ctx.Feedbacks.Include(f => f.Hotel).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f =>
                (f.Comment != null && f.Comment.Contains(search)) ||
                f.Hotel.Name.Contains(search));
        if (hotelId.HasValue)
            query = query.Where(f => f.HotelId == hotelId.Value);
        if (rating.HasValue)
            query = query.Where(f => f.Rating == rating.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Feedback?> GetByIdAsync(int id) => await _ctx.Feedbacks.FindAsync(id);

    public async Task<Feedback?> GetWithDetailsAsync(int id) =>
        await _ctx.Feedbacks
            .Include(f => f.Hotel)
            .Include(f => f.Reservation)
            .FirstOrDefaultAsync(f => f.FeedbackId == id);

    public async Task<bool> UpdateAsync(int id, int rating, string? comment)
    {
        var fb = await _ctx.Feedbacks.FindAsync(id);
        if (fb == null) return false;
        fb.Rating = rating;
        fb.Comment = comment;
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var fb = await _ctx.Feedbacks.FindAsync(id);
        if (fb == null) return false;
        _ctx.Feedbacks.Remove(fb);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task<double> GetAverageRatingAsync(int? hotelId = null)
    {
        var query = _ctx.Feedbacks.AsQueryable();
        if (hotelId.HasValue) query = query.Where(f => f.HotelId == hotelId.Value);
        var list = await query.Select(f => (double)f.Rating).ToListAsync();
        return list.Count > 0 ? list.Average() : 0;
    }
}

// ===== HOTEL IMAGE =====
public class AdminHotelImageRepository : IAdminHotelImageRepository
{
    private readonly HotelBookingContext _ctx;
    public AdminHotelImageRepository(HotelBookingContext ctx) => _ctx = ctx;

    public async Task<List<HotelImage>> GetByHotelAsync(int hotelId) =>
        await _ctx.HotelImages.Where(i => i.HotelId == hotelId).OrderBy(i => i.DisplayOrder).ToListAsync();

    public async Task<HotelImage?> GetByIdAsync(int id) => await _ctx.HotelImages.FindAsync(id);

    public async Task<HotelImage> CreateAsync(HotelImage image)
    {
        _ctx.HotelImages.Add(image);
        await _ctx.SaveChangesAsync();
        return image;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var img = await _ctx.HotelImages.FindAsync(id);
        if (img == null) return false;
        _ctx.HotelImages.Remove(img);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task SetPrimaryAsync(int hotelId, int imageId)
    {
        var images = await _ctx.HotelImages.Where(i => i.HotelId == hotelId).ToListAsync();
        foreach (var img in images) img.IsPrimary = img.ImageId == imageId;
        await _ctx.SaveChangesAsync();
    }

    public async Task ReorderAsync(int hotelId, List<int> imageIds)
    {
        var images = await _ctx.HotelImages.Where(i => i.HotelId == hotelId).ToListAsync();
        for (int i = 0; i < imageIds.Count; i++)
        {
            var img = images.FirstOrDefault(x => x.ImageId == imageIds[i]);
            if (img != null) img.DisplayOrder = i;
        }
        await _ctx.SaveChangesAsync();
    }
    public async Task<List<HotelImage>> CreateBulkAsync(List<HotelImage> images)
    {
        _ctx.HotelImages.AddRange(images);
        await _ctx.SaveChangesAsync();
        return images;
    }
}

// ===== CITY IMAGE =====
public class AdminCityImageRepository : IAdminCityImageRepository
{
    private readonly HotelBookingContext _ctx;
    public AdminCityImageRepository(HotelBookingContext ctx) => _ctx = ctx;

    public async Task<List<CityImage>> GetByCityAsync(int cityId) =>
        await _ctx.CityImages.Where(i => i.CityId == cityId).OrderBy(i => i.DisplayOrder).ToListAsync();

    public async Task<CityImage?> GetByIdAsync(int id) => await _ctx.CityImages.FindAsync(id);

    public async Task<CityImage> CreateAsync(CityImage image)
    {
        _ctx.CityImages.Add(image);
        await _ctx.SaveChangesAsync();
        return image;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var img = await _ctx.CityImages.FindAsync(id);
        if (img == null) return false;
        _ctx.CityImages.Remove(img);
        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task SetPrimaryAsync(int cityId, int imageId)
    {
        var images = await _ctx.CityImages.Where(i => i.CityId == cityId).ToListAsync();
        foreach (var img in images) img.IsPrimary = img.ImageId == imageId;
        await _ctx.SaveChangesAsync();
    }
}

// ===== DASHBOARD =====
public class AdminDashboardRepository : IAdminDashboardRepository
{
    private readonly HotelBookingContext _ctx;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminDashboardRepository(HotelBookingContext ctx, UserManager<ApplicationUser> userManager)
    {
        _ctx = ctx;
        _userManager = userManager;
    }

    public async Task<int> CountUsersAsync() => await _ctx.Users.CountAsync();
    public async Task<int> CountHotelsAsync() => await _ctx.Hotels.CountAsync();
    public async Task<int> CountCitiesAsync() => await _ctx.Cities.CountAsync();
    public async Task<int> CountRoomsAsync() => await _ctx.Rooms.CountAsync();
    public async Task<int> CountReservationsAsync() => await _ctx.Reservations.CountAsync();

    public async Task<List<AdminTopHotelDto>> GetTopHotelsAsync(int count) =>
        await _ctx.Hotels
            .Include(h => h.City)
            .Include(h => h.RoomTypes).ThenInclude(rt => rt.Rooms).ThenInclude(r => r.Reservations).ThenInclude(res => res.Payments)
            .Include(h => h.Feedbacks)
            .OrderByDescending(h => h.RoomTypes.SelectMany(rt => rt.Rooms).SelectMany(r => r.Reservations).Count())
            .Take(count)
            .Select(h => new AdminTopHotelDto
            {
                HotelId = h.HotelId,
                Name = h.Name,
                CityName = h.City.Name,
                ReservationCount = h.RoomTypes.SelectMany(rt => rt.Rooms).SelectMany(r => r.Reservations).Count(),
                Revenue = h.RoomTypes
                    .SelectMany(rt => rt.Rooms)
                    .SelectMany(r => r.Reservations)
                    .SelectMany(res => res.Payments)
                    .Where(p => p.Status == "Success")
                    .Sum(p => p.Amount),
                AverageRating = h.Feedbacks.Any()
                    ? h.Feedbacks.Average(f => (double)f.Rating)
                    : 0
            })
            .ToListAsync();

    public async Task<List<AdminRecentBookingDto>> GetRecentBookingsAsync(int count)
    {
        var reservations = await _ctx.Reservations
            .Include(r => r.Room).ThenInclude(room => room.Hotel).ThenInclude(h => h.City)
            .Include(r => r.Room).ThenInclude(room => room.Hotel).ThenInclude(h => h.HotelImages)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();

        var userIds = reservations.Select(r => r.UserId).Distinct().ToList();

        var users = await _ctx.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .ToDictionaryAsync(u => u.Id);

        return reservations.Select(r =>
        {
            var user = users.GetValueOrDefault(r.UserId);
            var guestName = user != null
                ? $"{user.FirstName} {user.LastName}".Trim()
                : "Unknown";

            return new AdminRecentBookingDto
            {
                ReservationId = r.ReservationId,
                BookingCode = $"BK-{r.CreatedAt.Year}-{r.ReservationId:D3}",
                HotelName = r.Room.Hotel.Name,
                HotelImage = r.Room.Hotel.HotelImages
                    .Where(i => i.IsPrimary == true)
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault() ?? r.Room.Hotel.ImgUrl ?? "",
                CityName = r.Room.Hotel.City.Name,
                GuestName = guestName,
                CheckInDate = r.CheckInDate.ToDateTime(TimeOnly.MinValue),
                CheckOutDate = r.CheckOutDate.ToDateTime(TimeOnly.MinValue),
                Amount = r.TotalPrice,
                Status = r.PaymentStatus ?? "Pending"
            };
        }).ToList();
    }

    public async Task<List<AdminPopularCityDto>> GetPopularCitiesAsync(int count)
    {
        var totalBookings = await _ctx.Reservations.CountAsync();

        return await _ctx.Cities
            .Include(c => c.Hotels)
                .ThenInclude(h => h.RoomTypes)
                .ThenInclude(rt => rt.Rooms)
                .ThenInclude(r => r.Reservations)
            .Select(c => new
            {
                c.CityId,
                c.Name,
                HotelCount = c.Hotels.Count,
                BookingCount = c.Hotels
                .SelectMany(h => h.RoomTypes)
                .SelectMany(rt => rt.Rooms)
                .SelectMany(r => r.Reservations)
                .Count()
            })
            .OrderByDescending(c => c.BookingCount)
            .Take(count)
            .Select(c => new AdminPopularCityDto
            {
                CityId = c.CityId,
                Name = c.Name,
                HotelCount = c.HotelCount,
                BookingCount = c.BookingCount,
                Percentage = totalBookings > 0
                    ? (int)Math.Round((double)c.BookingCount / totalBookings * 100)
                    : 0
            })
            .ToListAsync();
    }
    public async Task<List<AdminActivityDto>> GetRecentActivitiesAsync(int count)
    {
        var bookings = await _ctx.Reservations
            .Include(r => r.Room).ThenInclude(room => room.Hotel)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new AdminActivityDto
            {
                Message = $"New booking at {r.Room.Hotel.Name}",
                Time = r.CreatedAt,
                Type = "booking"
            })
            .ToListAsync();

        var payments = await _ctx.Payments
            .OrderByDescending(p => p.PaymentDate)
            .Take(5)
            .Select(p => new AdminActivityDto
            {
                Message = $"Payment received - ${p.Amount}",
                Time = p.PaymentDate,
                Type = "payment"
            })
            .ToListAsync();

        return bookings
            .Concat(payments)
            .OrderByDescending(a => a.Time)
            .Take(count)
            .ToList();
    }
}
