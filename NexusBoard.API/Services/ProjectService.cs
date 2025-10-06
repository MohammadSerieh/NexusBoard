using NexusBoard.API.DTOs.Projects;
using NexusBoard.API.Interfaces.IRepositories;
using NexusBoard.API.Interfaces.IServices;
using NexusBoard.Core.Entities;

namespace NexusBoard.API.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<List<ProjectListResponse>> GetMyProjectsAsync(Guid userId)
    {
        var projects = await _projectRepository.GetUserProjectsWithDetailsAsync(userId);

        return projects.Select(p => new ProjectListResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Status = p.Status,
            Priority = p.Priority,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            CreatedAt = p.CreatedAt,
            Team = new ProjectTeamDto
            {
                Id = p.Team.Id,
                Name = p.Team.Name
            },
            Creator = new ProjectCreatorDto
            {
                Id = p.Creator.Id,
                FirstName = p.Creator.FirstName,
                LastName = p.Creator.LastName
            },
            TaskCounts = new TaskCountsDto
            {
                Total = p.Tasks.Count(t => t.IsActive),
                Todo = p.Tasks.Count(t => t.IsActive && t.Status == WorkItemStatus.Todo),
                InProgress = p.Tasks.Count(t => t.IsActive && t.Status == WorkItemStatus.InProgress),
                Review = p.Tasks.Count(t => t.IsActive && t.Status == WorkItemStatus.Review),
                Done = p.Tasks.Count(t => t.IsActive && t.Status == WorkItemStatus.Done)
            }
        }).ToList();
    }

    public async Task<ProjectDetailResponse?> GetProjectAsync(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetProjectWithDetailsAsync(projectId, userId);

        if (project == null)
        {
            return null;
        }

        return new ProjectDetailResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            Priority = project.Priority,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            CreatedAt = project.CreatedAt,
            Team = new ProjectTeamDetailDto
            {
                Id = project.Team.Id,
                Name = project.Team.Name,
                Description = project.Team.Description,
                Members = project.Team.Members
                    .Where(m => m.IsActive)
                    .Select(m => new ProjectTeamMemberDto
                    {
                        Id = m.User.Id,
                        FirstName = m.User.FirstName,
                        LastName = m.User.LastName,
                        Email = m.User.Email,
                        Role = m.Role.ToString()
                    })
                    .ToList()
            },
            Creator = new ProjectCreatorDetailDto
            {
                Id = project.Creator.Id,
                FirstName = project.Creator.FirstName,
                LastName = project.Creator.LastName,
                Email = project.Creator.Email
            },
            Tasks = project.Tasks
                .Where(t => t.IsActive)
                .Select(t => new ProjectTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    Assignee = t.Assignee != null ? new ProjectTaskAssigneeDto
                    {
                        Id = t.Assignee.Id,
                        FirstName = t.Assignee.FirstName,
                        LastName = t.Assignee.LastName
                    } : null
                })
                .OrderBy(t => t.Status)
                .ThenByDescending(t => t.Priority)
                .ToList()
        };
    }

    public async Task<CreateProjectResponse> CreateProjectAsync(
        CreateProjectRequest request, 
        Guid userId)
    {
        // Verify user is member of the team
        if (!await _projectRepository.IsUserTeamMemberAsync(userId, request.TeamId))
        {
            throw new UnauthorizedAccessException("You must be a team member to create projects");
        }

        // Create project entity
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            TeamId = request.TeamId,
            CreatedBy = userId,
            Status = request.Status,
            Priority = request.Priority,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        // Save project
        project = await _projectRepository.CreateProjectAsync(project);

        // Reload with relations
        var createdProject = await _projectRepository.GetProjectWithCreatorAndTeamAsync(project.Id);

        if (createdProject == null)
        {
            throw new InvalidOperationException("Failed to retrieve created project");
        }

        // Map to response DTO
        return new CreateProjectResponse
        {
            Id = createdProject.Id,
            Name = createdProject.Name,
            Description = createdProject.Description,
            Status = createdProject.Status,
            Priority = createdProject.Priority,
            StartDate = createdProject.StartDate,
            EndDate = createdProject.EndDate,
            CreatedAt = createdProject.CreatedAt,
            Team = new ProjectTeamDto
            {
                Id = createdProject.Team.Id,
                Name = createdProject.Team.Name
            },
            Creator = new ProjectCreatorDto
            {
                Id = createdProject.Creator.Id,
                FirstName = createdProject.Creator.FirstName,
                LastName = createdProject.Creator.LastName
            }
        };
    }

    public async Task UpdateProjectAsync(
        Guid projectId, 
        UpdateProjectRequest request, 
        Guid userId)
    {
        var project = await _projectRepository.GetProjectForUpdateAsync(projectId, userId);

        if (project == null)
        {
            throw new KeyNotFoundException("Project not found or access denied");
        }

        // Update fields
        project.Name = request.Name;
        project.Description = request.Description;
        project.Status = request.Status;
        project.Priority = request.Priority;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;

        await _projectRepository.UpdateProjectAsync(project);
    }

    public async Task DeleteProjectAsync(Guid projectId, Guid userId)
    {
        var project = await _projectRepository.GetProjectForDeleteAsync(projectId, userId);

        if (project == null)
        {
            throw new KeyNotFoundException("Project not found or insufficient permissions");
        }

        await _projectRepository.SoftDeleteProjectAsync(project);
    }
}