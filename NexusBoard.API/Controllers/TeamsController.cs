using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusBoard.API.DTOs.Teams;
using NexusBoard.API.Interfaces.IServices;
using System.Security.Claims;

namespace NexusBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyTeams()
    {
        var userId = GetCurrentUserId();
        var teams = await _teamService.GetMyTeamsAsync(userId);
        return Ok(teams);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var userId = GetCurrentUserId();
        var response = await _teamService.CreateTeamAsync(request, userId);
        return Ok(response);
    }

    [HttpPost("{teamId}/members")]
    public async Task<IActionResult> AddTeamMember(Guid teamId, [FromBody] AddTeamMemberRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var response = await _teamService.AddTeamMemberAsync(teamId, request, userId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{teamId}/members/{memberId}")]
    public async Task<IActionResult> RemoveTeamMember(Guid teamId, Guid memberId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _teamService.RemoveTeamMemberAsync(teamId, memberId, userId);
            return Ok(new { message = "Member removed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}