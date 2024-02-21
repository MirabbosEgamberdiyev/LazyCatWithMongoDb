
namespace Application.Interfaces;

public interface IIdentityService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task DeleteAccountAsync(LoginRequest request);
    Task LogoutAsync(LoginRequest request);
    Task<LoginResponse> ChangePasswordAsync(ChangePasswordRequest dto);
    Task<RegisterResponse> RegisterUserAsync(RegistrationRequest request);

    Task<RegisterResponse> RegisterAdminAsync(RegistrationRequest request);
    Task<RegisterResponse> RegisterSuperAdminAsync(RegistrationRequest request);
}
