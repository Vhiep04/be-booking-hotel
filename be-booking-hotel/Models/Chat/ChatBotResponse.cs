using System.Text.Json.Serialization;

public class ChatBotResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("recommendedRoomIds")]
    public List<int> RecommendedRoomIds { get; set; } = new();

    [JsonPropertyName("hasRecommendation")]
    public bool HasRecommendation { get; set; }

    [JsonPropertyName("followUpQuestion")]
    public string? FollowUpQuestion { get; set; }
}