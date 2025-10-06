using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBoard.API.DTOs.Projects;
using NexusBoard.API.Interfaces.IServices;
using System.Security.Claims;

namespace NexusBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProjects()
    {
        var userId = GetCurrentUserId();
        var projects = await _projectService.GetMyProjectsAsync(userId);
        return Ok(projects);
    }

    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProject(Guid projectId)
    {
        var userId = GetCurrentUserId();
        var project = await _projectService.GetProjectAsync(projectId, userId);

        if (project == null)
        {
            return NotFound(new { message = "Project not found or access denied" });
        }

        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _projectService.CreateProjectAsync(request, userId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{projectId}")]
    public async Task<IActionResult> UpdateProject(Guid projectId, [FromBody] UpdateProjectRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _projectService.UpdateProjectAsync(projectId, request, userId);
            return Ok(new { message = "Project updated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{projectId}")]
    public async Task<IActionResult> DeleteProject(Guid projectId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _projectService.DeleteProjectAsync(projectId, userId);
            return Ok(new { message = "Project deleted successfully" });
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