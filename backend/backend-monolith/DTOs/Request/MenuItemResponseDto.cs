using System;
using System.Text.Json;

namespace backend_monolith.DTOs;

public record MenuItemResponseDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool Available,
    int Visibility,
    JsonDocument? Components,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt
);