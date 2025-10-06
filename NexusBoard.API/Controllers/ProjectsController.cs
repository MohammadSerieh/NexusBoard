using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoard.Core.Entities;
using NexusBoard.Infrastructure.Data;
using NexusBoard.API.DTOs.Projects;
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
        
        var projects = await _context.Projects
            .Where(p => p.IsActive && 
                       _context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                     tm.UserId == userId && 
                                                     tm.IsActive))
            .Include(p => p.Team)
            .Include(p => p.Creator)
            .Include(p => p.Tasks.Where(t => t.IsActive))
            .Select(p => new ProjectListResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                Priority = p.Priority,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                Team = new ProjectTeamDto
                {
                    Id = p.Team.Id,
                    Name = p.Team.Name
                },
                Creator = new ProjectCreatorDto
                {
                    Id = p.Creator.Id,
                    FirstName = p.Creator.FirstName,
                    LastName = p.Creator.LastName
                },
                TaskCounts = new TaskCountsDto
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
            .Select(p => new ProjectDetailResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                Priority = p.Priority,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                Team = new ProjectTeamDetailDto
                {
                    Id = p.Team.Id,
                    Name = p.Team.Name,
                    Description = p.Team.Description,
                    Members = p.Team.Members
                        .Where(m => m.IsActive)
                        .Select(m => new ProjectTeamMemberDto
                        {
                            Id = m.User.Id,
                            FirstName = m.User.FirstName,
                            LastName = m.User.LastName,
                            Email = m.User.Email,
                            Role = m.Role.ToString()
                        })
                        .ToList()
                },
                Creator = new ProjectCreatorDetailDto
                {
                    Id = p.Creator.Id,
                    FirstName = p.Creator.FirstName,
                    LastName = p.Creator.LastName,
                    Email = p.Creator.Email
                },
                Tasks = p.Tasks
                    .Where(t => t.IsActive)
                    .Select(t => new ProjectTaskDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        Priority = t.Priority,
                        DueDate = t.DueDate,
                        CreatedAt = t.CreatedAt,
                        Assignee = t.Assignee != null ? new ProjectTaskAssigneeDto
                        {
                            Id = t.Assignee.Id,
                            FirstName = t.Assignee.FirstName,
                            LastName = t.Assignee.LastName
                        } : null
                    })
                    .OrderBy(t => t.Status)
                    .ThenByDescending(t => t.Priority)
                    .ToList()
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
            .Select(p => new CreateProjectResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                Priority = p.Priority,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                Team = new ProjectTeamDto
                {
                    Id = p.Team.Id,
                    Name = p.Team.Name
                },
                Creator = new ProjectCreatorDto
                {
                    Id = p.Creator.Id,
                    FirstName = p.Creator.FirstName,
                    LastName = p.Creator.LastName
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