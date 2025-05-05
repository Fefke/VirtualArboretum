using System.Diagnostics.CodeAnalysis;
using VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;

namespace VirtualArboretum.Core.Application.DataTransferObjects;

/// <summary>
/// Represents the outcome of an operation, which can be either success or failure.
/// </summary>
/// <typeparam name="TSuccess">The type of the value returned on success.</typeparam>
/// <typeparam name="TError">The type of the error returned on failure.</typeparam>
public record Result<TSuccess, TError> where TError : Enum
{
    private readonly TSuccess? _value;
    private readonly ErrorResult<TError>? _error;

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the success value. Access only if IsSuccess is true,<br/>
    /// otherwise you get an InvalidOperationException thrown at your face (test for success).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if IsSuccess is false.</exception>
    public TSuccess Value => IsSuccess
        ? _value!
            : throw new InvalidOperationException("Result does not contain a success value.");

    /// <summary>
    /// Gets the error value. Access only if IsSuccess is false.<br/>
    /// otherwise you get an InvalidOperationException thrown at your face (test for success).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if IsSuccess is true.</exception>
    public ErrorResult<TError> Error =>
        !IsSuccess ? _error!
            : throw new InvalidOperationException("Result does not contain an error value.");


    /// <summary>
    /// Creates a success result with the specified value.
    /// </summary>
    public static Result<TSuccess, TError> Ok(TSuccess value)
    {
        // Optional: Add null check if TSuccess is a reference type and null is not a valid success value
        // if (value is null) throw new ArgumentNullException(nameof(value), "Success value cannot be null.");
        return new Result<TSuccess, TError>(value);
    }

    /// <summary>
    /// Creates a failure result with the specified error.
    /// </summary>
    public static Result<TSuccess, TError> Fail(ErrorResult<TError> error)
    {
        // Optional: Add null check if TError is a reference type and null is not a valid error value
        // if (error is null) throw new ArgumentNullException(nameof(error), "Error value cannot be null.");
        return new Result<TSuccess, TError>(error);
    }

    // Private constructors
    private Result(TSuccess value)
    {
        IsSuccess = true;
        _value = value;
        _error = default;
    }

    private Result(ErrorResult<TError> error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    /// <summary>
    /// Executes one of the provided functions based on the result state.
    /// </summary>
    /// <typeparam name="TResult">The return type of the functions.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<TSuccess, TResult> onSuccess, Func<ErrorResult<TError>, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }
    
}