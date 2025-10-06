using NexusBoard.API.DTOs.WorkItems;

namespace NexusBoard.API.Interfaces.IServices;

public interface IWorkItemService
{
    Task<List<WorkItemListResponse>> GetProjectWorkItemsAsync(Guid projectId, Guid userId);
    Task<WorkItemDetailResponse?> GetWorkItemAsync(Guid workItemId, Guid userId);
    Task<CreateWorkItemResponse> CreateWorkItemAsync(CreateWorkItemRequest request, Guid userId);
    Task UpdateWorkItemAsync(Guid workItemId, UpdateWorkItemRequest request, Guid userId);
    Task DeleteWorkItemAsync(Guid workItemId, Guid userId);
}