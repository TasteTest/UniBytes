using System.Text.Json.Serialization;

namespace backend.DTOs.AI.OpenRouter;

/// <summary>
/// Request model for OpenRouter API.
/// </summary>
public class OpenRouterRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "openai/gpt-oss-20b:free";

    [JsonPropertyName("messages")]
    public List<OpenRouterMessage> Messages { get; set; } = new();

    [JsonPropertyName("reasoning")]
    public OpenRouterReasoning? Reasoning { get; set; }
}

/// <summary>
/// Message in OpenRouter request.
/// </summary>
public class OpenRouterMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Reasoning configuration for OpenRouter.
/// </summary>
public class OpenRouterReasoning
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}
