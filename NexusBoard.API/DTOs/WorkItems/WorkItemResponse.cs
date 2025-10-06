using NexusBoard.Core.Entities;

namespace NexusBoard.API.DTOs.WorkItems;

public class WorkItemListResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public WorkItemAssigneeDto? Assignee { get; set; }
    public WorkItemCreatorDto Creator { get; set; } = null!;
    public int FileCount { get; set; }
}

public class WorkItemDetailResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public WorkItemProjectDto Project { get; set; } = null!;
    public WorkItemAssigneeDto? Assignee { get; set; }
    public WorkItemCreatorDetailDto Creator { get; set; } = null!;
    public List<WorkItemFileDto> Files { get; set; } = new();
}

public class WorkItemAssigneeDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class WorkItemCreatorDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class WorkItemCreatorDetailDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class WorkItemProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public WorkItemTeamDto Team { get; set; } = null!;
}

public class WorkItemTeamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WorkItemFileDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class CreateWorkItemResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public WorkItemProjectSimpleDto Project { get; set; } = null!;
    public WorkItemAssigneeSimpleDto? Assignee { get; set; }
    public WorkItemCreatorDto Creator { get; set; } = null!;
}

public class WorkItemProjectSimpleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WorkItemAssigneeSimpleDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}