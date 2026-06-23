using OrderService.Application.Abstractions;
using OrderService.Application.DTOs.Auth;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Errors;
using OrderService.Domain.Models;
using System.Security.Cryptography;
using System.Text;

namespace OrderService.Application.Services;

public class AuthService(IUnitOfWork unitOfWork, ITokenService tokenService) : IAuthService
{
    public async Task<Result<TokenResult>> LoginAsync(LoginDto loginDto, CancellationToken ct)
    {
        var user = await unitOfWork.UserRepository.GetByUsernameAsync(loginDto.Username, ct);
        if (user is null || !VerifyHash(loginDto.Password, user.PasswordHash))
            return Result<TokenResult>.Failure(AuthErrors.Unauthorized);

        return Result.Success(tokenService.GenerateToken(user));
    }

    public async Task<Result> RegisterAsync(RegisterUserDto registerUserDto, CancellationToken ct)
    {
        if (await unitOfWork.UserRepository.GetByUsernameAsync(registerUserDto.Username, ct) is not null)
            return Result.Failure(AuthErrors.UsernameInUse);

        var user = new User()
        {
            Username = registerUserDto.Username,
            PasswordHash = Hash(registerUserDto.Password)
        };

        await unitOfWork.UserRepository.AddAsync(user, ct);

        return Result.Success();
    }

    private static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static bool VerifyHash(string password, string hash)
        => Hash(password) == hash;
}
