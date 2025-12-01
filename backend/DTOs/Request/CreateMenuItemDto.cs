using System;
using System.Text.Json;

namespace backend.DTOs;

public record CreateMenuItemDto(
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool Available,
    JsonDocument? Components
);