using System.ComponentModel.DataAnnotations;

namespace NexusBoard.Core.Entities;

public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public Guid CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties - these connect to other models
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<TeamMember> Members { get; set; } = new List<TeamMember>(); // "virtual" tells EF Core to use lazy loading, and tells Entity Framework this is a relationship 
    public virtual ICollection<Project> Projects { get; set; } = new List<Project>(); // ICollection is a list that can grow and shrink
}