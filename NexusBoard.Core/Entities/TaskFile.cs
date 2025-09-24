using System.ComponentModel.DataAnnotations;

namespace NexusBoard.Core.Entities;

public class TaskFile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSizeBytes { get; set; }
    
    // Which task this file belongs to
    public Guid TaskId { get; set; }
    
    // Who uploaded this file
    public Guid UploadedBy { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual WorkItem Task { get; set; } = null!;
    public virtual User Uploader { get; set; } = null!;
}