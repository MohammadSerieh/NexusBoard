using System.ComponentModel.DataAnnotations;
using NexusBoard.Core.Entities;

namespace NexusBoard.API.DTOs.WorkItems;

public class UpdateWorkItemRequest
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(300, ErrorMessage = "Title cannot exceed 300 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000, ErrorMessage = "Description cannot exceed 5000 characters")]
    public string Description { get; set; } = string.Empty;

    public WorkItemStatus Status { get; set; }

    public WorkItemPriority Priority { get; set; }

    public Guid? AssigneeId { get; set; }

    public DateTime? DueDate { get; set; }
}