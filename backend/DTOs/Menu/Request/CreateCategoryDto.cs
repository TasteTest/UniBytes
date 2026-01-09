using System.Text.Json.Serialization;

namespace backend.DTOs.Menu.Request;

public record CreateCategoryDto(
    string Name,
    string? Description,
    [property: JsonRequired] int DisplayOrder,
    [property: JsonRequired] bool IsActive
);