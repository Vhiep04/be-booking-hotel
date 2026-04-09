namespace be_booking_hotel.DTOs.Admin;

public class AdminPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;
}

public class AdminApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static AdminApiResponse<T> Ok(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static AdminApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

// ===== USER =====
public class AdminUserResponse
{
    public string Id { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class AdminCreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new() { "User" };
}

public class AdminUpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string>? Roles { get; set; }
}

public class AdminChangePasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

// ===== IMAGE (shared cho Hotel và City) =====
public class AdminImageResponse
{
    public int ImageId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
}

public class AdminImageRequest
{
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public string? Description { get; set; }
}

// ===== CITY =====
public class AdminCityResponse
{
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int HotelCount { get; set; }
    public List<AdminImageResponse> Images { get; set; } = new();
}

public class AdminCityRequest
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

// ===== HOTEL =====
public class AdminHotelResponse
{
    public int HotelId { get; set; }
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImgUrl { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RoomCount { get; set; }
    public double? AverageRating { get; set; }
    public int FeedbackCount { get; set; }
    public List<AdminImageResponse> Images { get; set; } = new();
}

public class AdminHotelRequest
{
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImgUrl { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

// ===== ROOM =====
public class AdminRoomResponse
{
    public int RoomId { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public int Capacity { get; set; }
    public string? ImgUrl { get; set; }
    public List<AdminFacilityResponse> Facilities { get; set; } = new();
}

public class AdminRoomRequest
{
    public int HotelId { get; set; }
    public int RoomTypeId { get; set; }    // thay RoomType string
    public string RoomNumber { get; set; } = null!;
    public string? Status { get; set; }
    // Bỏ: PricePerNight, Capacity, ImgUrl, FacilityIds (quản lý ở RoomType)
}

// ===== FACILITY =====
public class AdminFacilityResponse
{
    public int FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AdminFacilityRequest
{
    public string Name { get; set; } = string.Empty;
}

// ===== RESERVATION =====
public class AdminReservationResponse
{
    public int ReservationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public int RoomId { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int Nights { get; set; }
    public decimal TotalPrice { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<AdminPaymentResponse> Payments { get; set; } = new();
}

public class AdminUpdateReservationStatusRequest
{
    public string PaymentStatus { get; set; } = string.Empty;
}

// ===== PAYMENT =====
public class AdminPaymentResponse
{
    public int PaymentId { get; set; }
    public int ReservationId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

// ===== FEEDBACK =====
public class AdminFeedbackResponse
{
    public int FeedbackId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public int HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public int? ReservationId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminUpdateFeedbackRequest
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

// ===== DASHBOARD =====
public class AdminRecentBookingDto
{
    public int ReservationId { get; set; }
    public string BookingCode { get; set; }  // "BK-2024-001"
    public string HotelName { get; set; }
    public string HotelImage { get; set; }
    public string CityName { get; set; }
    public string GuestName { get; set; }
    public string GuestAvatar { get; set; } // chữ cái đầu
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
}

public class AdminPopularCityDto
{
    public int CityId { get; set; }
    public string Name { get; set; }
    public int HotelCount { get; set; }
    public int BookingCount { get; set; }
    public int Percentage { get; set; }  // tính từ BookingCount / Total * 100
}
public class AdminActivityDto
{
    public string Message { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string Type { get; set; } = string.Empty; // "booking" | "payment"
}
public class AdminDashboardStats
{
    public int TotalUsers { get; set; }
    public int TotalHotels { get; set; }
    public int TotalCities { get; set; }
    public int TotalRooms { get; set; }
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public double AverageRating { get; set; }
    public List<AdminRevenueByMonthDto> RevenueChart { get; set; } = new();
    public List<AdminTopHotelDto> TopHotels { get; set; } = new();
    public int CompletedReservations { get; set; }   // donut chart
    public int CancelledReservations { get; set; }   // donut chart
    public List<AdminRecentBookingDto> RecentBookings { get; set; } = new(); // bảng cuối
    public List<AdminPopularCityDto> PopularCities { get; set; } = new();   // progress bar
    public List<AdminActivityDto> RecentActivities { get; set; } = new();
}

public class AdminRevenueByMonthDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int ReservationCount { get; set; }
}

public class AdminTopHotelDto
{
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string? ImgUrl { get; set; }
    public int ReservationCount { get; set; }
    public decimal Revenue { get; set; }
    public double AverageRating { get; set; }
}

public class CloudinaryUploadResult
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? PublicId { get; set; }
    public string? Error { get; set; }
}
public class UploadImageResponse
{
    /// <summary>URL Cloudinary - dùng để lưu vào DB (imageUrl field)</summary>
    public string Url { get; set; } = "";

    /// <summary>Public ID - dùng để xóa ảnh sau này</summary>
    public string PublicId { get; set; } = "";
}

public class UploadImagesResponse
{
    public List<UploadImageResponse> Uploaded { get; set; } = new();
    public int FailedCount { get; set; }
}
