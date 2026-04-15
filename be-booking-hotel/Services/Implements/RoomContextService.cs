// Services/Chat/RoomContextService.cs
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using System.Text;

public class RoomContextService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IHotelRepository _hotelRepository;

    public RoomContextService(IRoomRepository roomRepository, IHotelRepository hotelRepository)
    {
        _roomRepository = roomRepository;
        _hotelRepository = hotelRepository;
    }

    public async Task<string> BuildRoomContextAsync(
        int? hotelId = null,
        DateTime? checkIn = null,
        DateTime? checkOut = null)
    {
        // Convert DateTime? → DateOnly? cho repository
        DateOnly? checkInDate = checkIn.HasValue ? DateOnly.FromDateTime(checkIn.Value) : null;
        DateOnly? checkOutDate = checkOut.HasValue ? DateOnly.FromDateTime(checkOut.Value) : null;

        // Lấy danh sách hotels
        IEnumerable<Hotel> hotels;
        if (hotelId.HasValue)
        {
            var hotel = await _hotelRepository.GetHotelByIdAsync(hotelId.Value);
            hotels = hotel != null ? new[] { hotel } : Array.Empty<Hotel>();
        }
        else
        {
            var (hotelList, _) = await _hotelRepository.GetHotelsAsync(new be_booking_hotel.DTOs.Hotel.HotelFilterDto());
            hotels = hotelList;
        }

        // Lấy tất cả available rooms (đã lọc theo ngày)
        var availableRooms = (await _roomRepository.GetAvailableRoomsAsync(checkInDate, checkOutDate))
            .ToList();

        var sb = new StringBuilder();

        foreach (var hotel in hotels)
        {
            // Rooms thuộc hotel này
            var hotelRooms = availableRooms
                .Where(r => r.HotelId == hotel.HotelId)
                .ToList();

            // Tính giá min/max từ RoomTypes
            var prices = hotel.RoomTypes.Select(rt => rt.PricePerNight).ToList();
            var minPrice = prices.Any() ? prices.Min() : 0;
            var maxPrice = prices.Any() ? prices.Max() : 0;

            sb.AppendLine($"""
                === KHÁCH SẠN: {hotel.Name} ===
                Địa điểm  : {hotel.Location}, {hotel.City.Name}, {hotel.City.Country}
                Mô tả     : {hotel.Description ?? "Không có"}
                Giá từ    : {minPrice:N0} - {maxPrice:N0} VND/đêm
                Phòng còn : {hotelRooms.Count}/{hotel.Rooms.Count}
                """);

            sb.AppendLine("--- DANH SÁCH PHÒNG ---");

            foreach (var room in hotelRooms)
            {
                // Facilities từ RoomType.Facilities
                var facilities = room.RoomType?.Facilities
                    .Select(f => f.Name)
                    ?? Enumerable.Empty<string>();

                sb.AppendLine($"""
                    • RoomId={room.RoomId} | Số phòng: {room.RoomNumber} | Loại: {room.RoomType?.TypeName} | Sức chứa: {room.RoomType?.Capacity} khách
                      Giá: {room.RoomType?.PricePerNight:N0} VND/đêm
                      Tiện nghi: {string.Join(", ", facilities)}
                      Trạng thái: Còn phòng
                    """);
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}