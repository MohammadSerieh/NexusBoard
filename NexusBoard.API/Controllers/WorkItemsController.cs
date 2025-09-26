using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoard.Core.Entities;
using NexusBoard.Infrastructure.Data;
using System.Security.Claims;

namespace NexusBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkItemsController : ControllerBase
{
    private readonly NexusBoardDbContext _context;

    public WorkItemsController(NexusBoardDbContext context)
    {
        _context = context;
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectWorkItems(Guid projectId)
    {
        var userId = GetCurrentUserId();
        
        // Verify user has access to this project
        var hasAccess = await _context.Projects
            .AnyAsync(p => p.Id == projectId && p.IsActive &&
                          _context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                        tm.UserId == userId && 
                                                        tm.IsActive));

        if (!hasAccess)
        {
            return Forbid("Access denied to this project");
        }

        var workItems = await _context.WorkItems
            .Where(wi => wi.ProjectId == projectId && wi.IsActive)
            .Include(wi => wi.Assignee)
            .Include(wi => wi.Creator)
            .Select(wi => new
            {
                wi.Id,
                wi.Title,
                wi.Description,
                wi.Status,
                wi.Priority,
                wi.DueDate,
                wi.CreatedAt,
                wi.CompletedAt,
                Assignee = wi.Assignee != null ? new
                {
                    wi.Assignee.Id,
                    wi.Assignee.FirstName,
                    wi.Assignee.LastName,
                    wi.Assignee.Email
                } : null,
                Creator = new
                {
                    wi.Creator.Id,
                    wi.Creator.FirstName,
                    wi.Creator.LastName
                },
                FileCount = wi.Files.Count(f => f.IsActive)
            })
            .OrderBy(wi => wi.Status)
            .ThenByDescending(wi => wi.Priority)
            .ThenBy(wi => wi.CreatedAt)
            .ToListAsync();

        return Ok(workItems);
    }

    [HttpGet("{workItemId}")]
    public async Task<IActionResult> GetWorkItem(Guid workItemId)
    {
        var userId = GetCurrentUserId();
        
        var workItem = await _context.WorkItems
            .Where(wi => wi.Id == workItemId && wi.IsActive &&
                        _context.TeamMembers.Any(tm => tm.TeamId == wi.Project.TeamId && 
                                                      tm.UserId == userId && 
                                                      tm.IsActive))
            .Include(wi => wi.Project)
                .ThenInclude(p => p.Team)
            .Include(wi => wi.Assignee)
            .Include(wi => wi.Creator)
            .Include(wi => wi.Files.Where(f => f.IsActive))
            .Select(wi => new
            {
                wi.Id,
                wi.Title,
                wi.Description,
                wi.Status,
                wi.Priority,
                wi.DueDate,
                wi.CreatedAt,
                wi.CompletedAt,
                Project = new
                {
                    wi.Project.Id,
                    wi.Project.Name,
                    Team = new
                    {
                        wi.Project.Team.Id,
                        wi.Project.Team.Name
                    }
                },
                Assignee = wi.Assignee != null ? new
                {
                    wi.Assignee.Id,
                    wi.Assignee.FirstName,
                    wi.Assignee.LastName,
                    wi.Assignee.Email
                } : null,
                Creator = new
                {
                    wi.Creator.Id,
                    wi.Creator.FirstName,
                    wi.Creator.LastName,
                    wi.Creator.Email
                },
                Files = wi.Files
                    .Where(f => f.IsActive)
                    .Select(f => new
                    {
                        f.Id,
                        f.OriginalFileName,
                        f.FileSizeBytes,
                        f.ContentType,
                        f.UploadedAt
                    })
            })
            .FirstOrDefaultAsync();

        if (workItem == null)
        {
            return NotFound(new { message = "Work item not found or access denied" });
        }

        return Ok(workItem);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkItem([FromBody] CreateWorkItemRequest request)
    {
        var userId = GetCurrentUserId();

        // Verify user has access to the project
        var hasAccess = await _context.Projects
            .AnyAsync(p => p.Id == request.ProjectId && p.IsActive &&
                          _context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                        tm.UserId == userId && 
                                                        tm.IsActive));

        if (!hasAccess)
        {
            return Forbid("Access denied to this project");
        }

        // Verify assignee is team member (if provided)
        if (request.AssigneeId.HasValue)
        {
            var project = await _context.Projects.FirstAsync(p => p.Id == request.ProjectId);
            var isAssigneeTeamMember = await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == project.TeamId && 
                               tm.UserId == request.AssigneeId.Value && 
                               tm.IsActive);

            if (!isAssigneeTeamMember)
            {
                return BadRequest(new { message = "Assignee must be a team member" });
            }
        }

