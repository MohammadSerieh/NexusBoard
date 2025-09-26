using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoard.Core.Entities;
using NexusBoard.Infrastructure.Data;
using System.Security.Claims;

namespace NexusBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class TeamsController : ControllerBase
{
    private readonly NexusBoardDbContext _context;

    public TeamsController(NexusBoardDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyTeams()
    {
        var userId = GetCurrentUserId();
        
        var teams = await _context.TeamMembers
            .Where(tm => tm.UserId == userId && tm.IsActive)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Creator)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Members.Where(m => m.IsActive))
                .ThenInclude(m => m.User)
            .Select(tm => new
            {
                tm.Team.Id,
                tm.Team.Name,
                tm.Team.Description,
                tm.Team.CreatedAt,
                Creator = new
                {
                    tm.Team.Creator.Id,
                    tm.Team.Creator.FirstName,
                    tm.Team.Creator.LastName,
                    tm.Team.Creator.Email
                },
                MyRole = tm.Role,
                MemberCount = tm.Team.Members.Count(m => m.IsActive),
                Members = tm.Team.Members
                    .Where(m => m.IsActive)
                    .Take(5) // Show first 5 members
                    .Select(m => new
                    {
                        m.User.Id,
                        m.User.FirstName,
                        m.User.LastName,
                        m.User.Email,
                        Role = m.Role
                    })
            })
            .ToListAsync();

        return Ok(teams);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
    {
        var userId = GetCurrentUserId();

        // Create the team
        var team = new Team
        {
            Name = request.Name,
            Description = request.Description,
            CreatedBy = userId
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        // Add creator as team lead
        var teamMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = userId,
            Role = TeamRole.TeamLead
        };

        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();

        // Return the created team
        var createdTeam = await _context.Teams
            .Where(t => t.Id == team.Id)
            .Include(t => t.Creator)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.CreatedAt,
                Creator = new
                {
                    t.Creator.Id,
                    t.Creator.FirstName,
                    t.Creator.LastName,
                    t.Creator.Email
                },
                MemberCount = 1
            })
            .FirstOrDefaultAsync();

        return Ok(createdTeam);
    }

    [HttpPost("{teamId}/members")]
    public async Task<IActionResult> AddTeamMember(Guid teamId, [FromBody] AddTeamMemberRequest request)
    {
        var userId = GetCurrentUserId();

        // Check if current user is team lead
        var isTeamLead = await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId && 
                           tm.Role == TeamRole.TeamLead && tm.IsActive);

        if (!isTeamLead)
        {
            return Forbid("Only team leads can add members");
        }

        // Check if user exists
        var userToAdd = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive);

        if (userToAdd == null)
        {
            return BadRequest(new { message = "User not found" });
        }

        // Check if user is already a member
        var existingMember = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userToAdd.Id);

        if (existingMember != null)
        {
            if (existingMember.IsActive)
            {
                return BadRequest(new { message = "User is already a team member" });
            }
            else
            {
                // Reactivate existing member
                existingMember.IsActive = true;
                existingMember.Role = TeamRole.Member;
                existingMember.JoinedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Add new team member
            var teamMember = new TeamMember
            {
                TeamId = teamId,
                UserId = userToAdd.Id,
                Role = TeamRole.Member
            };

            _context.TeamMembers.Add(teamMember);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            userToAdd.Id,
            userToAdd.FirstName,
            userToAdd.LastName,
            userToAdd.Email,
            Role = TeamRole.Member,
            JoinedAt = DateTime.UtcNow
        });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}

public class CreateTeamRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AddTeamMemberRequest
{
    public string Email { get; set; } = string.Empty;
}