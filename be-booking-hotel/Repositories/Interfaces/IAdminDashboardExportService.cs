namespace be_booking_hotel.Repositories.Interfaces
{
    public interface IAdminDashboardExportService
    {
        Task<byte[]> ExportDashboardAsync();
    }
}
