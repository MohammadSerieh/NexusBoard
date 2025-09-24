using System.ComponentModel.DataAnnotations;

namespace NexusBoard.Core.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Member;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public virtual ICollection<WorkItem> AssignedTasks { get; set; } = new List<WorkItem>();  // Changed from Task to WorkItem
}



public enum UserRole
{
    Admin = 1, // Can do everything
    Manager = 2, // Can manage teams and projects 
    Member = 3 // Regular team member
}