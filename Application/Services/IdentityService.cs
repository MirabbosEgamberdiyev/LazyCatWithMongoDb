using Application.Commens.Helpers;
using Application.Common.Helpers;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class IdentityService(UserManager<ApplicationUser> userManager,
                             IConfiguration configuration,
                             RoleManager<ApplicationRole> roleManager) : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;


    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return new LoginResponse { Message = "Invalid email/password", Success = false };

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
        catch (Exception ex)
        {
            return new LoginResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
                return new RegisterResponse { Message = "User already exists", Success = false };

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email
            };

            var createUserResult = await _userManager.CreateAsync(user, request.Password);
            if (!createUserResult.Succeeded)
                return new RegisterResponse { Message = $"Create user failed {createUserResult?.Errors?.First()?.Description}", Success = false };

            foreach (var role in request.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new ApplicationRole(role));
                    await _userManager.AddToRoleAsync(user, role);
            }

            return new RegisterResponse { Success = true, Message = "User registered successfully" };
        }
        catch (Exception ex)
        {
            return new RegisterResponse { Message = ex.Message, Success = false };
        }
    }
}
