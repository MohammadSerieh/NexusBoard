using Microsoft.EntityFrameworkCore;
using NexusBoard.API.Interfaces.IRepositories;
using NexusBoard.Core.Entities;
using NexusBoard.Infrastructure.Data;

namespace NexusBoard.API.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly NexusBoardDbContext _context;

    public ProjectRepository(NexusBoardDbContext context)
    {
        _context = context;
    }

    public async Task<List<Project>> GetUserProjectsWithDetailsAsync(Guid userId)
    {
        return await _context.Projects
            .Where(p => p.IsActive && 
                       _context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                     tm.UserId == userId && 
                                                     tm.IsActive))
            .Include(p => p.Team)
            .Include(p => p.Creator)
            .Include(p => p.Tasks.Where(t => t.IsActive))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetProjectWithDetailsAsync(Guid projectId, Guid userId)
    {
        return await _context.Projects
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
            .FirstOrDefaultAsync();
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project?> GetProjectWithCreatorAndTeamAsync(Guid projectId)
    {
        return await _context.Projects
            .Where(p => p.Id == projectId)
            .Include(p => p.Team)
            .Include(p => p.Creator)
            .FirstOrDefaultAsync();
    }

    public async Task<Project?> GetProjectForUpdateAsync(Guid projectId, Guid userId)
    {
        return await _context.Projects
            .Where(p => p.Id == projectId && p.IsActive &&
                       _context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                     tm.UserId == userId && 
                                                     tm.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task UpdateProjectAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task<Project?> GetProjectForDeleteAsync(Guid projectId, Guid userId)
    {
        return await _context.Projects
            .Where(p => p.Id == projectId && p.IsActive &&
                       (_context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                       tm.UserId == userId && 
                                                       tm.Role == TeamRole.TeamLead && 
                                                       tm.IsActive) ||
                        p.CreatedBy == userId))
            .FirstOrDefaultAsync();
    }

    public async Task SoftDeleteProjectAsync(Project project)
    {
        project.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsUserTeamMemberAsync(Guid userId, Guid teamId)
    {
        return await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == teamId && 
                           tm.UserId == userId && 
                           tm.IsActive);
    }
}