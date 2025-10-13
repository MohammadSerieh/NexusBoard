using NexusBoard.API.DTOs.Teams;

namespace NexusBoard.API.Interfaces.IServices;

public interface ITeamService
{
    Task<List<TeamResponse>> GetMyTeamsAsync(Guid userId);
    Task<CreateTeamResponse> CreateTeamAsync(CreateTeamRequest request, Guid userId);
    Task<AddMemberResponse> AddTeamMemberAsync(Guid teamId, AddTeamMemberRequest request, Guid userId);
    Task RemoveTeamMemberAsync(Guid teamId, Guid memberId, Guid userId); // No data returned just success/failure
    Task DeleteTeamAsync(Guid teamId, Guid userId);
    Task<List<TeamMemberDto>> GetTeamMembersAsync(Guid teamId);
    Task<bool> IsUserTeamMemberAsync(Guid userId, Guid teamId);
}