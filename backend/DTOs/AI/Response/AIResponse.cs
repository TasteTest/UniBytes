using backend.DTOs.Menu.Response;

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
    /// List of recommended menu items based on AI recommendations.
    /// </summary>
    public List<MenuItemResponseDto> RecommendedProducts { get; set; } = new();
}
