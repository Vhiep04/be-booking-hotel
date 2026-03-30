using be_booking_hotel.Repositories.Admin.Interfaces;
using be_booking_hotel.DTOs.Admin;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Admin;
using be_booking_hotel.Services.Admin.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace be_booking_hotel.Services.Admin;

// ===== USER SERVICE =====
public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUserService(UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _userManager = um;
        _roleManager = rm;
    }

    public async Task<AdminPagedResult<AdminUserResponse>> GetUsersAsync(
        int page, int pageSize, string? search, string? role)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                (u.UserName != null && u.UserName.Contains(search)) ||
                (u.FirstName != null && u.FirstName.Contains(search)) ||
                (u.LastName != null && u.LastName.Contains(search)));

        var total = query.Count();
        var users = query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var responses = new List<AdminUserResponse>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role)) continue;
            responses.Add(MapUser(user, roles.ToList()));
        }

        return new AdminPagedResult<AdminUserResponse>
        {
            Items = responses, TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<AdminUserResponse?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;
        var roles = await _userManager.GetRolesAsync(user);
        return MapUser(user, roles.ToList());
    }

    public async Task<AdminApiResponse<AdminUserResponse>> CreateUserAsync(AdminCreateUserRequest request)
    {
        if (await _userManager.FindByEmailAsync(request.Email) != null)
            return AdminApiResponse<AdminUserResponse>.Fail("Email already in use.");

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName ?? "",
            LastName = request.LastName ?? "",
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return AdminApiResponse<AdminUserResponse>.Fail("Failed to create user.",
                result.Errors.Select(e => e.Description).ToList());

        foreach (var r in request.Roles)
            if (await _roleManager.RoleExistsAsync(r))
                await _userManager.AddToRoleAsync(user, r);

        var roles = await _userManager.GetRolesAsync(user);
        return AdminApiResponse<AdminUserResponse>.Ok(MapUser(user, roles.ToList()), "User created.");
    }

    public async Task<AdminApiResponse<AdminUserResponse>> UpdateUserAsync(string id, AdminUpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return AdminApiResponse<AdminUserResponse>.Fail("User not found.");

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return AdminApiResponse<AdminUserResponse>.Fail("Update failed.",
                result.Errors.Select(e => e.Description).ToList());

        if (request.Roles != null)
        {
            var current = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, current);
            foreach (var r in request.Roles)
                if (await _roleManager.RoleExistsAsync(r))
                    await _userManager.AddToRoleAsync(user, r);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return AdminApiResponse<AdminUserResponse>.Ok(MapUser(user, roles.ToList()), "User updated.");
    }

    public async Task<AdminApiResponse<bool>> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return AdminApiResponse<bool>.Fail("User not found.");
        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? AdminApiResponse<bool>.Ok(true, "User deleted.")
            : AdminApiResponse<bool>.Fail("Delete failed.", result.Errors.Select(e => e.Description).ToList());
    }

    public async Task<AdminApiResponse<bool>> ChangePasswordAsync(string id, AdminChangePasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return AdminApiResponse<bool>.Fail("User not found.");
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        return result.Succeeded
            ? AdminApiResponse<bool>.Ok(true, "Password changed.")
            : AdminApiResponse<bool>.Fail("Failed.", result.Errors.Select(e => e.Description).ToList());
    }

    public async Task<IEnumerable<string>> GetAllRolesAsync() =>
        _roleManager.Roles.Select(r => r.Name!).ToList();

    private static AdminUserResponse MapUser(ApplicationUser u, List<string> roles) => new()
    {
        Id = u.Id,
        UserName = u.UserName,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        FullName = $"{u.FirstName} {u.LastName}".Trim(),
        PhoneNumber = u.PhoneNumber,
        EmailConfirmed = u.EmailConfirmed,
        CreatedAt = u.CreatedAt,
        Roles = roles
    };
}

// ===== CITY SERVICE =====
public class AdminCityService : IAdminCityService
{
    private readonly IAdminCityRepository _repo;
    private readonly IAdminCityImageRepository _imageRepo;

