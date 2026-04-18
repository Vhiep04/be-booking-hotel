using be_booking_hotel.DTOs.Admin;
using be_booking_hotel.Models;
using be_booking_hotel.Services.Admin.Interfaces;
using be_booking_hotel.Services.Implements;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_booking_hotel.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
public abstract class AdminBaseController : ControllerBase { }

// ================================================================
// DASHBOARD
// ================================================================
[Route("api/admin/dashboard")]
public class AdminDashboardController : AdminBaseController
{
    private readonly IAdminDashboardService _service;
    public AdminDashboardController(IAdminDashboardService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<AdminApiResponse<AdminDashboardStats>>> GetStats()
        => Ok(AdminApiResponse<AdminDashboardStats>.Ok(await _service.GetStatsAsync()));
}

// ================================================================
// USERS
// ================================================================
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _service;
    public AdminUsersController(IAdminUserService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? role = null)
        => Ok(AdminApiResponse<AdminPagedResult<AdminUserResponse>>.Ok(
            await _service.GetUsersAsync(page, pageSize, search, role)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _service.GetUserByIdAsync(id);
        return user == null
            ? NotFound(AdminApiResponse<AdminUserResponse>.Fail("User not found."))
            : Ok(AdminApiResponse<AdminUserResponse>.Ok(user));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminCreateUserRequest request)
    {
        var result = await _service.CreateUserAsync(request);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] AdminUpdateUserRequest request)
    {
        var result = await _service.UpdateUserAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteUserAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] AdminChangePasswordRequest request)
    {
        var result = await _service.ChangePasswordAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
        => Ok(AdminApiResponse<IEnumerable<string>>.Ok(await _service.GetAllRolesAsync()));

    [HttpGet("{id}/reservations")]
    public async Task<IActionResult> GetReservations(string id, [FromServices] IAdminReservationService reservationService)
        => Ok(AdminApiResponse<IEnumerable<AdminReservationResponse>>.Ok(
            await reservationService.GetByUserAsync(id)));
}

