using System;

namespace backend_menu.DTOs;

public record CategoryResponseDto(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);