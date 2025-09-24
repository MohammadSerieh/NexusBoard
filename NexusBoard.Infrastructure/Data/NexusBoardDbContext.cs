using Microsoft.EntityFrameworkCore;
using NexusBoard.Core.Entities;
using TaskEntity = NexusBoard.Core.Entities.WorkItem;
namespace NexusBoard.Infrastructure.Data;

public class NexusBoardDbContext : DbContext
{
    public NexusBoardDbContext(DbContextOptions<NexusBoardDbContext> options) : base(options)
    {
    }
    
    // These represent tables in our database
    public DbSet<User> Users { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<WorkItem> WorkItems { get; set; }  // Changed from Task to WorkItem
    public DbSet<TaskFile> TaskFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique(); // Email must be unique
            entity.Property(e => e.Role).HasConversion<int>(); // Store enum as number
        });

        // Configure TeamMember (many-to-many relationship)
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => new { e.TeamId, e.UserId }); // Composite key
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Members)
                  .HasForeignKey(e => e.TeamId);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.TeamMemberships)
                  .HasForeignKey(e => e.UserId);
            entity.Property(e => e.Role).HasConversion<int>();
        });

        // Configure Team relationships
        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasOne(e => e.Creator)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict); // Don't delete creator if team is deleted
        });

        // Configure Project relationships
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasOne(e => e.Team)
                  .WithMany(t => t.Projects)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.Cascade); // Delete projects if team is deleted
            entity.HasOne(e => e.Creator)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Priority).HasConversion<int>();
        });

        // Configure WorkItem relationships
        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Tasks)  // Project has many Tasks (property name)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade); // Delete work items if project is deleted
            entity.HasOne(e => e.Assignee)
                  .WithMany(u => u.AssignedTasks)  // User has many AssignedTasks
                  .HasForeignKey(e => e.AssigneeId)
                  .OnDelete(DeleteBehavior.SetNull); // Unassign work item if user is deleted
            entity.HasOne(e => e.Creator)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.Priority).HasConversion<int>();
        });

        
        // Configure TaskFile relationships
        modelBuilder.Entity<TaskFile>(entity =>
        {
            entity.HasOne<WorkItem>("Task")  // Explicitly specify WorkItem type
                .WithMany(workItem => workItem.Files)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Uploader)
                .WithMany()
                .HasForeignKey(e => e.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}