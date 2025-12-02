namespace backend.DTOs.Menu.Response;

public record CategoryResponseDto(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);