namespace be_booking_hotel.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string? UserId { get; set; }
        public bool IsAdminNotification { get; set; } = false;
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public int? ReservationId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ApplicationUser? User { get; set; }
        public virtual Reservation? Reservation { get; set; }
    }

    public enum NotificationType
    {
        NewUser,
        NewBooking,
        BookingConfirmed,
        BookingCancelled,
        BookingCompleted,
        BookingRejected,
        PaymentSuccess,
        PaymentFailed,
        Reminder
    }
}
