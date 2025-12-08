namespace backend.DTOs.Menu.Request;

public record CreateCategoryDto(
    string Name,
    string? Description,
    int DisplayOrder,
    bool IsActive
);