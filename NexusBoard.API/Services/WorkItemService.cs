using NexusBoard.API.DTOs.WorkItems;
using NexusBoard.API.Interfaces.IRepositories;
using NexusBoard.API.Interfaces.IServices;
using NexusBoard.Core.Entities;

namespace NexusBoard.API.Services;

public class WorkItemService : IWorkItemService
{
    private readonly IWorkItemRepository _workItemRepository;

    public WorkItemService(IWorkItemRepository workItemRepository)
    {
        _workItemRepository = workItemRepository;
    }

    public async Task<List<WorkItemListResponse>> GetProjectWorkItemsAsync(
        Guid projectId, 
        Guid userId)
    {
        // Verify user has access to this project
        if (!await _workItemRepository.UserHasProjectAccessAsync(userId, projectId))
        {
            throw new UnauthorizedAccessException("Access denied to this project");
        }

        var workItems = await _workItemRepository.GetProjectWorkItemsWithDetailsAsync(projectId);

        return workItems.Select(wi => new WorkItemListResponse
        {
            Id = wi.Id,
            Title = wi.Title,
            Description = wi.Description,
            Status = wi.Status,
            Priority = wi.Priority,
            DueDate = wi.DueDate,
            CreatedAt = wi.CreatedAt,
            CompletedAt = wi.CompletedAt,
            Assignee = wi.Assignee != null ? new WorkItemAssigneeDto
            {
                Id = wi.Assignee.Id,
                FirstName = wi.Assignee.FirstName,
                LastName = wi.Assignee.LastName,
                Email = wi.Assignee.Email
            } : null,
            Creator = new WorkItemCreatorDto
            {
                Id = wi.Creator.Id,
                FirstName = wi.Creator.FirstName,
                LastName = wi.Creator.LastName
            },
            FileCount = wi.Files.Count(f => f.IsActive)
        }).ToList();
    }

    public async Task<WorkItemDetailResponse?> GetWorkItemAsync(Guid workItemId, Guid userId)
    {
        var workItem = await _workItemRepository.GetWorkItemWithDetailsAsync(workItemId, userId);

        if (workItem == null)
        {
            return null;
        }

        return new WorkItemDetailResponse
        {
            Id = workItem.Id,
            Title = workItem.Title,
            Description = workItem.Description,
            Status = workItem.Status,
            Priority = workItem.Priority,
            DueDate = workItem.DueDate,
            CreatedAt = workItem.CreatedAt,
            CompletedAt = workItem.CompletedAt,
            Project = new WorkItemProjectDto
            {
                Id = workItem.Project.Id,
                Name = workItem.Project.Name,
                Team = new WorkItemTeamDto
                {
                    Id = workItem.Project.Team.Id,
                    Name = workItem.Project.Team.Name
                }
            },
            Assignee = workItem.Assignee != null ? new WorkItemAssigneeDto
            {
                Id = workItem.Assignee.Id,
                FirstName = workItem.Assignee.FirstName,
                LastName = workItem.Assignee.LastName,
                Email = workItem.Assignee.Email
            } : null,
            Creator = new WorkItemCreatorDetailDto
            {
                Id = workItem.Creator.Id,
                FirstName = workItem.Creator.FirstName,
                LastName = workItem.Creator.LastName,
                Email = workItem.Creator.Email
            },
            Files = workItem.Files
                .Where(f => f.IsActive)
                .Select(f => new WorkItemFileDto
                {
                    Id = f.Id,
                    OriginalFileName = f.OriginalFileName,
                    FileSizeBytes = f.FileSizeBytes,
                    ContentType = f.ContentType,
                    UploadedAt = f.UploadedAt
                })
                .ToList()
        };
    }

