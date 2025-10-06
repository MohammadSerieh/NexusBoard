using Microsoft.EntityFrameworkCore;
using NexusBoard.API.Interfaces.IRepositories;
using NexusBoard.Core.Entities;
using NexusBoard.Infrastructure.Data;

namespace NexusBoard.API.Repositories;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly NexusBoardDbContext _context;

    public WorkItemRepository(NexusBoardDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UserHasProjectAccessAsync(Guid userId, Guid projectId)
    {
        return await _context.Projects
            .AnyAsync(p => p.Id == projectId && p.IsActive &&
                          _context.TeamMembers.Any(tm => tm.TeamId == p.TeamId && 
                                                        tm.UserId == userId && 
                                                        tm.IsActive));
    }

    public async Task<List<WorkItem>> GetProjectWorkItemsWithDetailsAsync(Guid projectId)
    {
        return await _context.WorkItems
            .Where(wi => wi.ProjectId == projectId && wi.IsActive)
            .Include(wi => wi.Assignee)
            .Include(wi => wi.Creator)
            .Include(wi => wi.Files.Where(f => f.IsActive))
            .OrderBy(wi => wi.Status)
            .ThenByDescending(wi => wi.Priority)
            .ThenBy(wi => wi.CreatedAt)
            .ToListAsync();
    }

    public async Task<WorkItem?> GetWorkItemWithDetailsAsync(Guid workItemId, Guid userId)
    {
        return await _context.WorkItems
            .Where(wi => wi.Id == workItemId && wi.IsActive &&
                        _context.TeamMembers.Any(tm => tm.TeamId == wi.Project.TeamId && 
                                                      tm.UserId == userId && 
                                                      tm.IsActive))
            .Include(wi => wi.Project)
                .ThenInclude(p => p.Team)
            .Include(wi => wi.Assignee)
            .Include(wi => wi.Creator)
            .Include(wi => wi.Files.Where(f => f.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<WorkItem> CreateWorkItemAsync(WorkItem workItem)
    {
        _context.WorkItems.Add(workItem);
        await _context.SaveChangesAsync();
        return workItem;
    }

    public async Task<WorkItem?> GetWorkItemWithRelationsAsync(Guid workItemId)
    {
        return await _context.WorkItems
            .Where(wi => wi.Id == workItemId)
            .Include(wi => wi.Assignee)
            .Include(wi => wi.Creator)
            .Include(wi => wi.Project)
            .FirstOrDefaultAsync();
    }

    public async Task<Project?> GetProjectAsync(Guid projectId)
    {
        return await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
    }

    public async Task<bool> IsUserTeamMemberAsync(Guid userId, Guid teamId)
    {
        return await _context.TeamMembers
            .AnyAsync(tm => tm.TeamId == teamId && 
                           tm.UserId == userId && 
                           tm.IsActive);
    }

    public async Task<WorkItem?> GetWorkItemForUpdateAsync(Guid workItemId, Guid userId)
    {
        return await _context.WorkItems
            .Where(wi => wi.Id == workItemId && wi.IsActive &&
                        _context.TeamMembers.Any(tm => tm.TeamId == wi.Project.TeamId && 
                                                      tm.UserId == userId && 
                                                      tm.IsActive))
            .Include(wi => wi.Project)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateWorkItemAsync(WorkItem workItem)
    {
        _context.WorkItems.Update(workItem);
        await _context.SaveChangesAsync();
    }

    public async Task<WorkItem?> GetWorkItemForDeleteAsync(Guid workItemId, Guid userId)
    {
        return await _context.WorkItems
            .Where(wi => wi.Id == workItemId && wi.IsActive &&
                        (_context.TeamMembers.Any(tm => tm.TeamId == wi.Project.TeamId && 
                                                        tm.UserId == userId && 
                                                        tm.Role == TeamRole.TeamLead && 
                                                        tm.IsActive) ||
                         wi.CreatedBy == userId))
            .FirstOrDefaultAsync();
    }

    public async Task SoftDeleteWorkItemAsync(WorkItem workItem)
    {
        workItem.IsActive = false;
        await _context.SaveChangesAsync();
    }
}