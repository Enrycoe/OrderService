namespace OrderService.Domain.Models;

public class Result
{
    protected Result() { }
    protected Result(Error error)
    {
        Errors.Add(error);
    }
    protected Result(List<Error> errors)
    {
        Errors.AddRange(errors);
    }

    public bool IsSuccess => Errors is { Count: 0 };

    public bool IsFailure => !IsSuccess;
    public List<Error> Errors { get; set; } = [];

    public static Result Success() => new();
    public static Result<T> Success<T>(T data) => Result<T>.Success(data);
    public static Result Failure(Error error) => new(error);
    public static Result Failure(List<Error> errors) => new(errors);
}

public class Result<T> : Result
{
    private Result(T data) : base()
    {
        Data = data;
    }
    private Result(Error error) : base(error)
    {
    }
    private Result(List<Error> errors) : base(errors)
    { }

    public static Result<T> Success(T data) => new(data);
    public static new Result<T> Failure(Error error) => new(error);
    public static new Result<T> Failure(List<Error> errors) => new(errors);

    public T? Data { get; set; }
}