    public AdminCityService(IAdminCityRepository repo, IAdminCityImageRepository imageRepo)
    {
        _repo = repo;
        _imageRepo = imageRepo;
    }

    public async Task<AdminPagedResult<AdminCityResponse>> GetCitiesAsync(
        int page, int pageSize, string? search, string? country)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, country);
        return new AdminPagedResult<AdminCityResponse>
        {
            Items = items.Select(MapCity).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<AdminCityResponse?> GetCityByIdAsync(int id)
    {
        var city = await _repo.GetWithDetailsAsync(id);
        return city == null ? null : MapCity(city);
    }

    public async Task<AdminApiResponse<AdminCityResponse>> CreateCityAsync(AdminCityRequest request)
    {
        var city = new City
        {
            Name = request.Name, Country = request.Country,
            Description = request.Description,
            Latitude = request.Latitude, Longitude = request.Longitude
        };
        var created = await _repo.CreateAsync(city);
        return AdminApiResponse<AdminCityResponse>.Ok(MapCity(created), "City created.");
    }

    public async Task<AdminApiResponse<AdminCityResponse>> UpdateCityAsync(int id, AdminCityRequest request)
    {
        var city = await _repo.GetByIdAsync(id);
        if (city == null) return AdminApiResponse<AdminCityResponse>.Fail("City not found.");
        city.Name = request.Name; city.Country = request.Country;
        city.Description = request.Description;
        city.Latitude = request.Latitude; city.Longitude = request.Longitude;
        await _repo.UpdateAsync(city);
        return AdminApiResponse<AdminCityResponse>.Ok(MapCity(city), "City updated.");
    }

    public async Task<AdminApiResponse<bool>> DeleteCityAsync(int id)
    {
        if (!await _repo.ExistsAsync(id)) return AdminApiResponse<bool>.Fail("City not found.");
        await _repo.DeleteAsync(id);
        return AdminApiResponse<bool>.Ok(true, "City deleted.");
    }

    public async Task<IEnumerable<string>> GetCountriesAsync() => await _repo.GetAllCountriesAsync();

    public async Task<AdminApiResponse<AdminImageResponse>> AddCityImageAsync(int cityId, AdminImageRequest request)
    {
        if (!await _repo.ExistsAsync(cityId)) return AdminApiResponse<AdminImageResponse>.Fail("City not found.");
        var img = new CityImage
        {
            CityId = cityId, ImageUrl = request.ImageUrl,
            IsPrimary = request.IsPrimary, DisplayOrder = request.DisplayOrder,
            Description = request.Description
        };
        var created = await _imageRepo.CreateAsync(img);
        if (request.IsPrimary) await _imageRepo.SetPrimaryAsync(cityId, created.ImageId);
        return AdminApiResponse<AdminImageResponse>.Ok(MapImage(created), "Image added.");
    }

    public async Task<AdminApiResponse<bool>> DeleteCityImageAsync(int cityId, int imageId)
    {
        var img = await _imageRepo.GetByIdAsync(imageId);
        if (img == null || img.CityId != cityId) return AdminApiResponse<bool>.Fail("Image not found.");
        await _imageRepo.DeleteAsync(imageId);
        return AdminApiResponse<bool>.Ok(true, "Image deleted.");
    }

    public async Task<AdminApiResponse<bool>> SetPrimaryCityImageAsync(int cityId, int imageId)
    {
        var img = await _imageRepo.GetByIdAsync(imageId);
        if (img == null || img.CityId != cityId) return AdminApiResponse<bool>.Fail("Image not found.");
        await _imageRepo.SetPrimaryAsync(cityId, imageId);
        return AdminApiResponse<bool>.Ok(true, "Primary image set.");
    }

    private static AdminCityResponse MapCity(City c) => new()
    {
        CityId = c.CityId, Name = c.Name, Country = c.Country,
        Description = c.Description, Latitude = c.Latitude, Longitude = c.Longitude,
        HotelCount = c.Hotels?.Count ?? 0,
        Images = c.CityImages?.OrderBy(i => i.DisplayOrder).Select(i => new AdminImageResponse
        {
            ImageId = i.ImageId,
            ImageUrl = i.ImageUrl,
            IsPrimary = i.IsPrimary ?? false,
            DisplayOrder = i.DisplayOrder ?? 0,
            Description = i.Description
        }).ToList() ?? new()
    };

    private static AdminImageResponse MapImage(CityImage i) => new()
    {
        ImageId = i.ImageId,
        ImageUrl = i.ImageUrl,
        IsPrimary = i.IsPrimary ?? false,
        DisplayOrder = i.DisplayOrder ?? 0,
        Description = i.Description
    };
}