        var workItem = new WorkItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            AssigneeId = request.AssigneeId,
            CreatedBy = userId,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate
        };

        _context.WorkItems.Add(workItem);
        await _context.SaveChangesAsync();

        var createdWorkItem = await _context.WorkItems
            .Where(wi => wi.Id == workItem.Id)
            .Include(wi => wi.Assignee)
            .Include(wi => wi.Creator)
            .Include(wi => wi.Project)
            .Select(wi => new
            {
                wi.Id,
                wi.Title,
                wi.Description,
                wi.Status,
                wi.Priority,
                wi.DueDate,
                wi.CreatedAt,
                Project = new
                {
                    wi.Project.Id,
                    wi.Project.Name
                },
                Assignee = wi.Assignee != null ? new
                {
                    wi.Assignee.Id,
                    wi.Assignee.FirstName,
                    wi.Assignee.LastName
                } : null,
                Creator = new
                {
                    wi.Creator.Id,
                    wi.Creator.FirstName,
                    wi.Creator.LastName
                }
            })
            .FirstOrDefaultAsync();

        return Ok(createdWorkItem);
    }

    [HttpPut("{workItemId}")]
    public async Task<IActionResult> UpdateWorkItem(Guid workItemId, [FromBody] UpdateWorkItemRequest request)
    {
        var userId = GetCurrentUserId();

        var workItem = await _context.WorkItems
            .Where(wi => wi.Id == workItemId && wi.IsActive &&
                        _context.TeamMembers.Any(tm => tm.TeamId == wi.Project.TeamId && 
                                                      tm.UserId == userId && 
                                                      tm.IsActive))
            .Include(wi => wi.Project)
            .FirstOrDefaultAsync();

        if (workItem == null)
        {
            return NotFound(new { message = "Work item not found or access denied" });
        }

        // Verify assignee is team member (if provided)
        if (request.AssigneeId.HasValue)
        {
            var isAssigneeTeamMember = await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == workItem.Project.TeamId && 
                               tm.UserId == request.AssigneeId.Value && 
                               tm.IsActive);

            if (!isAssigneeTeamMember)
            {
                return BadRequest(new { message = "Assignee must be a team member" });
            }
        }

        // Update fields
        workItem.Title = request.Title;
        workItem.Description = request.Description;
        workItem.Status = request.Status;
        workItem.Priority = request.Priority;
        workItem.DueDate = request.DueDate;
        workItem.AssigneeId = request.AssigneeId;

        // Set completion date when marking as done
        if (request.Status == WorkItemStatus.Done && workItem.CompletedAt == null)
        {
            workItem.CompletedAt = DateTime.UtcNow;
        }
        else if (request.Status != WorkItemStatus.Done)
        {
            workItem.CompletedAt = null;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Work item updated successfully" });
    }

    [HttpDelete("{workItemId}")]
    public async Task<IActionResult> DeleteWorkItem(Guid workItemId)
    {
        var userId = GetCurrentUserId();

        var workItem = await _context.WorkItems
            .Where(wi => wi.Id == workItemId && wi.IsActive &&
                        (_context.TeamMembers.Any(tm => tm.TeamId == wi.Project.TeamId && 
                                                        tm.UserId == userId && 
                                                        tm.Role == TeamRole.TeamLead && 
                                                        tm.IsActive) ||
                         wi.CreatedBy == userId))
            .FirstOrDefaultAsync();

        if (workItem == null)
        {
            return NotFound(new { message = "Work item not found or insufficient permissions" });
        }

        // Soft delete
        workItem.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Work item deleted successfully" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}

public class CreateWorkItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Todo;
    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;
    public DateTime? DueDate { get; set; }
}

public class UpdateWorkItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
}