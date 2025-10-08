using NexusBoard.API.DTOs.Teams;
using NexusBoard.API.Interfaces.IRepositories;
using NexusBoard.API.Interfaces.IServices;
using NexusBoard.Core.Entities;

namespace NexusBoard.API.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _teamRepository;

    public TeamService(ITeamRepository teamRepository)
    {
        _teamRepository = teamRepository;
    }

    public async Task<List<TeamResponse>> GetMyTeamsAsync(Guid userId)
    {
        var teams = await _teamRepository.GetUserTeamsWithDetailsAsync(userId);

        return teams.Select(team => new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            Description = team.Description,
            CreatedAt = team.CreatedAt,
            Creator = new TeamCreatorDto
            {
                Id = team.Creator.Id,
                FirstName = team.Creator.FirstName,
                LastName = team.Creator.LastName,
                Email = team.Creator.Email
            },
            MyRole = team.Members
                .First(m => m.UserId == userId && m.IsActive)
                .Role.ToString(),
            MemberCount = team.Members.Count(m => m.IsActive),
            Members = team.Members
                .Where(m => m.IsActive)
                .Take(5)
                .Select(m => new TeamMemberDto
                {
                    Id = m.User.Id,
                    FirstName = m.User.FirstName,
                    LastName = m.User.LastName,
                    Email = m.User.Email,
                    Role = m.Role.ToString()
                })
                .ToList()
        }).ToList();
    }

    public async Task<CreateTeamResponse> CreateTeamAsync(CreateTeamRequest request, Guid userId)
    {
        // Create team entity
        var team = new Team
        {
            Name = request.Name,
            Description = request.Description,
            CreatedBy = userId
        };

        // Save team
        team = await _teamRepository.CreateTeamAsync(team);

        // Add creator as team lead
        var teamMember = new TeamMember
        {
            TeamId = team.Id,
            UserId = userId,
            Role = TeamRole.TeamLead
        };

        await _teamRepository.AddTeamMemberAsync(teamMember);

        // Reload team with creator
        var createdTeam = await _teamRepository.GetTeamWithCreatorAsync(team.Id);

        if (createdTeam == null)
        {
            throw new InvalidOperationException("Failed to retrieve created team");
        }

        // Map to response DTO
        return new CreateTeamResponse
        {
            Id = createdTeam.Id,
            Name = createdTeam.Name,
            Description = createdTeam.Description,
            CreatedAt = createdTeam.CreatedAt,
            Creator = new TeamCreatorDto
            {
                Id = createdTeam.Creator.Id,
                FirstName = createdTeam.Creator.FirstName,
                LastName = createdTeam.Creator.LastName,
                Email = createdTeam.Creator.Email
            },
            MemberCount = 1
        };
    }

    public async Task<AddMemberResponse> AddTeamMemberAsync(
        Guid teamId,
        AddTeamMemberRequest request,
        Guid userId)
    {
        // Check if current user is team lead
        if (!await _teamRepository.IsUserTeamLeadAsync(userId, teamId))
        {
            throw new UnauthorizedAccessException("Only team leads can add members");
        }

        // Check if user exists
        var userToAdd = await _teamRepository.GetUserByEmailAsync(request.Email);
        if (userToAdd == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if user is already a member
        var existingMember = await _teamRepository.GetTeamMemberAsync(teamId, userToAdd.Id);

        if (existingMember != null)
        {
            if (existingMember.IsActive)
            {
                throw new InvalidOperationException("User is already a team member");
            }
            else
            {
                // Reactivate existing member
                existingMember.IsActive = true;
                existingMember.Role = TeamRole.Member;
                existingMember.JoinedAt = DateTime.UtcNow;
                await _teamRepository.UpdateTeamMemberAsync(existingMember);
            }
        }
        else
        {
            // Add new team member
            var teamMember = new TeamMember
            {
                TeamId = teamId,
                UserId = userToAdd.Id,
                Role = TeamRole.Member
            };

            await _teamRepository.AddTeamMemberAsync(teamMember);
        }

        // Map to response DTO
        return new AddMemberResponse
        {
            Id = userToAdd.Id,
            FirstName = userToAdd.FirstName,
            LastName = userToAdd.LastName,
            Email = userToAdd.Email,
            Role = TeamRole.Member.ToString(),
            JoinedAt = DateTime.UtcNow
        };
    }

    public async Task RemoveTeamMemberAsync(Guid teamId, Guid memberId, Guid userId)
    {
        if (!await _teamRepository.IsUserTeamLeadAsync(userId, teamId))
        {
            throw new UnauthorizedAccessException("Only team leads can remove members");
        }

        // Prevent team lead from remving themselves
        if (memberId == userId)
        {
            throw new InvalidProgramException("Team leads cannot remove themselves from the team");
        }

        var teamMember = await _teamRepository.GetTeamMemberAsync(teamId, memberId);

        if (teamMember == null || !teamMember.IsActive)
        {
            throw new InvalidOperationException("Team member not found");
        }

        if (teamMember.Role == TeamRole.TeamLead)
        {
            throw new InvalidOperationException("Cannot remove a team lead");
        }

        teamMember.IsActive = false;
        await _teamRepository.UpdateTeamMemberAsync(teamMember);

    }
    
    
}