// ===== HOTEL SERVICE =====
public class AdminHotelService : IAdminHotelService
{
    private readonly IAdminHotelRepository _repo;
    private readonly IAdminHotelImageRepository _imageRepo;

    public AdminHotelService(IAdminHotelRepository repo, IAdminHotelImageRepository imageRepo)
    {
        _repo = repo;
        _imageRepo = imageRepo;
    }

    public async Task<AdminPagedResult<AdminHotelResponse>> GetHotelsAsync(
        int page, int pageSize, string? search, int? cityId)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, cityId);
        return new AdminPagedResult<AdminHotelResponse>
        {
            Items = items.Select(MapHotel).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<AdminHotelResponse?> GetHotelByIdAsync(int id)
    {
        var hotel = await _repo.GetWithDetailsAsync(id);
        return hotel == null ? null : MapHotel(hotel);
    }

    public async Task<AdminApiResponse<AdminHotelResponse>> CreateHotelAsync(AdminHotelRequest request)
    {
        var hotel = new Hotel
        {
            CityId = request.CityId, Name = request.Name, Location = request.Location,
            Description = request.Description, ImgUrl = request.ImgUrl,
            Latitude = request.Latitude, Longitude = request.Longitude
        };
        var created = await _repo.CreateAsync(hotel);
        var full = await _repo.GetWithDetailsAsync(created.HotelId);
        return AdminApiResponse<AdminHotelResponse>.Ok(MapHotel(full!), "Hotel created.");
    }

    public async Task<AdminApiResponse<AdminHotelResponse>> UpdateHotelAsync(int id, AdminHotelRequest request)
    {
        var hotel = await _repo.GetByIdAsync(id);
        if (hotel == null) return AdminApiResponse<AdminHotelResponse>.Fail("Hotel not found.");
        hotel.CityId = request.CityId; hotel.Name = request.Name;
        hotel.Location = request.Location; hotel.Description = request.Description;
        hotel.ImgUrl = request.ImgUrl; hotel.Latitude = request.Latitude;
        hotel.Longitude = request.Longitude;
        await _repo.UpdateAsync(hotel);
        var full = await _repo.GetWithDetailsAsync(id);
        return AdminApiResponse<AdminHotelResponse>.Ok(MapHotel(full!), "Hotel updated.");
    }

    public async Task<AdminApiResponse<bool>> DeleteHotelAsync(int id)
    {
        if (!await _repo.ExistsAsync(id)) return AdminApiResponse<bool>.Fail("Hotel not found.");
        await _repo.DeleteAsync(id);
        return AdminApiResponse<bool>.Ok(true, "Hotel deleted.");
    }

    public async Task<AdminApiResponse<AdminImageResponse>> AddHotelImageAsync(int hotelId, AdminImageRequest request)
    {
        if (!await _repo.ExistsAsync(hotelId)) return AdminApiResponse<AdminImageResponse>.Fail("Hotel not found.");
        var img = new HotelImage
        {
            HotelId = hotelId, ImageUrl = request.ImageUrl,
            IsPrimary = request.IsPrimary, DisplayOrder = request.DisplayOrder,
            Description = request.Description
        };
        var created = await _imageRepo.CreateAsync(img);
        if (request.IsPrimary) await _imageRepo.SetPrimaryAsync(hotelId, created.ImageId);
        return AdminApiResponse<AdminImageResponse>.Ok(MapImage(created), "Image added.");
    }

    public async Task<AdminApiResponse<bool>> DeleteHotelImageAsync(int hotelId, int imageId)
    {
        var img = await _imageRepo.GetByIdAsync(imageId);
        if (img == null || img.HotelId != hotelId) return AdminApiResponse<bool>.Fail("Image not found.");
        await _imageRepo.DeleteAsync(imageId);
        return AdminApiResponse<bool>.Ok(true, "Image deleted.");
    }

    public async Task<AdminApiResponse<bool>> SetPrimaryHotelImageAsync(int hotelId, int imageId)
    {
        var img = await _imageRepo.GetByIdAsync(imageId);
        if (img == null || img.HotelId != hotelId) return AdminApiResponse<bool>.Fail("Image not found.");
        await _imageRepo.SetPrimaryAsync(hotelId, imageId);
        return AdminApiResponse<bool>.Ok(true, "Primary image set.");
    }

    public async Task<AdminApiResponse<bool>> ReorderHotelImagesAsync(int hotelId, List<int> imageIds)
    {
        await _imageRepo.ReorderAsync(hotelId, imageIds);
        return AdminApiResponse<bool>.Ok(true, "Images reordered.");
    }

    private static AdminHotelResponse MapHotel(Hotel h) => new()
    {
        HotelId = h.HotelId, CityId = h.CityId, CityName = h.City?.Name ?? "",
        Name = h.Name, Location = h.Location, Description = h.Description,
        ImgUrl = h.ImgUrl, Latitude = h.Latitude, Longitude = h.Longitude,
        CreatedAt = h.CreatedAt, RoomCount = h.Rooms?.Count ?? 0,
        AverageRating = h.Feedbacks?.Any() == true ? h.Feedbacks.Average(f => (double)f.Rating) : null,
        FeedbackCount = h.Feedbacks?.Count ?? 0,
        Images = h.HotelImages?.OrderBy(i => i.DisplayOrder).Select(i => new AdminImageResponse
        {
            ImageId = i.ImageId,
            ImageUrl = i.ImageUrl,
            IsPrimary = i.IsPrimary ?? false,
            DisplayOrder = i.DisplayOrder ?? 0,
            Description = i.Description
        }).ToList() ?? new()
    };

    private static AdminImageResponse MapImage(HotelImage i) => new()
    {
        ImageId = i.ImageId,
        ImageUrl = i.ImageUrl,
        IsPrimary = i.IsPrimary ?? false,
        DisplayOrder = i.DisplayOrder ?? 0,
        Description = i.Description
    };
}

