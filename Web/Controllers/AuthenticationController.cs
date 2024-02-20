using System.Net;
using System.Threading.Tasks;
using Application.Dtos;
using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/authenticate")]
    public class AuthenticationController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IIdentityService _identityService;

        public AuthenticationController(RoleManager<ApplicationRole> roleManager,
                                        IIdentityService identityService)
        {
            _roleManager = roleManager;
            _identityService = identityService;
        }

        [HttpPost]
        [Route("roles/add")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var role = new ApplicationRole { Name = request.Role };
            var result = await _roleManager.CreateAsync(role);

            return Ok(new { message = "Role created successfully" });
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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _identityService.LoginAsync(request);

            return result.Success ? Ok(result) : BadRequest(result.Message);
        }
    }
}
