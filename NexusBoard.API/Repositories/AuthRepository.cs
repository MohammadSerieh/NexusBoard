using Microsoft.EntityFrameworkCore;
using NexusBoard.API.Interfaces.IRepositories;
using NexusBoard.Core.Entities;
using NexusBoard.Infrastructure.Data;

namespace NexusBoard.API.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly NexusBoardDbContext _context;

    public AuthRepository(NexusBoardDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
    }

    public async Task UpdateUserLastLoginAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}