using System.ComponentModel.DataAnnotations;
using NexusBoard.Core.Entities;

namespace NexusBoard.API.DTOs.Projects;

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Project name is required")]
    [MaxLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Team ID is required")]
    public Guid TeamId { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    public ProjectPriority Priority { get; set; } = ProjectPriority.Medium;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}