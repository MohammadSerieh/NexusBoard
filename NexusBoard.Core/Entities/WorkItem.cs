using System.ComponentModel.DataAnnotations;

namespace NexusBoard.Core.Entities;

public class WorkItem  // Changed from Task to WorkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public WorkItemStatus Status { get; set; } = WorkItemStatus.Todo;  // Updated enum name
    
    [Required]
    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;  // Updated enum name
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User? Assignee { get; set; }
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<TaskFile> Files { get; set; } = new List<TaskFile>();
}

public enum WorkItemStatus  // Changed from TaskStatus
{
    Todo = 1,
    InProgress = 2,
    Review = 3,
    Done = 4
}

public enum WorkItemPriority  // Changed from TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}