using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoard.Core.Entities;
using NexusBoard.Infrastructure.Data;
using NexusBoard.API.DTOs.WorkItems;
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
            .Select(wi => new WorkItemListResponse
            {
                Id = wi.Id,
                Title = wi.Title,
                Description = wi.Description,
                Status = wi.Status,
                Priority = wi.Priority,
                DueDate = wi.DueDate,
                CreatedAt = wi.CreatedAt,
                CompletedAt = wi.CompletedAt,
                Assignee = wi.Assignee != null ? new WorkItemAssigneeDto
                {
                    Id = wi.Assignee.Id,
                    FirstName = wi.Assignee.FirstName,
                    LastName = wi.Assignee.LastName,
                    Email = wi.Assignee.Email
                } : null,
                Creator = new WorkItemCreatorDto
                {
                    Id = wi.Creator.Id,
                    FirstName = wi.Creator.FirstName,
                    LastName = wi.Creator.LastName
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
            .Select(wi => new WorkItemDetailResponse
            {
                Id = wi.Id,
                Title = wi.Title,
                Description = wi.Description,
                Status = wi.Status,
                Priority = wi.Priority,
                DueDate = wi.DueDate,
                CreatedAt = wi.CreatedAt,
                CompletedAt = wi.CompletedAt,
                Project = new WorkItemProjectDto
                {
                    Id = wi.Project.Id,
                    Name = wi.Project.Name,
                    Team = new WorkItemTeamDto
                    {
                        Id = wi.Project.Team.Id,
                        Name = wi.Project.Team.Name
                    }
                },
                Assignee = wi.Assignee != null ? new WorkItemAssigneeDto
                {
                    Id = wi.Assignee.Id,
                    FirstName = wi.Assignee.FirstName,
                    LastName = wi.Assignee.LastName,
                    Email = wi.Assignee.Email
                } : null,
                Creator = new WorkItemCreatorDetailDto
                {
                    Id = wi.Creator.Id,
                    FirstName = wi.Creator.FirstName,
                    LastName = wi.Creator.LastName,
                    Email = wi.Creator.Email
                },
                Files = wi.Files
                    .Where(f => f.IsActive)
                    .Select(f => new WorkItemFileDto
                    {
                        Id = f.Id,
                        OriginalFileName = f.OriginalFileName,
                        FileSizeBytes = f.FileSizeBytes,
                        ContentType = f.ContentType,
                        UploadedAt = f.UploadedAt
                    })
                    .ToList()
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
            .Select(wi => new CreateWorkItemResponse
            {
                Id = wi.Id,
                Title = wi.Title,
                Description = wi.Description,
                Status = wi.Status,
                Priority = wi.Priority,
                DueDate = wi.DueDate,
                CreatedAt = wi.CreatedAt,
                Project = new WorkItemProjectSimpleDto
                {
                    Id = wi.Project.Id,
                    Name = wi.Project.Name
                },
                Assignee = wi.Assignee != null ? new WorkItemAssigneeSimpleDto
                {
                    Id = wi.Assignee.Id,
                    FirstName = wi.Assignee.FirstName,
                    LastName = wi.Assignee.LastName
                } : null,
                Creator = new WorkItemCreatorDto
                {
                    Id = wi.Creator.Id,
                    FirstName = wi.Creator.FirstName,
                    LastName = wi.Creator.LastName
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