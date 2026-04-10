using be_booking_hotel.DTOs.Payment;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace be_booking_hotel.Repositories.Implements
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly HotelBookingContext _db;
        private readonly ILogger<PaymentRepository> _logger;

        public PaymentRepository(HotelBookingContext db, ILogger<PaymentRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> CreateReservationAsync(SendReceiptRequest request)
        {
            var existing = await _db.Reservations
                .Where(r =>
                    r.UserId == request.UserId &&
                    r.RoomId == request.RoomId &&
                    r.CheckInDate == request.CheckInDate &&
                    r.CheckOutDate == request.CheckOutDate)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                _logger.LogInformation("Reservation already exists: {Id}", existing.ReservationId);
                return existing.ReservationId;
            }

            var reservation = new Reservation
            {
                UserId = request.UserId,
                RoomId = request.RoomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                TotalPrice = request.Amount,
                PaymentStatus = "Confirmed",
                CreatedAt = DateTime.Now,
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Reservation created: {Id}", reservation.ReservationId);
            return reservation.ReservationId;
        }

        public async Task CreatePaymentAsync(int reservationId, SendReceiptRequest request)
        {
            var exists = await _db.Payments
                .AnyAsync(p =>
                    p.ReservationId == reservationId &&
                    p.Status == "Success");

            if (exists)
            {
                _logger.LogWarning("Payment already recorded for ReservationId: {Id}", reservationId);
                return;
            }

            var payment = new Payment
            {
                ReservationId = reservationId,
                PaymentMethod = request.PaymentMethod,
                Amount = request.Amount,
                PaymentDate = DateTime.Now,
                Status = "Success",
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Payment saved — ReservationId: {ResId}, Amount: {Amount}",
                reservationId, request.Amount
            );
        }
        public async Task UpdateReservationStatusAsync(int reservationId, string status)
        {
            var reservation = await _db.Reservations.FindAsync(reservationId);
            if (reservation != null)
            {
                reservation.PaymentStatus = status;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<int> CreateCashReservationAsync(CashReservationRequest request)
        {
            var existing = await _db.Reservations
                .Where(r =>
                    r.UserId == request.UserId &&
                    r.RoomId == request.RoomId &&
                    r.CheckInDate == request.CheckInDate &&
                    r.CheckOutDate == request.CheckOutDate)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                _logger.LogInformation("Cash reservation already exists: {Id}", existing.ReservationId);
                return existing.ReservationId;
            }

            var reservation = new Reservation
            {
                UserId = request.UserId,
                RoomId = request.RoomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                TotalPrice = request.Amount,
                PaymentStatus = "Pending",
                CreatedAt = DateTime.Now,
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Cash reservation created: {Id}", reservation.ReservationId);
            return reservation.ReservationId;
        }
    }
}