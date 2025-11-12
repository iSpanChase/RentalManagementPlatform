using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace RentalManagementPlatformWebAPI.Services.Assistant
{
    public class QwenOptions
    {
        public string? ApiKey { get; set; }
        public string? BaseUrl { get; set; } = "/v1/chat/completions"; // allow absolute too
        public string? Model { get; set; }
    }

    public interface IQwenClient
    {
        Task<QwenChatResponse> CreateChatCompletionAsync(QwenChatRequest req, CancellationToken ct);
    }

    public class QwenClient : IQwenClient
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly QwenOptions _opt;

        public QwenClient(IHttpClientFactory httpFactory, IOptions<QwenOptions> opt)
        {
            _httpFactory = httpFactory;
            _opt = opt.Value;
        }

        public async Task<QwenChatResponse> CreateChatCompletionAsync(QwenChatRequest req, CancellationToken ct)
        {
            var client = _httpFactory.CreateClient();

            // Support absolute BaseUrl or relative path
            var url = _opt.BaseUrl;
            if (string.IsNullOrWhiteSpace(url)) url = "/v1/chat/completions";

            var json = JsonSerializer.Serialize(req, QwenJson.Options);
            using var httpReq = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrWhiteSpace(_opt.ApiKey))
            {
                httpReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ApiKey);
            }

            using var resp = await client.SendAsync(httpReq, ct);
            resp.EnsureSuccessStatusCode();
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var result = await JsonSerializer.DeserializeAsync<QwenChatResponse>(stream, QwenJson.Options, ct)
                         ?? new QwenChatResponse { Choices = new List<QwenChoice>() };
            return result;
        }
    }

    public static class QwenJson
    {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
    }

    public record QwenChatRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<QwenMessage> Messages { get; set; } = new();
        public List<object>? Tools { get; set; }
        public string? ToolChoice { get; set; } // "auto"
    }

    public record QwenMessage
    {
        public QwenMessage() {}
        public QwenMessage(string role, string content)
        {
            Role = role; Content = content;
        }

        public string Role { get; set; } = "user";
        public string? Content { get; set; }
        public List<QwenToolCall>? ToolCalls { get; set; }
        public string? ToolCallId { get; set; } // for tool messages

        public static QwenMessage Tool(string toolCallId, string content)
        {
            return new QwenMessage("tool", content) { ToolCallId = toolCallId };
        }
    }

    public record QwenToolCall
    {
        public string? Id { get; set; }
        public QwenFunction? Function { get; set; }
    }

    public record QwenFunction
    {
        public string? Name { get; set; }
        public string? Arguments { get; set; }
    }

    public record QwenChatResponse
    {
        public List<QwenChoice> Choices { get; set; } = new();
    }

    public record QwenChoice
    {
        public QwenMessage? Message { get; set; }
    }
}

