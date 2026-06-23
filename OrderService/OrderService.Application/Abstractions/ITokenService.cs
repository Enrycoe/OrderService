using OrderService.Application.DTOs.Auth;
using OrderService.Domain.Entities;

namespace OrderService.Application.Abstractions;

public interface ITokenService
{
    TokenResult GenerateToken(User user);
}
