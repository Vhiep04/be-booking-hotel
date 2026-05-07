
using be_booking_hotel.Models;
namespace be_booking_hotel.Repositories.Interfaces

{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetByUserIdAsync(string userId);
        Task<List<Notification>> GetAdminNotificationsAsync();
        Task<int> GetUnreadCountAsync(string userId);
        Task<int> GetAdminUnreadCountAsync();
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task MarkAllAdminAsReadAsync();
    }
}
