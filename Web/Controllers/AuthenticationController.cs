using Application.Dtos;
using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Web.Controllers;

[ApiController]
[Route("api/authenticate")]
public class AuthenticationController(RoleManager<ApplicationRole> roleManager, 
                                      IIdentityService identityService) : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly IIdentityService _identityService = identityService;

    [HttpPost]
    [Route("roles/add")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var appRole = new ApplicationRole { Name = request.Role };
        var createRole = await _roleManager.CreateAsync(appRole);

        return Ok(new { message = "role created successfully" });
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _identityService.RegisterAsync(request);

        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    [HttpPost]
    [Route("login")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LoginResponse))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _identityService.LoginAsync(request);

        return result.Success ? Ok(result) : BadRequest(result.Message);
    }


}
