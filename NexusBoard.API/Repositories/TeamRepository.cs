using Microsoft.EntityFrameworkCore;
using NexusBoard.API.Interfaces.IRepositories;
using NexusBoard.Core.Entities;
using NexusBoard.Infrastructure.Data;

namespace NexusBoard.API.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly NexusBoardDbContext _context;

    public TeamRepository(NexusBoardDbContext context)
    {
        _context = context;
    }

    public async Task<List<Team>> GetUserTeamsWithDetailsAsync(Guid userId)
    {
        return await _context.TeamMembers
            .Where(tm => tm.UserId == userId && tm.IsActive)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Creator)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Members.Where(m => m.IsActive))
                .ThenInclude(m => m.User)
            .Select(tm => tm.Team)
            .ToListAsync();
    }

    public async Task<Team> CreateTeamAsync(Team team)
    {
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return team;
    }

    public async Task<Team?> GetTeamWithCreatorAsync(Guid teamId)
    {
        return await _context.Teams
            .Where(t => t.Id == teamId)
            .Include(t => t.Creator)
            .FirstOrDefaultAsync();
    }

    public async Task<TeamMember> AddTeamMemberAsync(TeamMember teamMember)
    {
        _context.TeamMembers.Add(teamMember);
        await _context.SaveChangesAsync();
        return teamMember;
    }

    public async Task<bool> IsUserTeamLeadAsync(Guid userId, Guid teamId)
    {
        return await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == teamId &&
                           tm.UserId == userId &&
                           tm.Role == TeamRole.TeamLead &&
                           tm.IsActive);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
    }

    public async Task<TeamMember?> GetTeamMemberAsync(Guid teamId, Guid userId)
    {
        return await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
    }

    public async Task UpdateTeamMemberAsync(TeamMember teamMember)
    {
        _context.TeamMembers.Update(teamMember);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTeamAsync(Guid teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            throw new InvalidOperationException("Team not found");
        }

        // Soft delete all team members
        foreach (var member in team.Members)
        {
            member.IsActive = false;
        }

        // Soft delete the team
        team.IsActive = false;

        await _context.SaveChangesAsync();
    }
    public async Task<bool> IsUserInTeamAsync(Guid userId, Guid teamId)
    {
        return await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == teamId &&
                        tm.UserId == userId &&
                        tm.IsActive);
    }
    
    public async Task<Team?> GetTeamByIdAsync(Guid teamId)
    {
        return await _context.Teams
            .Where(t => t.Id == teamId && t.IsActive)
            .Include(t => t.Members.Where(m => m.IsActive))
                .ThenInclude(m => m.User)
            .Include(t => t.Creator)
            .FirstOrDefaultAsync();
    }
}