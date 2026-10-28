using GroceryListAi.Api.Dtos;
using GroceryListAi.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroceryListAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ItemResponse>>> GetItemsAsync()
    {
        try
        {
            var items = await _itemService.GetAllAsync();
            return Ok(items.Select(ItemResponse.FromItem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing items");
            return StatusCode(500, new { message = "Error listing items" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ItemResponse>> CreateItemAsync(ItemCreateRequest request)
    {
        try
        {
            var item = await _itemService.CreateAsync(request.Description);
            return Ok(ItemResponse.FromItem(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item {Description}", request.Description);
            return StatusCode(500, new { message = "Error creating item" });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteItemAsync(int id)
    {
        try
        {
            var deleted = await _itemService.DeleteAsync(id);
            return deleted ? Ok(new { message = "Item deleted" }) : NotFound(new { message = "Item not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting item {ItemId}", id);
            return StatusCode(500, new { message = "Error deleting item" });
        }
    }

    [HttpPatch("{id:int}/checked")]
    public async Task<ActionResult<ItemResponse>> UpdateCheckedAsync(int id, ItemCheckedUpdateRequest request)
    {
        try
        {
            var item = await _itemService.UpdateCheckedAsync(id, request.Checked);
            return item is null ? NotFound(new { message = "Item not found" }) : Ok(ItemResponse.FromItem(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating checked state of item {ItemId}", id);
            return StatusCode(500, new { message = "Error updating item" });
        }
    }
}
