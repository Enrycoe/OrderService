namespace OrderService.Application.DTOs.Auth;

public record TokenResult(string Token, DateTime ExpiresAt);
