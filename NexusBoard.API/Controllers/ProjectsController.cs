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
public class ProjectsController : ControllerBase
{
    private readonly NexusBoardDbContext _context;

    public ProjectsController(NexusBoardDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProjects()
    {
        var userId = GetCurrentUserId();
        
        // Get projects from teams where user is a member
        var projects = await _context.Projects
            .Where(p => p.IsActive && 
                       _context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                     tm.UserId == userId && 
                                                     tm.IsActive))
            .Include(p => p.Team)
            .Include(p => p.Creator)
            .Include(p => p.Tasks.Where(t => t.IsActive))
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Status,
                p.Priority,
                p.StartDate,
                p.EndDate,
                p.CreatedAt,
                Team = new
                {
                    p.Team.Id,
                    p.Team.Name
                },
                Creator = new
                {
                    p.Creator.Id,
                    p.Creator.FirstName,
                    p.Creator.LastName
                },
                TaskCounts = new
                {
                    Total = p.Tasks.Count(t => t.IsActive),
                    Todo = p.Tasks.Count(t => t.IsActive && t.Status == WorkItemStatus.Todo),
                    InProgress = p.Tasks.Count(t => t.IsActive && t.Status == WorkItemStatus.InProgress),
                    Review = p.Tasks.Count(t => t.IsActive && t.Status == WorkItemStatus.Review),
                    Done = p.Tasks.Count(t => t.IsActive && t.Status == WorkItemStatus.Done)
                }
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProject(Guid projectId)
    {
        var userId = GetCurrentUserId();
        
        var project = await _context.Projects
            .Where(p => p.Id == projectId && p.IsActive &&
                       _context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                     tm.UserId == userId && 
                                                     tm.IsActive))
            .Include(p => p.Team)
                .ThenInclude(t => t.Members.Where(m => m.IsActive))
                .ThenInclude(m => m.User)
            .Include(p => p.Creator)
            .Include(p => p.Tasks.Where(t => t.IsActive))
                .ThenInclude(t => t.Assignee)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Status,
                p.Priority,
                p.StartDate,
                p.EndDate,
                p.CreatedAt,
                Team = new
                {
                    p.Team.Id,
                    p.Team.Name,
                    p.Team.Description,
                    Members = p.Team.Members
                        .Where(m => m.IsActive)
                        .Select(m => new
                        {
                            m.User.Id,
                            m.User.FirstName,
                            m.User.LastName,
                            m.User.Email,
                            Role = m.Role
                        })
                },
                Creator = new
                {
                    p.Creator.Id,
                    p.Creator.FirstName,
                    p.Creator.LastName,
                    p.Creator.Email
                },
                Tasks = p.Tasks
                    .Where(t => t.IsActive)
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.Status,
                        t.Priority,
                        t.DueDate,
                        t.CreatedAt,
                        Assignee = t.Assignee != null ? new
                        {
                            t.Assignee.Id,
                            t.Assignee.FirstName,
                            t.Assignee.LastName
                        } : null
                    })
                    .OrderBy(t => t.Status)
                    .ThenByDescending(t => t.Priority)
            })
            .FirstOrDefaultAsync();

        if (project == null)
        {
            return NotFound(new { message = "Project not found or access denied" });
        }

        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var userId = GetCurrentUserId();

        // Verify user is member of the team
        var isTeamMember = await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == request.TeamId && 
                           tm.UserId == userId && 
                           tm.IsActive);

        if (!isTeamMember)
        {
            return Forbid("You must be a team member to create projects");
        }

        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            TeamId = request.TeamId,
            CreatedBy = userId,
            Status = request.Status,
            Priority = request.Priority,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var createdProject = await _context.Projects
            .Where(p => p.Id == project.Id)
            .Include(p => p.Team)
            .Include(p => p.Creator)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Status,
                p.Priority,
                p.StartDate,
                p.EndDate,
                p.CreatedAt,
                Team = new
                {
                    p.Team.Id,
                    p.Team.Name
                },
                Creator = new
                {
                    p.Creator.Id,
                    p.Creator.FirstName,
                    p.Creator.LastName
                }
            })
            .FirstOrDefaultAsync();

        return Ok(createdProject);
    }

    [HttpPut("{projectId}")]
    public async Task<IActionResult> UpdateProject(Guid projectId, [FromBody] UpdateProjectRequest request)
    {
        var userId = GetCurrentUserId();

        var project = await _context.Projects
            .Where(p => p.Id == projectId && p.IsActive &&
                       _context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                     tm.UserId == userId && 
                                                     tm.IsActive))
            .FirstOrDefaultAsync();

        if (project == null)
        {
            return NotFound(new { message = "Project not found or access denied" });
        }

        // Update fields
        project.Name = request.Name;
        project.Description = request.Description;
        project.Status = request.Status;
        project.Priority = request.Priority;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Project updated successfully" });
    }

    [HttpDelete("{projectId}")]
    public async Task<IActionResult> DeleteProject(Guid projectId)
    {
        var userId = GetCurrentUserId();

        var project = await _context.Projects
            .Where(p => p.Id == projectId && p.IsActive &&
                       (_context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                       tm.UserId == userId && 
                                                       tm.Role == TeamRole.TeamLead && 
                                                       tm.IsActive) ||
                        p.CreatedBy == userId))
            .FirstOrDefaultAsync();

        if (project == null)
        {
            return NotFound(new { message = "Project not found or insufficient permissions" });
        }

        // Soft delete
        project.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Project deleted successfully" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TeamId { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public ProjectPriority Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}