// ===== ROOM SERVICE =====
public class AdminRoomService : IAdminRoomService
{
    private readonly IAdminRoomRepository _repo;
    public AdminRoomService(IAdminRoomRepository repo) => _repo = repo;

    public async Task<AdminPagedResult<AdminRoomResponse>> GetRoomsAsync(
        int page, int pageSize, string? search, int? hotelId, string? roomType)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, hotelId, roomType);
        return new AdminPagedResult<AdminRoomResponse>
        {
            Items = items.Select(MapRoom).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<AdminRoomResponse?> GetRoomByIdAsync(int id)
    {
        var room = await _repo.GetWithFacilitiesAsync(id);
        return room == null ? null : MapRoom(room);
    }

    public async Task<IEnumerable<AdminRoomResponse>> GetRoomsByHotelAsync(int hotelId) =>
        (await _repo.GetByHotelAsync(hotelId)).Select(MapRoom);

    public async Task<AdminApiResponse<AdminRoomResponse>> CreateRoomAsync(AdminRoomRequest request)
    {
        var room = new Room
        {
            HotelId = request.HotelId, RoomType = request.RoomType,
            PricePerNight = request.PricePerNight, Capacity = request.Capacity,
            ImgUrl = request.ImgUrl
        };
        var created = await _repo.CreateAsync(room);
        if (request.FacilityIds.Any())
            await _repo.UpdateFacilitiesAsync(created.RoomId, request.FacilityIds);
        var full = await _repo.GetWithFacilitiesAsync(created.RoomId);
        return AdminApiResponse<AdminRoomResponse>.Ok(MapRoom(full!), "Room created.");
    }

    public async Task<AdminApiResponse<AdminRoomResponse>> UpdateRoomAsync(int id, AdminRoomRequest request)
    {
        var room = await _repo.GetByIdAsync(id);
        if (room == null) return AdminApiResponse<AdminRoomResponse>.Fail("Room not found.");
        room.HotelId = request.HotelId; room.RoomType = request.RoomType;
        room.PricePerNight = request.PricePerNight; room.Capacity = request.Capacity;
        room.ImgUrl = request.ImgUrl;
        await _repo.UpdateAsync(room);
        await _repo.UpdateFacilitiesAsync(id, request.FacilityIds);
        var full = await _repo.GetWithFacilitiesAsync(id);
        return AdminApiResponse<AdminRoomResponse>.Ok(MapRoom(full!), "Room updated.");
    }

    public async Task<AdminApiResponse<bool>> DeleteRoomAsync(int id)
    {
        if (await _repo.HasActiveReservationsAsync(id))
            return AdminApiResponse<bool>.Fail("Cannot delete: room has active reservations.");
        if (!await _repo.ExistsAsync(id)) return AdminApiResponse<bool>.Fail("Room not found.");
        await _repo.DeleteAsync(id);
        return AdminApiResponse<bool>.Ok(true, "Room deleted.");
    }

    private static AdminRoomResponse MapRoom(Room r) => new()
    {
        RoomId = r.RoomId, HotelId = r.HotelId, HotelName = r.Hotel?.Name ?? "",
        RoomType = r.RoomType, PricePerNight = r.PricePerNight,
        Capacity = r.Capacity, ImgUrl = r.ImgUrl,
        Facilities = r.Facilities?.Select(f => new AdminFacilityResponse
        {
            FacilityId = f.FacilityId, Name = f.Name
        }).ToList() ?? new()
    };
}

