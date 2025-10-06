using System.ComponentModel.DataAnnotations;

namespace NexusBoard.API.DTOs.Teams;

public class AddTeamMemberRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}