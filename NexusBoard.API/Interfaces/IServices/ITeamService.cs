using NexusBoard.API.DTOs.Teams;

namespace NexusBoard.API.Interfaces.IServices;

public interface ITeamService
{
    Task<List<TeamResponse>> GetMyTeamsAsync(Guid userId);
    Task<CreateTeamResponse> CreateTeamAsync(CreateTeamRequest request, Guid userId);
    Task<AddMemberResponse> AddTeamMemberAsync(Guid teamId, AddTeamMemberRequest request, Guid userId);
}