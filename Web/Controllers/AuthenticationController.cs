


namespace Application.Controllers;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController(IIdentityService identityService) : ControllerBase
{
    private readonly IIdentityService _identityService = identityService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var response = await _identityService.RegisterAsync(request);
            return response.Success ? Ok(response) : Conflict(response);
        }
        catch(CustomException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the request: {ex.Message}");
        }
    }

    [HttpPost("register-user")]
    public async Task<IActionResult> RegisterUser(RegistrationRequest request)
    {
        try
        {
            var response = await _identityService.RegisterUserAsync(request);
            return response.Success ? Ok(response) : Conflict(response);
        }
        catch (CustomException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the request: {ex.Message}");
        }
    }

    #region CreateAdmin 
    [HttpPost("create-admin")]
    [Authorize(Roles = "SuperAdmin")]

    public async Task<IActionResult> CreateAdmin(RegistrationRequest request)
    {
        try
        {
            var response = await _identityService.RegisterAdminAsync(request);
            return response.Success ? Ok(response) : Conflict(response);
        }
        catch (CustomException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the request: {ex.Message}");
        }
    }

    #endregion


    [HttpPost("create-super-admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateSuperAdmin(RegistrationRequest request)
    {
        try
        {
            var response = await _identityService.RegisterSuperAdminAsync(request);
            return response.Success ? Ok(response) : Conflict(response);
        }
        catch (CustomException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the request: {ex.Message}");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var response = await _identityService.LoginAsync(request);
            return response.Success ? Ok(response) : Unauthorized(response);
        }
        catch (CustomException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the request: {ex.Message}");
        }
    }


    [HttpPatch("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        try
        {
            var response =  await _identityService.ChangePasswordAsync(request);
            return Ok(response);
        }
        catch (CustomException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the request: {ex.Message}");
        }
    }


    [HttpDelete("logout")]
    public async Task<IActionResult> Logout(LoginRequest request)
    {
        try
        {
            await _identityService.LogoutAsync(request);
            return NoContent();
        }
        catch (CustomException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the request: {ex.Message}");
        }
    }

    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount(LoginRequest request)
    {
        try
        {
            await _identityService.DeleteAccountAsync(request);
            return NoContent();
        }
        catch (CustomException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while processing the request: {ex.Message}");
        }
    }
}
