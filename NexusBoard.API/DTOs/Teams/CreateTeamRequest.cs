using System.ComponentModel.DataAnnotations;

namespace NexusBoard.API.DTOs.Teams;

public class CreateTeamRequest
{
    [Required(ErrorMessage = "Team name is required")]
    [MaxLength(200, ErrorMessage = "Team name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;
}