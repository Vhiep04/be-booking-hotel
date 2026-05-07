using be_booking_hotel.Models;

namespace be_booking_hotel.Services.Interfaces
{
    public interface INotificationService
    {
        // User events
        Task NotifyNewUserRegistered(ApplicationUser user);

        // Booking events  
        Task NotifyNewBooking(Reservation reservation);
        Task NotifyBookingConfirmed(Reservation reservation);
        Task NotifyBookingRejected(Reservation reservation, string reason = "");
        Task NotifyBookingCancelled(Reservation reservation, bool cancelledByUser = true);
        Task NotifyBookingCompleted(Reservation reservation);

        // Payment events
        Task NotifyPaymentSuccess(Payment payment);
        Task NotifyPaymentFailed(Payment payment);
    }
}
