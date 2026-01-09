using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.DTOs.Menu.Request;

public record CreateMenuItemDto(
    [property: JsonRequired] Guid CategoryId,
    string Name,
    string? Description,
    [property: JsonRequired] decimal Price,
    string Currency,
    [property: JsonRequired] bool Available,
    JsonDocument? Components
);