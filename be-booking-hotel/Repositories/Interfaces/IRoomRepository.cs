using be_booking_hotel.Models;

public interface IRoomRepository
{
    Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateOnly? checkIn = null, DateOnly? checkOut = null);
}