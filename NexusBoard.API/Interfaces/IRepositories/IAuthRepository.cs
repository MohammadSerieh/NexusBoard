using NexusBoard.Core.Entities;

namespace NexusBoard.API.Interfaces.IRepositories;

public interface IAuthRepository
{
    Task<bool> UserExistsByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task UpdateUserLastLoginAsync(Guid userId);
}