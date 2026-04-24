using Library_Book_Borrowing_System.Dtos;
using Library_Book_Borrowing_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password)
        ) return BadRequest("All fields are required.");

        if (request.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters.");

        var (response, error) = await _authService.RegisterAsync(request);
        if (response is null) return Conflict(error);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password)
        ) return BadRequest("Email and password are required.");

        var response = await _authService.LoginAsync(request);
        if (response is null)
            return Unauthorized("Invalid email or password.");
            
        return Ok(response);
    }
}