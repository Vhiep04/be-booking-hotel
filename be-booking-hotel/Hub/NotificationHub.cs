using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var user = Context.User;
            _logger.LogInformation("[Hub] IsAuthenticated: {auth}", user?.Identity?.IsAuthenticated);

            if (user?.Identity?.IsAuthenticated != true)
            {
                Context.Abort();
                return;
            }

            var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("[Hub] UserId: {userId}", userId);

            var isAdmin = user.IsInRole("Admin");
            _logger.LogInformation("[Hub] IsAdmin: {isAdmin}", isAdmin);

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            _logger.LogInformation("[Hub] Added to group user-{userId}", userId);

            if (isAdmin)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                _logger.LogInformation("[Hub] Added to group Admins");
            }

            await base.OnConnectedAsync();
            _logger.LogInformation("[Hub] OnConnectedAsync completed OK");
        }
        catch (Exception ex)
        {
            // Log chính xác lỗi gì
            _logger.LogError(ex, "[Hub] OnConnectedAsync EXCEPTION: {msg}", ex.Message);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

            if (Context.User!.IsInRole("Admin"))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
        }

        _logger.LogInformation("[Hub] Disconnected - Error: {err}", exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }
}