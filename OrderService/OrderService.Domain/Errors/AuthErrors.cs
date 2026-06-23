using OrderService.Domain.Models;

namespace OrderService.Domain.Errors;

public static class AuthErrors
{
    public static readonly Error Unauthorized = Error.Unauthorized("Auth.Unauthorized", "Credenciais inválidas.");
    public static readonly Error UsernameInUse = Error.Conflict("Auth.UsernameInUse", "Username já cadastrado.");
}
