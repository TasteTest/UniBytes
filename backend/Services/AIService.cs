using System.Text;
using System.Text.Json;
using backend.Common;
using backend.DTOs.AI.OpenRouter;
using backend.DTOs.AI.Request;
using backend.DTOs.AI.Response;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services;

/// <summary>
/// AI service implementation for personalized menu recommendations using OpenRouter.
/// </summary>
public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIService> _logger;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly string _apiKey;
    private const string OpenRouterApiUrl = "https://openrouter.ai/api/v1/chat/completions";
    private const string Model = "openai/gpt-oss-20b:free";

    public AIService(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration, 
        ILogger<AIService> logger,
        IMenuItemRepository menuItemRepository)
    {
        _httpClient = httpClientFactory.CreateClient("OpenRouter");
        _logger = logger;
        _menuItemRepository = menuItemRepository;
        _apiKey = configuration["OpenRouter:ApiKey"] 
                  ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") 
                  ?? throw new InvalidOperationException("OpenRouter API key is not configured");
    }

    public async Task<Result<AIResponse>> GetMenuRecommendationsAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Fetch all menu items
            var menuItems = await _menuItemRepository.GetAllAsync(cancellationToken);
            
            // Prompt engineering: build prompt from user preferences and menu data
            var enhancedPrompt = BuildEnhancedPrompt(request, menuItems);

            // Build OpenRouter request
            var openRouterRequest = new OpenRouterRequest
            {
                Model = Model,
                Messages = new List<OpenRouterMessage>
                {
                    new()
                    {
                        Role = "user",
                        Content = enhancedPrompt
                    }
                },
                Reasoning = new OpenRouterReasoning { Enabled = false }
            };

            // Prepare HTTP request
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, OpenRouterApiUrl)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(openRouterRequest),
                    Encoding.UTF8,
                    "application/json")
            };

            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

            // Call OpenRouter API
            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenRouter API error: {StatusCode} - {Error}", 
                    httpResponse.StatusCode, errorContent);
                return Result<AIResponse>.Failure($"AI service error: {httpResponse.StatusCode}");
            }

            var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var openRouterResponse = JsonSerializer.Deserialize<OpenRouterResponse>(responseContent);

            if (openRouterResponse?.Choices == null || openRouterResponse.Choices.Count == 0)
            {
                _logger.LogError("OpenRouter returned empty response");
                return Result<AIResponse>.Failure("AI service returned no response");
            }

            var message = openRouterResponse.Choices[0].Message;
            
            // Parse response to extract product IDs
            var responseText = message.Content;
            var productIds = new List<Guid>();
            
            // Look for PRODUCT_IDS: line in the response
            var lines = responseText.Split('\n');
            var productIdLine = lines.FirstOrDefault(l => l.TrimStart().StartsWith("PRODUCT_IDS:", StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrEmpty(productIdLine))
            {
                // Extract the IDs part after "PRODUCT_IDS:"
                var idsText = productIdLine.Substring(productIdLine.IndexOf(':') + 1).Trim();
                var idStrings = idsText.Split(',', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var idString in idStrings)
                {
                    if (Guid.TryParse(idString.Trim(), out var guid))
                    {
                        productIds.Add(guid);
                    }
                }
                
                // Remove the PRODUCT_IDS line from the response text
                responseText = string.Join('\n', lines.Where(l => !l.TrimStart().StartsWith("PRODUCT_IDS:", StringComparison.OrdinalIgnoreCase)));
            }
            
            // Fetch the recommended products
            var recommendedProducts = new List<DTOs.Menu.Response.MenuItemResponseDto>();
            if (productIds.Any())
            {
                foreach (var productId in productIds)
                {
                    var product = menuItems.FirstOrDefault(m => m.Id == productId);
                    if (product != null)
                    {
                        recommendedProducts.Add(new DTOs.Menu.Response.MenuItemResponseDto
                        {
                            Id = product.Id,
                            CategoryId = product.CategoryId,
                            Name = product.Name,
                            Description = product.Description,
                            Price = product.Price,
                            Currency = product.Currency,
                            Available = product.Available,
                            Visibility = product.Visibility,
                            Components = product.Components,
                            ImageUrl = product.ImageUrl,
                            CreatedAt = product.CreatedAt,
                            UpdatedAt = product.UpdatedAt
                        });
                    }
                }
            }
            
            var aiResponse = new AIResponse
            {
                Response = responseText.Trim(),
                RecommendedProducts = recommendedProducts
            };

            return Result<AIResponse>.Success(aiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenRouter API");
            return Result<AIResponse>.Failure($"Error communicating with AI service: {ex.Message}");
        }
    }

    /// <summary>
    /// Build comprehensive prompt for menu recommendations based on user preferences.
    /// </summary>
    private string BuildEnhancedPrompt(AIRequest request, IEnumerable<Models.MenuItem> menuItems)
    {
        var prompt = new StringBuilder();

        // System context
        prompt.AppendLine("# SYSTEM CONTEXT");
        prompt.AppendLine("You are a nutrition-focused AI assistant for CampusEats, a university cafeteria ordering application.");
        prompt.AppendLine("Your role is to create personalized menu recommendations based on student dietary preferences and goals.");
        prompt.AppendLine();

        // Available menu items
        prompt.AppendLine("# AVAILABLE MENU ITEMS");
        prompt.AppendLine("Below is the complete menu with all available items:");
        prompt.AppendLine();

        var groupedItems = menuItems
            .Where(m => m.Available)
            .GroupBy(m => m.Category.Name)
            .OrderBy(g => g.Key);

        foreach (var category in groupedItems)
        {
            prompt.AppendLine($"## {category.Key}");
            foreach (var item in category.OrderBy(i => i.Name))
            {
                prompt.AppendLine($"- {item.Name} (ID: {item.Id}) - {item.Price:F2} {item.Currency.ToUpper()}");
                if (!string.IsNullOrEmpty(item.Description))
                {
                    prompt.AppendLine($"  Description: {item.Description}");
                }
            }
            prompt.AppendLine();
        }

        // User preferences
        prompt.AppendLine("# USER PREFERENCES");
        prompt.AppendLine($"**Dietary Goal**: {request.DietaryGoal}");
        
        if (request.DietaryRestrictions.Any())
        {
            prompt.AppendLine($"**Dietary Restrictions**: {string.Join(", ", request.DietaryRestrictions)}");
        }
        
        if (!string.IsNullOrWhiteSpace(request.Allergies))
        {
            prompt.AppendLine($"**Allergies**: {request.Allergies}");
        }
        
        if (!string.IsNullOrWhiteSpace(request.Dislikes))
        {
            prompt.AppendLine($"**Dislikes**: {request.Dislikes}");
        }
        
        prompt.AppendLine($"**Preferred Meal Type**: {request.PreferredMealType}");
        prompt.AppendLine($"**Calorie Preference**: {request.CaloriePreference}");
        prompt.AppendLine($"**Menu Type Requested**: {request.MenuType}");
        prompt.AppendLine();

        // Task instructions
        prompt.AppendLine("# YOUR TASK");
        prompt.AppendLine($"Create a personalized {request.MenuType} recommendation from the available menu items above.");
        prompt.AppendLine("Consider ALL user preferences when making recommendations:");
        prompt.AppendLine("- Align recommendations with the dietary goal");
        prompt.AppendLine("- Respect all dietary restrictions and allergies");
        prompt.AppendLine("- Avoid foods the user dislikes");
        prompt.AppendLine("- Match the preferred meal type when possible");
        prompt.AppendLine("- Consider the calorie preference");
        prompt.AppendLine("- Provide the appropriate number of items based on menu type:");
        prompt.AppendLine("  * Single Item: 1 item");
        prompt.AppendLine("  * Light Snack: 2 items");
        prompt.AppendLine("  * Light Meal: 3-4 items");
        prompt.AppendLine("  * Full Meal: 5-6 items");
        prompt.AppendLine("  * Complete Menu: 7+ items");
        prompt.AppendLine();

        // Critical rules
        prompt.AppendLine("# CRITICAL RULES");
        prompt.AppendLine("1. **ONLY recommend items from the menu above** - Never invent items");
        prompt.AppendLine("2. **Show exact prices** in RON format (e.g., '15.50 RON')");
        prompt.AppendLine("3. **Respect ALL dietary restrictions and allergies** - This is non-negotiable");
        prompt.AppendLine("4. **Avoid disliked foods** whenever possible");
        prompt.AppendLine("5. **Provide brief reasoning** for each recommendation (1-2 sentences)");
        prompt.AppendLine("6. **No markdown formatting** - Use plain text only");
        prompt.AppendLine("7. **Format recommendations as**:");
        prompt.AppendLine("   - Item Name (Price RON) - Why this fits your goals");
        prompt.AppendLine("8. **If constraints are too restrictive** and you can't find enough suitable items, explain the issue and suggest the best available options");
        prompt.AppendLine();
        
        prompt.AppendLine("# OUTPUT FORMAT");
        prompt.AppendLine("After your recommendation text, you MUST include a line starting with 'PRODUCT_IDS:' followed by the comma-separated GUIDs of the products you recommended.");
        prompt.AppendLine("Example:");
        prompt.AppendLine("PRODUCT_IDS: 12345678-1234-1234-1234-123456789abc, 87654321-4321-4321-4321-cba987654321");
        prompt.AppendLine();

        prompt.AppendLine("Generate the personalized menu recommendation now, following ALL rules and preferences above.");

        return prompt.ToString();
    }
}
