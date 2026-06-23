using Microsoft.AspNetCore.Mvc;

namespace OrderService.Api.Models;

public sealed class ApiProblemDetails : ProblemDetails
{
    public string Code { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}