// ===== FACILITY SERVICE =====
public class AdminFacilityService : IAdminFacilityService
{
    private readonly IAdminFacilityRepository _repo;
    public AdminFacilityService(IAdminFacilityRepository repo) => _repo = repo;

    public async Task<AdminPagedResult<AdminFacilityResponse>> GetFacilitiesAsync(
        int page, int pageSize, string? search)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, search);
        return new AdminPagedResult<AdminFacilityResponse>
        {
            Items = items.Select(f => new AdminFacilityResponse { FacilityId = f.FacilityId, Name = f.Name }).ToList(),
            TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<IEnumerable<AdminFacilityResponse>> GetAllAsync() =>
        (await _repo.GetAllAsync()).Select(f => new AdminFacilityResponse { FacilityId = f.FacilityId, Name = f.Name });

    public async Task<AdminFacilityResponse?> GetByIdAsync(int id)
    {
        var f = await _repo.GetByIdAsync(id);
        return f == null ? null : new AdminFacilityResponse { FacilityId = f.FacilityId, Name = f.Name };
    }

    public async Task<AdminApiResponse<AdminFacilityResponse>> CreateAsync(AdminFacilityRequest request)
    {
        var f = await _repo.CreateAsync(new Facility { Name = request.Name });
        return AdminApiResponse<AdminFacilityResponse>.Ok(
            new AdminFacilityResponse { FacilityId = f.FacilityId, Name = f.Name }, "Facility created.");
    }

    public async Task<AdminApiResponse<AdminFacilityResponse>> UpdateAsync(int id, AdminFacilityRequest request)
    {
        var f = await _repo.GetByIdAsync(id);
        if (f == null) return AdminApiResponse<AdminFacilityResponse>.Fail("Not found.");
        f.Name = request.Name;
        await _repo.UpdateAsync(f);
        return AdminApiResponse<AdminFacilityResponse>.Ok(
            new AdminFacilityResponse { FacilityId = f.FacilityId, Name = f.Name }, "Facility updated.");
    }

    public async Task<AdminApiResponse<bool>> DeleteAsync(int id)
    {
        if (await _repo.IsInUseAsync(id))
            return AdminApiResponse<bool>.Fail("Facility is in use by rooms.");
        if (!await _repo.ExistsAsync(id)) return AdminApiResponse<bool>.Fail("Not found.");
        await _repo.DeleteAsync(id);
        return AdminApiResponse<bool>.Ok(true, "Facility deleted.");
    }
}

