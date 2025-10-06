using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBoard.API.DTOs.WorkItems;
using NexusBoard.API.Interfaces.IServices;
using System.Security.Claims;

namespace NexusBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkItemsController : ControllerBase
{
    private readonly IWorkItemService _workItemService;

    public WorkItemsController(IWorkItemService workItemService)
    {
        _workItemService = workItemService;
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectWorkItems(Guid projectId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var workItems = await _workItemService.GetProjectWorkItemsAsync(projectId, userId);
            return Ok(workItems);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{workItemId}")]
    public async Task<IActionResult> GetWorkItem(Guid workItemId)
    {
        var userId = GetCurrentUserId();
        var workItem = await _workItemService.GetWorkItemAsync(workItemId, userId);

        if (workItem == null)
        {
            return NotFound(new { message = "Work item not found or access denied" });
        }

        return Ok(workItem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkItem([FromBody] CreateWorkItemRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _workItemService.CreateWorkItemAsync(request, userId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{workItemId}")]
    public async Task<IActionResult> UpdateWorkItem(Guid workItemId, [FromBody] UpdateWorkItemRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _workItemService.UpdateWorkItemAsync(workItemId, request, userId);
            return Ok(new { message = "Work item updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{workItemId}")]
    public async Task<IActionResult> DeleteWorkItem(Guid workItemId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _workItemService.DeleteWorkItemAsync(workItemId, userId);
            return Ok(new { message = "Work item deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}