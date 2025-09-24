using System.ComponentModel.DataAnnotations;

namespace NexusBoard.Core.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    
    [Required]
    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Which team owns this project
    public Guid TeamId { get; set; }
    
    // Who created this project
    public Guid CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual Team Team { get; set; } = null!;
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<WorkItem> Tasks { get; set; } = new List<WorkItem>();
}

public enum ProjectStatus
{
    Planning = 1,
    Active = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}

public enum ProjectPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}