// ===== RESERVATION SERVICE =====
public class AdminReservationService : IAdminReservationService
{
    private readonly IAdminReservationRepository _repo;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminReservationService(IAdminReservationRepository repo, UserManager<ApplicationUser> um)
    {
        _repo = repo;
        _userManager = um;
    }

    public async Task<AdminPagedResult<AdminReservationResponse>> GetReservationsAsync(
        int page, int pageSize, string? search,
        string? status, int? hotelId, DateTime? fromDate, DateTime? toDate)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, status, hotelId, fromDate, toDate);
        var responses = new List<AdminReservationResponse>();
        foreach (var r in items)
        {
            var user = await _userManager.FindByIdAsync(r.UserId);
            responses.Add(MapReservation(r, user));
        }
        return new AdminPagedResult<AdminReservationResponse>
        {
            Items = responses, TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<AdminReservationResponse?> GetReservationByIdAsync(int id)
    {
        var r = await _repo.GetWithDetailsAsync(id);
        if (r == null) return null;
        var user = await _userManager.FindByIdAsync(r.UserId);
        return MapReservation(r, user);
    }

    public async Task<IEnumerable<AdminReservationResponse>> GetByUserAsync(string userId)
    {
        var reservations = await _repo.GetByUserAsync(userId);
        var user = await _userManager.FindByIdAsync(userId);
        return reservations.Select(r => MapReservation(r, user));
    }

    public async Task<AdminApiResponse<bool>> UpdateStatusAsync(int id, string status)
    {
        var valid = new[] { "Pending", "Confirmed", "Cancelled", "Completed" };
        if (!valid.Contains(status))
            return AdminApiResponse<bool>.Fail($"Invalid status. Valid values: {string.Join(", ", valid)}");
        var updated = await _repo.UpdateStatusAsync(id, status);
        return updated
            ? AdminApiResponse<bool>.Ok(true, "Status updated.")
            : AdminApiResponse<bool>.Fail("Reservation not found.");
    }

    private static AdminReservationResponse MapReservation(Reservation r, ApplicationUser? user) => new()
    {
        ReservationId = r.ReservationId, UserId = r.UserId,
        UserEmail = user?.Email,
        UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
        RoomId = r.RoomId, RoomType = r.Room?.RoomType ?? "",
        HotelName = r.Room?.Hotel?.Name ?? "", CityName = r.Room?.Hotel?.City?.Name ?? "",
        CheckInDate = r.CheckInDate.ToDateTime(TimeOnly.MinValue),
        CheckOutDate = r.CheckOutDate.ToDateTime(TimeOnly.MinValue),
        Nights = r.CheckOutDate.DayNumber - r.CheckInDate.DayNumber,
        TotalPrice = r.TotalPrice, PaymentStatus = r.PaymentStatus ?? "",
        CreatedAt = r.CreatedAt,
        Payments = r.Payments?.Select(p => new AdminPaymentResponse
        {
            PaymentId = p.PaymentId, ReservationId = p.ReservationId,
            PaymentMethod = p.PaymentMethod, Amount = p.Amount,
            PaymentDate = p.PaymentDate, Status = p.Status ?? ""
        }).ToList() ?? new()
    };
}

