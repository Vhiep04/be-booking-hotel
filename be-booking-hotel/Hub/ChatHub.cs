using be_booking_hotel.Services.Implements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

//[Authorize]
public class ChatHub : Hub
{
    private readonly ChatBotService _chatBotService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ChatBotService chatBotService, ILogger<ChatHub> logger)
    {
        _chatBotService = chatBotService;
        _logger = logger;
    }

    // Client gọi method này để gửi tin nhắn
    public async Task SendMessage(string message)
    {
        var sessionId = Context.ConnectionId; // Dùng connectionId làm sessionId
        var ct = Context.ConnectionAborted;   // Tự cancel khi client disconnect

        try
        {
            // Báo client biết bot đang typing
            await Clients.Caller.SendAsync("BotTyping", true, ct);

            // Stream từng token về client
            await foreach (var token in _chatBotService.StreamResponseAsync(sessionId, message, ct))
            {
                await Clients.Caller.SendAsync("ReceiveToken", token, ct);
            }

            // Báo stream kết thúc
            await Clients.Caller.SendAsync("StreamComplete", ct);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Client {Id} disconnected mid-stream", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming chat for session {Id}", sessionId);
            await Clients.Caller.SendAsync("ChatError", "Có lỗi xảy ra, vui lòng thử lại.");
        }
        finally
        {
            await Clients.Caller.SendAsync("BotTyping", false);
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Cleanup session khi disconnect
        _chatBotService.ClearSession(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}

