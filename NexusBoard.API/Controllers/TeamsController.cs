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

    [HttpDelete("{teamId}")]
    public async Task<IActionResult> DeleteTeam(Guid teamId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _teamService.DeleteTeamAsync(teamId, userId);
            return Ok(new { message = "Team deleted successfuly" });
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
    
    [HttpGet("{teamId}/members")]
    public async Task<IActionResult> GetTeamMembers(Guid teamId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Verify user is a member of this team
            if (!await _teamService.IsUserTeamMemberAsync(userId, teamId))
            {
                return Forbid("You are not a member of this team");
            }
            
            var members = await _teamService.GetTeamMembersAsync(teamId);
            return Ok(members);
        }
        catch (Exception ex)
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