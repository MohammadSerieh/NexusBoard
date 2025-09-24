using System.ComponentModel.DataAnnotations;

namespace NexusBoard.Core.Entities;

public class TeamMember
{
    // this model links Users and Teams in a many-to-many relationship
    // Stores role within team: John might be TeamLead in Marketing but Member in Development
    // No separate Id - we use composite key (TeamId + UserId)
    public Guid TeamId { get; set; }
    public Guid UserId { get; set; }
    
    [Required]
    public TeamRole Role { get; set; } = TeamRole.Member;
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties - connect to the actual objects
    public virtual Team Team { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public enum TeamRole
{
    TeamLead = 1,
    Member = 2
}