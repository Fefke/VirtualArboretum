namespace VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;

/// <summary>
/// Represents a structured error outcome, identified by an error code enum.
/// </summary>
/// <typeparam name="TErrorCodeEnum">The enum type used for error codes. Must be any Enum indicating your possible errors.</typeparam>
public record ErrorResult<TErrorCodeEnum> where TErrorCodeEnum : Enum
{
    /// <summary>
    /// specific error code enum value to 
    /// </summary>
    public TErrorCodeEnum Code { get; }

    /// <summary>
    /// human-readable error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets an optional identifier for the target of the error (e.g., a field name).
    /// </summary>
    public string? Target { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorResult{TErrorCodeEnum}"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="target">Optional target identifier (e.g., field name).</param>
    public ErrorResult(TErrorCodeEnum code, string message, string? target = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                @"Error message cannot be null or whitespace.", nameof(message)
                );
        }

        Code = code;
        Message = message;
        Target = target;
    }
}