using be_booking_hotel.Models.Chat;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace be_booking_hotel.Services.Implements
{
    // Services/ChatBotService.cs
    public class ChatBotService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RoomContextService _roomContextService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;

        // Lưu conversation history theo sessionId
        private static readonly ConcurrentDictionary<string, List<OllamaMessage>> _sessions = new();

        public ChatBotService(
            IHttpClientFactory httpClientFactory,
            RoomContextService roomContextService,
            IMemoryCache cache,
            IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _roomContextService = roomContextService;
            _cache = cache;
            _config = config;
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(
            string sessionId,
            string userMessage,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            // 1. Lấy hoặc khởi tạo conversation history
            var history = _sessions.GetOrAdd(sessionId, _ => new List<OllamaMessage>());

            // 2. Thêm tin nhắn user vào history
            history.Add(new OllamaMessage { Role = "user", Content = userMessage });

            // 3. Build system prompt với room context
            var roomContext = await _roomContextService.BuildRoomContextAsync();
            var systemPrompt = BuildSystemPrompt(roomContext);

            // 4. Build message list: system + history (giới hạn 10 tin gần nhất)
            var messages = new List<OllamaMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };
            messages.AddRange(history.TakeLast(10));

            // 5. Gọi Ollama API với streaming
            var client = _httpClientFactory.CreateClient("Ollama");
            var request = new OllamaRequest
            {
                Model = _config["Ollama:Model"] ?? "qwen2.5:7b",
                Messages = messages,
                Stream = true,
                Options = new OllamaOptions { Temperature = 0.7f }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync("/api/chat", content, ct);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            var fullResponse = new StringBuilder();

            // 6. Đọc từng dòng JSON stream từ Ollama
            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (string.IsNullOrWhiteSpace(line)) continue;

                var chunk = JsonSerializer.Deserialize<OllamaStreamResponse>(line);
                if (chunk?.Message?.Content is { Length: > 0 } token)
                {
                    fullResponse.Append(token);
                    yield return token; // Stream từng token về SignalR
                }

                if (chunk?.Done == true) break;
            }

            // 7. Lưu response của assistant vào history
            history.Add(new OllamaMessage
            {
                Role = "assistant",
                Content = fullResponse.ToString()
            });

            // Giới hạn history tối đa 20 messages để tránh context quá dài
            if (history.Count > 20)
                history.RemoveRange(0, history.Count - 20);
        }

        private string BuildSystemPrompt(string roomContext)
        {
            return $"""
            Bạn là trợ lý tư vấn đặt phòng của khách sạn Grand Hotel.
            Nhiệm vụ của bạn là giúp khách hàng chọn phòng phù hợp nhất với nhu cầu.

            {roomContext}

            === QUY TẮC TƯ VẤN ===
            1. Luôn hỏi về nhu cầu: số khách, ngày nhận/trả phòng, ngân sách nếu chưa rõ.
            2. Đề xuất 2-3 phòng phù hợp nhất, giải thích tại sao phù hợp.
            3. Nêu rõ giá, tiện nghi nổi bật, vị trí tầng.
            4. Nếu hỏi về thông tin không có trong danh sách phòng, hãy hướng dẫn liên hệ lễ tân.
            5. Trả lời thân thiện, ngắn gọn bằng tiếng Việt.
            6. Không bịa đặt thông tin về phòng ngoài danh sách được cung cấp.
            """;
        }

        public void ClearSession(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
        }
    }
}
