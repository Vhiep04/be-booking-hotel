namespace be_booking_hotel.DTOs.Payment
{
    public class CashReservationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string HotelAddress { get; set; } = string.Empty;
        public DateOnly CheckInDate { get; set; }
        public DateOnly CheckOutDate { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
    }
}