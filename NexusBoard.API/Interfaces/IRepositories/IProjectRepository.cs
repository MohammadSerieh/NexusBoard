using NexusBoard.Core.Entities;

namespace NexusBoard.API.Interfaces.IRepositories;

public interface IProjectRepository
{
    Task<List<Project>> GetUserProjectsWithDetailsAsync(Guid userId);
    Task<Project?> GetProjectWithDetailsAsync(Guid projectId, Guid userId);
    Task<Project> CreateProjectAsync(Project project);
    Task<Project?> GetProjectWithCreatorAndTeamAsync(Guid projectId);
    Task<Project?> GetProjectForUpdateAsync(Guid projectId, Guid userId);
    Task UpdateProjectAsync(Project project);
    Task<Project?> GetProjectForDeleteAsync(Guid projectId, Guid userId);
    Task SoftDeleteProjectAsync(Project project);
    Task<bool> IsUserTeamMemberAsync(Guid userId, Guid teamId);
}