using NexusBoard.API.DTOs.Auth;

namespace NexusBoard.API.Interfaces.IServices;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}