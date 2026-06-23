using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Models;
using OrderService.Domain.Models;

namespace OrderService.Api.Extensions;

public static class ResultExtensions
{
    extension(Result result)
    {
        public IActionResult ToProblem()
        {
            var error = result.Errors.First();
            var statusCode = GetStatusCode(error);
            var problemDetails = new ApiProblemDetails
            {
                Title = GetErrorTitle(error),
                Type = $"https://httpstatuses.com/{statusCode}",
                Status = statusCode,
                Detail = error.Message,
                Code = error.Code,
                Error = error.Message
            };

            return new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };
        }

        public IActionResult Match(Func<Result, IActionResult> matchFunc)
        {
            return result.IsSuccess
                ? matchFunc(result)
                : result.ToProblem();
        }
    }

    extension<T>(Result<T> result)
    {
        public IActionResult Match(Func<T?, IActionResult> onSuccess)
        {
            return result.IsSuccess
                ? onSuccess(result.Data)
                : result.ToProblem();
        }
    }

    private static string GetErrorTitle(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => "Validation Error",
            ErrorType.Unauthorized => "Unauthorized Access",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.NotFound => "Resource Not Found",
            ErrorType.Conflict => "Conflict",
            _ => "An Error Occurred"
        };

    private static int GetStatusCode(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => 400,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            _ => 500
        };
}