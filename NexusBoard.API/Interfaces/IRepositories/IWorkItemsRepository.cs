using NexusBoard.Core.Entities;

namespace NexusBoard.API.Interfaces.IRepositories;

public interface IWorkItemRepository
{
    Task<bool> UserHasProjectAccessAsync(Guid userId, Guid projectId);
    Task<List<WorkItem>> GetProjectWorkItemsWithDetailsAsync(Guid projectId);
    Task<WorkItem?> GetWorkItemWithDetailsAsync(Guid workItemId, Guid userId);
    Task<WorkItem> CreateWorkItemAsync(WorkItem workItem);
    Task<WorkItem?> GetWorkItemWithRelationsAsync(Guid workItemId);
    Task<Project?> GetProjectAsync(Guid projectId);
    Task<bool> IsUserTeamMemberAsync(Guid userId, Guid teamId);
    Task<WorkItem?> GetWorkItemForUpdateAsync(Guid workItemId, Guid userId);
    Task UpdateWorkItemAsync(WorkItem workItem);
    Task<WorkItem?> GetWorkItemForDeleteAsync(Guid workItemId, Guid userId);
    Task SoftDeleteWorkItemAsync(WorkItem workItem);
}