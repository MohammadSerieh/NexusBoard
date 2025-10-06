using System.ComponentModel.DataAnnotations;
using NexusBoard.Core.Entities;

namespace NexusBoard.API.DTOs.Projects;

public class UpdateProjectRequest
{
    [Required(ErrorMessage = "Project name is required")]
    [MaxLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; }

    public ProjectPriority Priority { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}