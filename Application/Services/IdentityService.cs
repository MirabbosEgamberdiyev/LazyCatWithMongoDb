
using Application.Commens.Constants;

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

    public async Task<LoginResponse> ChangePasswordAsync(ChangePasswordRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new NotFoundException("User not found");

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
                throw new ValidationException("Failed to change password");

            var roles = await _userManager.GetRolesAsync(user);
            var token = JwtHelper.GenerateJwtToken(user, roles, _configuration);

            // Generate the new token before removing the old one
            await _userManager.RemoveAuthenticationTokenAsync(user, _configuration["Jwt:Issuer"] ?? "", "Token");

            return new LoginResponse
            {
                AccessToken = token,
                Message = "Password changed successfully",
                Email = user.Email,
                Success = true,
                UserId = user.Id.ToString()
            };
        }
        catch (NotFoundException ex)
        {
            return new LoginResponse { Success = false, Message = ex.Message };
        }
        catch (ValidationException ex)
        {
            return new LoginResponse { Success = false, Message = ex.Message };
        }
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, Message = $"An error occurred while changing password: {ex.Message}" };
        }
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

    public async Task<RegisterResponse> RegisterUserAsync(RegistrationRequest request)
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

        
            if (!await _roleManager.RoleExistsAsync(IdentityRoles.USER))
                await _roleManager.CreateAsync(new ApplicationRole(IdentityRoles.USER));

            await _userManager.AddToRoleAsync(user, IdentityRoles.USER);
            

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

    public async Task<RegisterResponse> RegisterAdminAsync(RegistrationRequest request)
    {
        try
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
                throw new CustomException("Admin already exists");

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email
            };

            var createUserResult = await _userManager.CreateAsync(user, request.Password);
            if (!createUserResult.Succeeded)
                throw new ValidationException($"Create admin failed {createUserResult?.Errors?.First()?.Description}");


            if (!await _roleManager.RoleExistsAsync(IdentityRoles.ADMIN))
                await _roleManager.CreateAsync(new ApplicationRole(IdentityRoles.ADMIN));

            await _userManager.AddToRoleAsync(user, IdentityRoles.ADMIN);


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

    public async Task<RegisterResponse> RegisterSuperAdminAsync(RegistrationRequest request)
    {
        try
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
                throw new CustomException("SuperAdmin already exists");

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email
            };

            var createUserResult = await _userManager.CreateAsync(user, request.Password);
            if (!createUserResult.Succeeded)
                throw new ValidationException($"Create SuperAdmin failed {createUserResult?.Errors?.First()?.Description}");


            if (!await _roleManager.RoleExistsAsync(IdentityRoles.SUPER_ADMIN))
                await _roleManager.CreateAsync(new ApplicationRole(IdentityRoles.SUPER_ADMIN));

            await _userManager.AddToRoleAsync(user, IdentityRoles.SUPER_ADMIN);


            return new RegisterResponse { Success = true, Message = "SuperAdmin registered successfully" };
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
}
