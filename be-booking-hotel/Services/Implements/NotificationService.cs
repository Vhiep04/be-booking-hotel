using be_booking_hotel.DTOs;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;
using be_booking_hotel.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace be_booking_hotel.Services.Implements
{
    // Services/NotificationService.cs
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(
            INotificationRepository repo,
            IHubContext<NotificationHub> hub)
        {
            _repo = repo;
            _hub = hub;
        }

        
        
        private async Task SendToUser(string userId, Notification noti)
        {
            await _repo.AddAsync(noti);

            var dto = new NotificationDto
            {
                NotificationId = noti.NotificationId,
                Type = noti.Type.ToString(),
                Title = noti.Title,
                Message = noti.Message,
                ReservationId = noti.ReservationId,
                IsRead = noti.IsRead,
                CreatedAt = noti.CreatedAt == default ? DateTime.UtcNow : noti.CreatedAt,
            };

            await _hub.Clients.Group($"user-{userId}")
                      .SendAsync("ReceiveNotification", dto);
        }

        private async Task SendToAdmin(Notification noti)
        {
            await _repo.AddAsync(noti);

            var dto = new NotificationDto
            {
                NotificationId = noti.NotificationId,
                Type = noti.Type.ToString(),
                Title = noti.Title,
                Message = noti.Message,
                ReservationId = noti.ReservationId,
                IsRead = noti.IsRead,
                CreatedAt = noti.CreatedAt == default ? DateTime.UtcNow : noti.CreatedAt,
            };

            await _hub.Clients.Group("Admins")
                      .SendAsync("ReceiveNotification", dto);
        }

        
        // USER EVENTS
        
        public async Task NotifyNewUserRegistered(ApplicationUser user)
        {
            await SendToAdmin(new Notification
            {
                IsAdminNotification = true,
                Type = NotificationType.NewUser,
                Title = "Người dùng mới đăng ký",
                Message = $"{user.FirstName} {user.LastName} ({user.Email}) vừa tạo tài khoản."
            });
        }

        
        // BOOKING EVENTS
        
        public async Task NotifyNewBooking(Reservation reservation)
        {
            // Admin
            await SendToAdmin(new Notification
            {
                IsAdminNotification = true,
                Type = NotificationType.NewBooking,
                Title = "Đặt phòng mới",
                Message = $"Có đặt phòng mới #{reservation.ReservationId} - Phòng {reservation.RoomId}.",
                ReservationId = reservation.ReservationId
            });

            // User
            await SendToUser(reservation.UserId, new Notification
            {
                UserId = reservation.UserId,
                Type = NotificationType.NewBooking,
                Title = "Đặt phòng thành công",
                Message = $"Booking #{reservation.ReservationId} đã được tạo. Vui lòng chờ xác nhận.",
                ReservationId = reservation.ReservationId
            });
        }

        public async Task NotifyBookingConfirmed(Reservation reservation)
        {
            await SendToUser(reservation.UserId, new Notification
            {
                UserId = reservation.UserId,
                Type = NotificationType.BookingConfirmed,
                Title = "Booking đã được xác nhận ✅",
                Message = $"Booking #{reservation.ReservationId} đã xác nhận. Check-in: {reservation.CheckInDate:dd/MM/yyyy}.",
                ReservationId = reservation.ReservationId
            });
        }

        public async Task NotifyBookingRejected(Reservation reservation, string reason = "")
        {
            var msg = $"Booking #{reservation.ReservationId} đã bị từ chối.";
            if (!string.IsNullOrEmpty(reason)) msg += $" Lý do: {reason}";

            await SendToUser(reservation.UserId, new Notification
            {
                UserId = reservation.UserId,
                Type = NotificationType.BookingRejected,
                Title = "Booking bị từ chối ❌",
                Message = msg,
                ReservationId = reservation.ReservationId
            });
        }

        public async Task NotifyBookingCancelled(Reservation reservation, bool cancelledByUser = true)
        {
            if (cancelledByUser)
            {
                await SendToAdmin(new Notification
                {
                    IsAdminNotification = true,
                    Type = NotificationType.BookingCancelled,
                    Title = "User huỷ đặt phòng",
                    Message = $"User {reservation.UserId} đã huỷ booking #{reservation.ReservationId}.",
                    ReservationId = reservation.ReservationId
                });
            }
            else
            {
                await SendToUser(reservation.UserId, new Notification
                {
                    UserId = reservation.UserId,
                    Type = NotificationType.BookingCancelled,
                    Title = "Booking đã bị huỷ",
                    Message = $"Booking #{reservation.ReservationId} đã bị huỷ bởi khách sạn.",
                    ReservationId = reservation.ReservationId
                });
            }
        }

        public async Task NotifyBookingCompleted(Reservation reservation)
        {
            await SendToUser(reservation.UserId, new Notification
            {
                UserId = reservation.UserId,
                Type = NotificationType.BookingCompleted,
                Title = "Cảm ơn bạn đã lưu trú! 🌟",
                Message = $"Booking #{reservation.ReservationId} đã hoàn thành. Hãy để lại đánh giá nhé!",
                ReservationId = reservation.ReservationId
            });
        }

        
        // PAYMENT EVENTS
        
        public async Task NotifyPaymentSuccess(Payment payment)
        {
            // Load reservation nếu chưa có
            var userId = payment.Reservation?.UserId;
            if (userId == null) return;

            await SendToAdmin(new Notification
            {
                IsAdminNotification = true,
                Type = NotificationType.PaymentSuccess,
                Title = "Thanh toán thành công",
                Message = $"Booking #{payment.ReservationId} thanh toán {payment.Amount:N0}đ thành công.",
                ReservationId = payment.ReservationId
            });

            await SendToUser(userId, new Notification
            {
                UserId = userId,
                Type = NotificationType.PaymentSuccess,
                Title = "Thanh toán thành công",
                Message = $"Thanh toán {payment.Amount:N0}đ cho booking #{payment.ReservationId} thành công.",
                ReservationId = payment.ReservationId
            });
        }

        public async Task NotifyPaymentFailed(Payment payment)
        {
            var userId = payment.Reservation.UserId;

            await SendToUser(userId, new Notification
            {
                UserId = userId,
                Type = NotificationType.PaymentFailed,
                Title = "Thanh toán thất bại",
                Message = $"Thanh toán cho booking #{payment.ReservationId} thất bại. Vui lòng thử lại.",
                ReservationId = payment.ReservationId
            });
        }
    }
}
