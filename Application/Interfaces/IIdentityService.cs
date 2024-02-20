using Application.Dtos;

namespace Application.Interfaces;

public interface IIdentityService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
}
