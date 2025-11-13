using System;
using System.Text.Json;

namespace backend_menu.DTOs;

public record CreateMenuItemDto(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool Available,
    JsonDocument? Components
);