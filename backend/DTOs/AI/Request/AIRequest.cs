using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.AI.Request;

/// <summary>
/// Request model for AI menu recommendations based on user preferences.
/// </summary>
public class AIRequest
{
    /// <summary>
    /// User's dietary goal (e.g., Weight Loss, Muscle Building, etc.)
    /// </summary>
    [Required(ErrorMessage = "Dietary goal is required")]
    public string DietaryGoal { get; set; } = string.Empty;

    /// <summary>
    /// List of dietary restrictions (e.g., Vegetarian, Vegan, Gluten-Free, etc.)
    /// </summary>
    public List<string> DietaryRestrictions { get; set; } = new();

    /// <summary>
    /// User's food allergies (optional).
    /// </summary>
    [StringLength(500, ErrorMessage = "Allergies must not exceed 500 characters")]
    public string? Allergies { get; set; }

    /// <summary>
    /// Foods the user dislikes (optional).
    /// </summary>
    [StringLength(500, ErrorMessage = "Dislikes must not exceed 500 characters")]
    public string? Dislikes { get; set; }

    /// <summary>
    /// Preferred meal type (e.g., Breakfast, Lunch, Dinner, Snack, Any).
    /// </summary>
    [Required(ErrorMessage = "Preferred meal type is required")]
    public string PreferredMealType { get; set; } = "Any";

    /// <summary>
    /// Calorie preference for recommendations (e.g., Low, Medium, High, No Preference).
    /// </summary>
    [Required(ErrorMessage = "Calorie preference is required")]
    public string CaloriePreference { get; set; } = "No Preference";

    /// <summary>
    /// Type of menu to generate (e.g., Single Item, Light Meal, Full Meal, etc.).
    /// </summary>
    [Required(ErrorMessage = "Menu type is required")]
    public string MenuType { get; set; } = "Light Meal";
}
