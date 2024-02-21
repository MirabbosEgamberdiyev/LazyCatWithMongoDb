
namespace Application.Interfaces;

public interface IIdentityService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task DeleteAccountAsync(LoginRequest request);
    Task LogoutAsync(LoginRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest dto);
}
