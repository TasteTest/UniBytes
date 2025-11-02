namespace backend_payment.Common;

/// <summary>
/// Result pattern for operation outcomes
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    public List<string>? Errors { get; private set; }

    private Result(bool isSuccess, T? value, string? error, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Errors = errors;
    }

    public static Result<T> Success(T value) => new(true, value, null);

    public static Result<T> Failure(string error) => new(false, default, error);

    public static Result<T> Failure(List<string> errors) => new(false, default, null, errors);
}

