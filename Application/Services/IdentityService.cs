
namespace Application.Services;

public class IdentityService (UserManager<ApplicationUser> userManager,
                              IConfiguration configuration,
                              RoleManager<ApplicationRole> roleManager) : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
                throw new CustomException("User already exists");

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email
            };

            var createUserResult = await _userManager.CreateAsync(user, request.Password);
            if (!createUserResult.Succeeded)
                throw new ValidationException($"Create user failed {createUserResult?.Errors?.First()?.Description}");

            foreach (var role in request.Roles)
            {
                if (!IsRoleValid(role))
                {
                    await _userManager.DeleteAsync(user);
                    throw new ValidationException($"Invalid role: {role}");
                }

                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new ApplicationRole(role));

                await _userManager.AddToRoleAsync(user, role);
            }

            return new RegisterResponse { Success = true, Message = "User registered successfully" };
        }
        catch (CustomException ex)
        {
            return new RegisterResponse { Success = false, Message = ex.Message };
        }
        catch (ValidationException ex)
        {
            return new RegisterResponse { Success = false, Message = ex.Message };
        }
        catch (Exception ex)
        {
            return new RegisterResponse { Success = false, Message = ex.Message };
        }
    }


    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new CustomException("Invalid email/password");

        // Check password validity
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            throw new CustomException("Invalid email/password");

        var roles = await _userManager.GetRolesAsync(user);
        var token = JwtHelper.GenerateJwtToken(user, roles, _configuration);

        return new LoginResponse
        {
            AccessToken = token,
            Message = "Login Successful",
            Email = user.Email,
            Success = true,
            UserId = user.Id.ToString()
        };
    }

    public async Task LogoutAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new NotFoundException("User not found");
        // Check password validity
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            throw new CustomException("Invalid email/password");

        await _userManager.RemoveAuthenticationTokenAsync(user, _configuration["Jwt:Issuer"] ?? "", "Token");
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new NotFoundException("User not found");

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
        await _userManager.RemoveAuthenticationTokenAsync(user, _configuration["Jwt:Issuer"] ?? "", "Token");

        if (!result.Succeeded)
            throw new ValidationException("Failed to change password");
    }

    public async Task DeleteAccountAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new NotFoundException("User not found");

        // Check password validity
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            throw new CustomException("Invalid email/password");

        await _userManager.RemoveAuthenticationTokenAsync(user, _configuration["Jwt:Issuer"] ?? "", "Token");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new ValidationException("Failed to delete user");
    }


    private bool IsRoleValid(string role)
    {
        return role switch
        {
            "Admin" => true,
            "User" => true,
            "SuperAdmin" => true,
            _ => false
        };
    }
}
