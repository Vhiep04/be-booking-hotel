using System.Text.Json.Serialization;

namespace be_booking_hotel.Models.Chat
{
    public class ChatMessage
    {
        public string Role { get; set; } = "user"; // "user" | "assistant" | "system"
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ChatRequest
    {
        public string Message { get; set; } = "";
        public string? SessionId { get; set; }
    }

    // Models/Chat/OllamaRequest.cs  
    public class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "qwen2.5:7b";

        [JsonPropertyName("messages")]
        public List<OllamaMessage> Messages { get; set; } = new();

        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = true;

        [JsonPropertyName("options")]
        public OllamaOptions Options { get; set; } = new();
    }

    public class OllamaMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }

    public class OllamaOptions
    {
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = 0.7f;

        [JsonPropertyName("num_predict")]
        public int NumPredict { get; set; } = 512;
    }

    public class OllamaStreamResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}