    public async Task<CreateWorkItemResponse> CreateWorkItemAsync(
        CreateWorkItemRequest request, 
        Guid userId)
    {
        // Verify user has access to the project
        if (!await _workItemRepository.UserHasProjectAccessAsync(userId, request.ProjectId))
        {
            throw new UnauthorizedAccessException("Access denied to this project");
        }

        // Verify assignee is team member (if provided)
        if (request.AssigneeId.HasValue)
        {
            var project = await _workItemRepository.GetProjectAsync(request.ProjectId);
            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }

            var isAssigneeTeamMember = await _workItemRepository
                .IsUserTeamMemberAsync(request.AssigneeId.Value, project.TeamId);

            if (!isAssigneeTeamMember)
            {
                throw new InvalidOperationException("Assignee must be a team member");
            }
        }

        // Create work item entity
        var workItem = new WorkItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            AssigneeId = request.AssigneeId,
            CreatedBy = userId,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate
        };

        // Save work item
        workItem = await _workItemRepository.CreateWorkItemAsync(workItem);

        // Reload with relations
        var createdWorkItem = await _workItemRepository.GetWorkItemWithRelationsAsync(workItem.Id);

        if (createdWorkItem == null)
        {
            throw new InvalidOperationException("Failed to retrieve created work item");
        }

        // Map to response DTO
        return new CreateWorkItemResponse
        {
            Id = createdWorkItem.Id,
            Title = createdWorkItem.Title,
            Description = createdWorkItem.Description,
            Status = createdWorkItem.Status,
            Priority = createdWorkItem.Priority,
            DueDate = createdWorkItem.DueDate,
            CreatedAt = createdWorkItem.CreatedAt,
            Project = new WorkItemProjectSimpleDto
            {
                Id = createdWorkItem.Project.Id,
                Name = createdWorkItem.Project.Name
            },
            Assignee = createdWorkItem.Assignee != null ? new WorkItemAssigneeSimpleDto
            {
                Id = createdWorkItem.Assignee.Id,
                FirstName = createdWorkItem.Assignee.FirstName,
                LastName = createdWorkItem.Assignee.LastName
            } : null,
            Creator = new WorkItemCreatorDto
            {
                Id = createdWorkItem.Creator.Id,
                FirstName = createdWorkItem.Creator.FirstName,
                LastName = createdWorkItem.Creator.LastName
            }
        };
    }

    public async Task UpdateWorkItemAsync(
        Guid workItemId, 
        UpdateWorkItemRequest request, 
        Guid userId)
    {
        var workItem = await _workItemRepository.GetWorkItemForUpdateAsync(workItemId, userId);

        if (workItem == null)
        {
            throw new KeyNotFoundException("Work item not found or access denied");
        }

        // Verify assignee is team member (if provided)
        if (request.AssigneeId.HasValue)
        {
            var isAssigneeTeamMember = await _workItemRepository
                .IsUserTeamMemberAsync(request.AssigneeId.Value, workItem.Project.TeamId);

            if (!isAssigneeTeamMember)
            {
                throw new InvalidOperationException("Assignee must be a team member");
            }
        }

        // Update fields
        workItem.Title = request.Title;
        workItem.Description = request.Description;
        workItem.Status = request.Status;
        workItem.Priority = request.Priority;
        workItem.DueDate = request.DueDate;
        workItem.AssigneeId = request.AssigneeId;

        // Set completion date when marking as done
        if (request.Status == WorkItemStatus.Done && workItem.CompletedAt == null)
        {
            workItem.CompletedAt = DateTime.UtcNow;
        }
        else if (request.Status != WorkItemStatus.Done)
        {
            workItem.CompletedAt = null;
        }

        await _workItemRepository.UpdateWorkItemAsync(workItem);
    }

    public async Task DeleteWorkItemAsync(Guid workItemId, Guid userId)
    {
        var workItem = await _workItemRepository.GetWorkItemForDeleteAsync(workItemId, userId);

        if (workItem == null)
        {
            throw new KeyNotFoundException("Work item not found or insufficient permissions");
        }

        await _workItemRepository.SoftDeleteWorkItemAsync(workItem);
    }
}