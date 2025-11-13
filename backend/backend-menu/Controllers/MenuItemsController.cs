using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using backend_menu.DTOs;
using backend_menu.Repositories;
using backend_menu.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_menu.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly IBlobStorageService _blobStorage;
    private readonly IMenuItemRepository _menuItemRepo;

    public MenuItemsController(
        IMenuService menuService,
        IBlobStorageService blobStorage,
        IMenuItemRepository menuItemRepo)
    {
        _menuService = menuService;
        _blobStorage = blobStorage;
        _menuItemRepo = menuItemRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _menuService.GetAllMenuItemsAsync(ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _menuService.GetMenuItemByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId, CancellationToken ct)
    {
        var result = await _menuService.GetMenuItemsByCategoryAsync(categoryId, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMenuItemDto dto, CancellationToken ct)
    {
        var result = await _menuService.CreateMenuItemAsync(dto, ct);
        return result.IsSuccess 
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value) 
            : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateMenuItemDto dto, CancellationToken ct)
    {
        var result = await _menuService.UpdateMenuItemAsync(id, dto, ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _menuService.DeleteMenuItemAsync(id, ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{id:guid}/image")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile image, CancellationToken ct)
    {
        if (image == null || image.Length == 0)
            return BadRequest("No image provided");

        var menuItem = await _menuItemRepo.GetByIdAsync(id, ct);
        if (menuItem == null)
            return NotFound("Menu item not found");

        // Delete old image if exists
        if (!string.IsNullOrEmpty(menuItem.ImageUrl))
            await _blobStorage.DeleteImageAsync(menuItem.ImageUrl, ct);

        // Upload new image
        using var stream = image.OpenReadStream();
        var uploadResult = await _blobStorage.UploadImageAsync(stream, image.FileName, ct);
        
        if (!uploadResult.IsSuccess)
            return BadRequest(uploadResult.Error);

        menuItem.ImageUrl = uploadResult.Value;
        await _menuItemRepo.UpdateAsync(menuItem, ct);

        return Ok(new { imageUrl = uploadResult.Value });
    }
}