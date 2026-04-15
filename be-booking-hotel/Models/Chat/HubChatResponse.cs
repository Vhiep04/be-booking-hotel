using be_booking_hotel.Models.Chat;

public class HubChatResponse
{
    public string Message { get; set; } = "";
    public List<RoomLink> RecommendedRooms { get; set; } = new();
    public bool HasRecommendation { get; set; }
    public string? FollowUpQuestion { get; set; }
}