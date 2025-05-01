using VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;

namespace VirtualArboretum.Core.Application.DataTransferObjects;

/// <summary>
/// Static factory for simple Result object creation (with less verbosity). Use with 'using static'.
/// </summary>
public abstract class ResultFactory
{
    /// <summary>
    /// Creates a success Result. TSuccess type is often inferred.
    /// </summary>
    public static Result<TSuccess, TErrorEnum> Ok<TSuccess, TErrorEnum>(TSuccess value)
        where TErrorEnum : Enum
        => Result<TSuccess, TErrorEnum>.Ok(value);

    /// <summary>
    /// Creates a failure Result with a new ErrorResult. Types often inferred from context.
    /// </summary>
    public static Result<TSuccess, TErrorEnum> Fail<TSuccess, TErrorEnum>(
        TErrorEnum code, string message = "", string? target = null
        ) where TErrorEnum : Enum
        => Result<TSuccess, TErrorEnum>.Fail(new ErrorResult<TErrorEnum>(code, message, target));

    /// <summary>
    /// Creates a failure Result from an existing ErrorResult. TSuccess usually needs explicit specification.
    /// </summary>
    public static Result<TSuccess, TErrorEnum> Fail<TSuccess, TErrorEnum>(
        ErrorResult<TErrorEnum> error
        ) where TErrorEnum : Enum
        => Result<TSuccess, TErrorEnum>.Fail(error);
}