// ================================================================
// CITIES
// ================================================================
[Route("api/admin/cities")]
public class AdminCitiesController : AdminBaseController
{
    private readonly IAdminCityService _service;
    public AdminCitiesController(IAdminCityService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? country = null)
        => Ok(AdminApiResponse<AdminPagedResult<AdminCityResponse>>.Ok(
            await _service.GetCitiesAsync(page, pageSize, search, country)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var city = await _service.GetCityByIdAsync(id);
        return city == null
            ? NotFound(AdminApiResponse<AdminCityResponse>.Fail("City not found."))
            : Ok(AdminApiResponse<AdminCityResponse>.Ok(city));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminCityRequest request)
    {
        var result = await _service.CreateCityAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.CityId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminCityRequest request)
    {
        var result = await _service.UpdateCityAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteCityAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries()
        => Ok(AdminApiResponse<IEnumerable<string>>.Ok(await _service.GetCountriesAsync()));

    [HttpPost("{id}/images")]
    public async Task<IActionResult> AddImage(int id, [FromBody] AdminImageRequest request)
    {
        var result = await _service.AddCityImageAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}/images/{imageId}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var result = await _service.DeleteCityImageAsync(id, imageId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/images/{imageId}/set-primary")]
    public async Task<IActionResult> SetPrimary(int id, int imageId)
    {
        var result = await _service.SetPrimaryCityImageAsync(id, imageId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

// ================================================================
// HOTELS
// ================================================================
[Route("api/admin/hotels")]
public class AdminHotelsController : AdminBaseController
{
    private readonly IAdminHotelService _service;
    public AdminHotelsController(IAdminHotelService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] int? cityId = null)
        => Ok(AdminApiResponse<AdminPagedResult<AdminHotelResponse>>.Ok(
            await _service.GetHotelsAsync(page, pageSize, search, cityId)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var hotel = await _service.GetHotelByIdAsync(id);
        return hotel == null
            ? NotFound(AdminApiResponse<AdminHotelResponse>.Fail("Hotel not found."))
            : Ok(AdminApiResponse<AdminHotelResponse>.Ok(hotel));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminHotelRequest request)
    {
        var result = await _service.CreateHotelAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.HotelId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminHotelRequest request)
    {
        var result = await _service.UpdateHotelAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteHotelAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/images")]
    public async Task<IActionResult> AddImage(int id, [FromBody] AdminImageRequest request)
    {
        var result = await _service.AddHotelImageAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}/images/{imageId}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var result = await _service.DeleteHotelImageAsync(id, imageId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/images/{imageId}/set-primary")]
    public async Task<IActionResult> SetPrimary(int id, int imageId)
    {
        var result = await _service.SetPrimaryHotelImageAsync(id, imageId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id}/images/reorder")]
    public async Task<IActionResult> ReorderImages(int id, [FromBody] List<int> imageIds)
        => Ok(await _service.ReorderHotelImagesAsync(id, imageIds));

    /// <summary>
    /// Upload nhiều ảnh cho hotel (không set isPrimary, chỉ cần id + imageUrl)
    /// </summary>
    [HttpPost("{hotelId}/images/bulk")]
    public async Task<IActionResult> AddHotelImagesBulk(int hotelId, [FromBody] AdminImageBulkRequest request)
    {
        var result = await _service.AddHotelImagesBulkAsync(hotelId, request);
        return result.Success
            ? Ok(result)
            : result.Message == "Hotel not found."
                ? NotFound(result)
                : BadRequest(result);
    }
}

// ================================================================
// ROOMTYPES
// ================================================================
[Route("api/admin/room-types")]
public class AdminRoomTypesController : ControllerBase
{
    private readonly IAdminRoomTypeService _service;

    public AdminRoomTypesController(IAdminRoomTypeService service) => _service = service;

    // -----------------------------------------------------------------------
    // GET /api/admin/room-types
    //   ?page=1&pageSize=10&search=deluxe&hotelId=5
    // -----------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? hotelId = null)
        => Ok(AdminApiResponse<AdminPagedResult<AdminRoomTypeResponse>>.Ok(
            await _service.GetRoomTypesAsync(page, pageSize, search, hotelId)));

    // -----------------------------------------------------------------------
    // GET /api/admin/room-types/by-hotel/{hotelId}
    //   Lấy toàn bộ room types của một hotel (dùng khi tạo Room, cần dropdown)
    // -----------------------------------------------------------------------
    [HttpGet("by-hotel/{hotelId:int}")]
    public async Task<IActionResult> GetByHotel(int hotelId)
        => Ok(AdminApiResponse<IEnumerable<AdminRoomTypeResponse>>.Ok(
            await _service.GetByHotelAsync(hotelId)));

    // -----------------------------------------------------------------------
    // GET /api/admin/room-types/{id}
    // -----------------------------------------------------------------------
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var rt = await _service.GetByIdAsync(id);
        return rt == null
            ? NotFound(AdminApiResponse<AdminRoomTypeResponse>.Fail("RoomType not found."))
            : Ok(AdminApiResponse<AdminRoomTypeResponse>.Ok(rt));
    }

    // -----------------------------------------------------------------------
    // POST /api/admin/room-types
    //   Body: { hotelId, typeName, description, pricePerNight, capacity,
    //           imgUrl, facilityIds: [1,2,3] }
    // -----------------------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminRoomTypeRequest request)
    {
        var result = await _service.CreateAsync(request);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.RoomTypeId }, result);
    }

    // -----------------------------------------------------------------------
    // PUT /api/admin/room-types/{id}
    //   Cập nhật thông tin + đồng bộ lại danh sách facilities (replace)
    // -----------------------------------------------------------------------
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminRoomTypeRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // -----------------------------------------------------------------------
    // DELETE /api/admin/room-types/{id}
    //   Chỉ xóa được khi không còn Room instance nào thuộc type này
    // -----------------------------------------------------------------------
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
// ================================================================
// ROOMS
// ================================================================
[Route("api/admin/rooms")]
public class AdminRoomsController : ControllerBase
{
    private readonly IAdminRoomService _service;

    public AdminRoomsController(IAdminRoomService service) => _service = service;

    // -----------------------------------------------------------------------
    // GET /api/admin/rooms
    //   ?page=1&pageSize=10&search=101&hotelId=3&roomType=Deluxe
    // -----------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? hotelId = null,
        [FromQuery] string? roomType = null)
        => Ok(AdminApiResponse<AdminPagedResult<AdminRoomResponse>>.Ok(
            await _service.GetRoomsAsync(page, pageSize, search, hotelId, roomType)));

    // -----------------------------------------------------------------------
    // GET /api/admin/rooms/{id}
    //   Trả về Room kèm thông tin RoomType (price, capacity, imgUrl, facilities)
    // -----------------------------------------------------------------------
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _service.GetRoomByIdAsync(id);
        return room == null
            ? NotFound(AdminApiResponse<AdminRoomResponse>.Fail("Room not found."))
            : Ok(AdminApiResponse<AdminRoomResponse>.Ok(room));
    }

    // -----------------------------------------------------------------------
    // GET /api/admin/rooms/by-hotel/{hotelId}
    // -----------------------------------------------------------------------
    [HttpGet("by-hotel/{hotelId:int}")]
    public async Task<IActionResult> GetByHotel(int hotelId)
        => Ok(AdminApiResponse<IEnumerable<AdminRoomResponse>>.Ok(
            await _service.GetRoomsByHotelAsync(hotelId)));

    // -----------------------------------------------------------------------
    // POST /api/admin/rooms
    //   Tạo một Room đơn lẻ.
    //   Body: { hotelId, roomTypeId, roomNumber, status? }
    //   Lưu ý: price/capacity/imgUrl/facilities đều lấy từ RoomType, không truyền lại.
    // -----------------------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminRoomRequest request)
    {
        var result = await _service.CreateRoomAsync(request);
        if (!result.Success) return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.RoomId }, result);
    }

    // -----------------------------------------------------------------------
    // POST /api/admin/rooms/bulk
    //   Tạo nhiều Room cùng RoomType trong một lần.
    //   Body: { hotelId, roomTypeId, roomNumbers: ["101","102","103"] }
    // -----------------------------------------------------------------------
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreate([FromBody] AdminBulkCreateRoomRequest request)
    {
        var result = await _service.BulkCreateRoomsAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // -----------------------------------------------------------------------
    // PUT /api/admin/rooms/{id}
    //   Cập nhật Room: có thể đổi RoomType, RoomNumber, Status.
    //   Body: { hotelId, roomTypeId, roomNumber, status? }
    // -----------------------------------------------------------------------
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminRoomRequest request)
    {
        var result = await _service.UpdateRoomAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // -----------------------------------------------------------------------
    // PATCH /api/admin/rooms/{id}/status
    //   Chỉ cập nhật trạng thái: Available | Occupied | Maintenance
    //   Body: { "status": "Maintenance" }
    // -----------------------------------------------------------------------
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AdminUpdateRoomStatusRequest request)
    {
        var result = await _service.UpdateRoomStatusAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // -----------------------------------------------------------------------
    // DELETE /api/admin/rooms/{id}
    //   Không cho xóa nếu còn reservation Pending/Confirmed.
    // -----------------------------------------------------------------------
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteRoomAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ================================================================
// FACILITIES
// ================================================================
[Route("api/admin/facilities")]
public class AdminFacilitiesController : AdminBaseController
{
    private readonly IAdminFacilityService _service;
    public AdminFacilitiesController(IAdminFacilityService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
        => Ok(AdminApiResponse<AdminPagedResult<AdminFacilityResponse>>.Ok(
            await _service.GetFacilitiesAsync(page, pageSize, search)));

    [HttpGet("all")]
    public async Task<IActionResult> GetAllList()
        => Ok(AdminApiResponse<IEnumerable<AdminFacilityResponse>>.Ok(await _service.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var f = await _service.GetByIdAsync(id);
        return f == null
            ? NotFound(AdminApiResponse<AdminFacilityResponse>.Fail("Facility not found."))
            : Ok(AdminApiResponse<AdminFacilityResponse>.Ok(f));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminFacilityRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.FacilityId }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminFacilityRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

// ================================================================
// RESERVATIONS
// ================================================================
[Route("api/admin/reservations")]
public class AdminReservationsController : AdminBaseController
{
    private readonly IAdminReservationService _service;
    private readonly INotificationService _notificationService;
    public AdminReservationsController(IAdminReservationService service, INotificationService notificationService)
    {
        _service = service;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? status = null,
        [FromQuery] int? hotelId = null, [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
        => Ok(AdminApiResponse<AdminPagedResult<AdminReservationResponse>>.Ok(
            await _service.GetReservationsAsync(page, pageSize, search, status, hotelId, fromDate, toDate)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var r = await _service.GetReservationByIdAsync(id);
        return r == null
            ? NotFound(AdminApiResponse<AdminReservationResponse>.Fail("Reservation not found."))
            : Ok(AdminApiResponse<AdminReservationResponse>.Ok(r));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AdminUpdateReservationStatusRequest request)
    {
        var result = await _service.UpdateStatusAsync(id, request.PaymentStatus);

        if (result.Success)
        {
            // Lấy reservation để gửi notification
            var reservation = await _service.GetReservationByIdAsync(id);

            if (reservation != null)
            {
                // Map sang Reservation model để truyền vào NotificationService
                var reservationModel = new Reservation
                {
                    ReservationId = reservation.ReservationId,
                    UserId = reservation.UserId,
                    RoomId = reservation.RoomId,
                    CheckInDate = DateOnly.FromDateTime(reservation.CheckInDate),
                    CheckOutDate = DateOnly.FromDateTime(reservation.CheckOutDate),
                    TotalPrice = reservation.TotalPrice,
                    PaymentStatus = request.PaymentStatus
                };

                try
                {
                    switch (request.PaymentStatus)
                    {
                        case "Confirmed":
                            await _notificationService.NotifyBookingConfirmed(reservationModel);
                            break;
                        case "Cancelled":
                            // false = admin huỷ
                            await _notificationService.NotifyBookingCancelled(reservationModel, cancelledByUser: false);
                            break;
                        case "Completed":
                            await _notificationService.NotifyBookingCompleted(reservationModel);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Không return lỗi, chỉ log — notification thất bại không ảnh hưởng business
                    Console.WriteLine($"Notification failed: {ex.Message}");
                }
            }

            return Ok(result);
        }

        return BadRequest(result);
    }
}

// ================================================================
// FEEDBACKS
// ================================================================
[Route("api/admin/feedbacks")]
public class AdminFeedbacksController : AdminBaseController
{
    private readonly IAdminFeedbackService _service;
    public AdminFeedbacksController(IAdminFeedbackService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] int? hotelId = null,
        [FromQuery] int? rating = null)
        => Ok(AdminApiResponse<AdminPagedResult<AdminFeedbackResponse>>.Ok(
            await _service.GetFeedbacksAsync(page, pageSize, search, hotelId, rating)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var fb = await _service.GetFeedbackByIdAsync(id);
        return fb == null
            ? NotFound(AdminApiResponse<AdminFeedbackResponse>.Fail("Feedback not found."))
            : Ok(AdminApiResponse<AdminFeedbackResponse>.Ok(fb));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateFeedbackRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
