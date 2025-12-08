using System;
using System.Text.Json;

namespace backend.DTOs.Menu.Request;

public record CreateMenuItemDto(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool Available,
    JsonDocument? Components
);