using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusBoard.Core.Entities;
using NexusBoard.Infrastructure.Data;
using NexusBoard.API.DTOs.Teams;
using System.Security.Claims;

namespace NexusBoard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
            .Select(tm => new TeamResponse
            {
                Id = tm.Team.Id,
                Name = tm.Team.Name,
                Description = tm.Team.Description,
                CreatedAt = tm.Team.CreatedAt,
                Creator = new TeamCreatorDto
                {
                    Id = tm.Team.Creator.Id,
                    FirstName = tm.Team.Creator.FirstName,
                    LastName = tm.Team.Creator.LastName,
                    Email = tm.Team.Creator.Email
                },
                MyRole = tm.Role.ToString(),
                MemberCount = tm.Team.Members.Count(m => m.IsActive),
                Members = tm.Team.Members
                    .Where(m => m.IsActive)
                    .Take(5)
                    .Select(m => new TeamMemberDto
                    {
                        Id = m.User.Id,
                        FirstName = m.User.FirstName,
                        LastName = m.User.LastName,
                        Email = m.User.Email,
                        Role = m.Role.ToString()
                    })
                    .ToList()
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
            .Select(t => new CreateTeamResponse
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                CreatedAt = t.CreatedAt,
                Creator = new TeamCreatorDto
                {
                    Id = t.Creator.Id,
                    FirstName = t.Creator.FirstName,
                    LastName = t.Creator.LastName,
                    Email = t.Creator.Email
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

        var response = new AddMemberResponse
        {
            Id = userToAdd.Id,
            FirstName = userToAdd.FirstName,
            LastName = userToAdd.LastName,
            Email = userToAdd.Email,
            Role = TeamRole.Member.ToString(),
            JoinedAt = DateTime.UtcNow
        };

        return Ok(response);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}