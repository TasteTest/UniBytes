namespace backend.DTOs.AI.Response;

/// <summary>
/// Response model for AI chat messages.
/// </summary>
public class AIResponse
{
    /// <summary>
    /// The AI's response message.
    /// </summary>
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Optional reasoning from the AI model.
    /// </summary>
    public string? Reasoning { get; set; }
}
