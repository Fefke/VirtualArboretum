using VirtualArboretum.Core.Application.DataTransferObjects.ResultDTOs;
using VirtualArboretum.Core.Application.DataTransferObjects;

namespace VirtualArboretum.Core.Application.UseCases;

public abstract class AbstractUseCase<TSuccess, TError> where TError : Enum
{

    //public abstract Result<TSuccess, TError> Apply();
    //  => would require third type to be passed in for the Input DTO, but idc atm
    //     and want freedom to implement method-names as I see fit for specific use-case.

    protected static Result<TSuccess, TError> Ok(TSuccess success)
        => Result<TSuccess, TError>.Ok(success);

    protected static Result<TSuccess, TError> Fail(
        TError errorCode, string message = "", string? target = null
        ) => Result<TSuccess, TError>.Fail(new ErrorResult<TError>(errorCode, message, target));
}
