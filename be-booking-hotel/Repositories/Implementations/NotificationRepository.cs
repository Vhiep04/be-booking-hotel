using Microsoft.EntityFrameworkCore;
using be_booking_hotel.Models;
using be_booking_hotel.Repositories.Interfaces;

namespace be_booking_hotel.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly HotelBookingContext _context;

        public NotificationRepository(HotelBookingContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetByUserIdAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsAdminNotification)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAdminNotificationsAsync()
        {
            return await _context.Notifications
                .Where(n => n.IsAdminNotification)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<int> GetAdminUnreadCountAsync()
        {
            return await _context.Notifications
                .CountAsync(n => n.IsAdminNotification && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var noti = await _context.Notifications.FindAsync(notificationId);
            if (noti != null)
            {
                noti.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task MarkAllAdminAsReadAsync()
        {
            await _context.Notifications
                .Where(n => n.IsAdminNotification && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }
    }
}
