using NexusBoard.Core.Entities;

namespace NexusBoard.API.Interfaces.IRepositories;

public interface ITeamRepository
{
    Task<List<Team>> GetUserTeamsWithDetailsAsync(Guid userId);
    Task<Team> CreateTeamAsync(Team team);
    Task<Team?> GetTeamWithCreatorAsync(Guid teamId);
    Task<TeamMember> AddTeamMemberAsync(TeamMember teamMember);
    Task<bool> IsUserTeamLeadAsync(Guid userId, Guid teamId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<TeamMember?> GetTeamMemberAsync(Guid teamId, Guid userId);
    Task UpdateTeamMemberAsync(TeamMember teamMember);
    Task DeleteTeamAsync(Guid teamId);
}