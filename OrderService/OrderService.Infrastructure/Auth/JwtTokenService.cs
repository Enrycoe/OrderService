using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderService.Application.Abstractions;
using OrderService.Application.DTOs.Auth;
using OrderService.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime;
using System.Security.Claims;
using System.Text;

namespace OrderService.Infrastructure.Auth;

public class JwtTokenService(IOptions<JwtSettings> settings) : ITokenService
{
    public TokenResult GenerateToken(User user)
    {
        var setting = settings.Value;

        var key = new SymmetricSecurityKey(
           Encoding.UTF8.GetBytes(setting.Key));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(setting.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: setting.Issuer,
            audience: setting.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new TokenResult(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt: expiresAt
        );
    }
}
