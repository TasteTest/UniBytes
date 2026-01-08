using System.Text.Json.Serialization;

namespace backend.DTOs.AI.OpenRouter;

/// <summary>
/// Response model from OpenRouter API.
/// </summary>
public class OpenRouterResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<OpenRouterChoice> Choices { get; set; } = new();

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
}

/// <summary>
/// Choice in OpenRouter response.
/// </summary>
public class OpenRouterChoice
{
    [JsonPropertyName("message")]
    public OpenRouterResponseMessage Message { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
}

/// <summary>
/// Message in OpenRouter response.
/// </summary>
public class OpenRouterResponseMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("reasoning")]
    public string? Reasoning { get; set; }
}