// ===== FEEDBACK SERVICE =====
public class AdminFeedbackService : IAdminFeedbackService
{
    private readonly IAdminFeedbackRepository _repo;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminFeedbackService(IAdminFeedbackRepository repo, UserManager<ApplicationUser> um)
    {
        _repo = repo;
        _userManager = um;
    }

    public async Task<AdminPagedResult<AdminFeedbackResponse>> GetFeedbacksAsync(
        int page, int pageSize, string? search, int? hotelId, int? rating)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, search, hotelId, rating);
        var responses = new List<AdminFeedbackResponse>();
        foreach (var f in items)
        {
            var user = await _userManager.FindByIdAsync(f.UserId);
            responses.Add(MapFeedback(f, user));
        }
        return new AdminPagedResult<AdminFeedbackResponse>
        {
            Items = responses, TotalCount = total, Page = page, PageSize = pageSize
        };
    }

    public async Task<AdminFeedbackResponse?> GetFeedbackByIdAsync(int id)
    {
        var f = await _repo.GetWithDetailsAsync(id);
        if (f == null) return null;
        var user = await _userManager.FindByIdAsync(f.UserId);
        return MapFeedback(f, user);
    }

    public async Task<AdminApiResponse<AdminFeedbackResponse>> UpdateAsync(int id, AdminUpdateFeedbackRequest request)
    {
        var updated = await _repo.UpdateAsync(id, request.Rating, request.Comment);
        if (!updated) return AdminApiResponse<AdminFeedbackResponse>.Fail("Feedback not found.");
        var f = await GetFeedbackByIdAsync(id);
        return AdminApiResponse<AdminFeedbackResponse>.Ok(f!, "Feedback updated.");
    }

    public async Task<AdminApiResponse<bool>> DeleteAsync(int id)
    {
        var deleted = await _repo.DeleteAsync(id);
        return deleted
            ? AdminApiResponse<bool>.Ok(true, "Feedback deleted.")
            : AdminApiResponse<bool>.Fail("Feedback not found.");
    }

    private static AdminFeedbackResponse MapFeedback(Feedback f, ApplicationUser? user) => new()
    {
        FeedbackId = f.FeedbackId, UserId = f.UserId,
        UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
        UserEmail = user?.Email, HotelId = f.HotelId, HotelName = f.Hotel?.Name ?? "",
        ReservationId = f.ReservationId, Rating = f.Rating, Comment = f.Comment,
        CreatedAt = f.CreatedAt
    };
}

// ===== DASHBOARD SERVICE =====
public class AdminDashboardService : IAdminDashboardService
{
    private readonly IAdminDashboardRepository _repo;
    private readonly IAdminReservationRepository _reservationRepo;
    private readonly IAdminFeedbackRepository _feedbackRepo;

    public AdminDashboardService(
        IAdminDashboardRepository repo,
        IAdminReservationRepository reservationRepo,
        IAdminFeedbackRepository feedbackRepo)
    {
        _repo = repo;
        _reservationRepo = reservationRepo;
        _feedbackRepo = feedbackRepo;
    }

    public async Task<AdminDashboardStats> GetStatsAsync() => new()
    {
        TotalUsers = await _repo.CountUsersAsync(),
        TotalHotels = await _repo.CountHotelsAsync(),
        TotalCities = await _repo.CountCitiesAsync(),
        TotalRooms = await _repo.CountRoomsAsync(),
        TotalReservations = await _repo.CountReservationsAsync(),
        PendingReservations = await _reservationRepo.CountByStatusAsync("Pending"),
        ConfirmedReservations = await _reservationRepo.CountByStatusAsync("Confirmed"),
        TotalRevenue = await _reservationRepo.GetTotalRevenueAsync(),
        RevenueThisMonth = await _reservationRepo.GetRevenueThisMonthAsync(),
        AverageRating = await _feedbackRepo.GetAverageRatingAsync(),
        RevenueChart = await _reservationRepo.GetRevenueChartAsync(12),
        TopHotels = await _repo.GetTopHotelsAsync(5)
    };
}
