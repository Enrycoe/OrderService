using OrderService.Application.DTOs.Auth;
using OrderService.Domain.Models;

namespace OrderService.Application.Abstractions;

public interface IAuthService
{
    Task<Result> RegisterAsync(RegisterUserDto registerUserDto, CancellationToken ct);
    Task<Result<TokenResult>> LoginAsync(LoginDto loginDto, CancellationToken ct);
}
