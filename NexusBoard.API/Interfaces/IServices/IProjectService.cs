using NexusBoard.API.DTOs.Projects;

namespace NexusBoard.API.Interfaces.IServices;

public interface IProjectService
{
    Task<List<ProjectListResponse>> GetMyProjectsAsync(Guid userId);
    Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, Guid userId);
    Task<CreateProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid userId);
    Task UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, Guid userId);
    Task DeleteProjectAsync(Guid projectId, Guid userId);
}