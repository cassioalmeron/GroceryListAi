using GroceryListAi.Api.Dtos;
using GroceryListAi.Core.Exceptions;
using GroceryListAi.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroceryListAi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListsController : ControllerBase
{
    private readonly IListService _listService;
    private readonly ILogger<ListsController> _logger;

    public ListsController(IListService listService, ILogger<ListsController> logger)
    {
        _listService = listService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ListResponse>>> GetListsAsync()
    {
        try
        {
            var lists = await _listService.GetAllAsync();
            return Ok(lists.Select(ListResponse.FromList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing lists");
            return StatusCode(500, new { message = "Error listing lists" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ListResponse>> CreateListAsync(ListRequest request)
    {
        try
        {
            var list = await _listService.CreateAsync(request.Name);
            return Ok(ListResponse.FromList(list));
        }
        catch (DuplicateListNameException ex)
        {
            _logger.LogWarning(ex, "Duplicate list name {ListName}", request.Name);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating list {ListName}", request.Name);
            return StatusCode(500, new { message = "Error creating list" });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ListResponse>> RenameListAsync(int id, ListRequest request)
    {
        try
        {
            var list = await _listService.RenameAsync(id, request.Name);
            return list is null ? NotFound(new { message = "List not found" }) : Ok(ListResponse.FromList(list));
        }
        catch (DuplicateListNameException ex)
        {
            _logger.LogWarning(ex, "Duplicate list name {ListName} while renaming list {ListId}", request.Name, id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renaming list {ListId}", id);
            return StatusCode(500, new { message = "Error renaming list" });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteListAsync(int id)
    {
        try
        {
            var result = await _listService.DeleteAsync(id);

            return result switch
            {
                ListDeleteResult.Deleted => Ok(new { message = "List deleted" }),
                ListDeleteResult.LastList => BadRequest(new { message = "The last list cannot be deleted" }),
                _ => NotFound(new { message = "List not found" })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting list {ListId}", id);
            return StatusCode(500, new { message = "Error deleting list" });
        }
    }
}
