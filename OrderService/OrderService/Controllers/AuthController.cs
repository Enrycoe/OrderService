using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Extensions;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs.Auth;

namespace OrderService.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync(LoginDto loginDto, CancellationToken ct)
    {
        var result = await authService.LoginAsync(loginDto, ct);
        return result.Match(Ok);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterAsync(RegisterUserDto registerDto, CancellationToken ct)
    {
        var result = await authService.RegisterAsync(registerDto, ct);
        return result.Match(x => Created());
    }
}
