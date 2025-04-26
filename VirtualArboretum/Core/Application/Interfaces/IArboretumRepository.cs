using VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;
using VirtualArboretum.Core.Domain.AggregateRoots;

namespace VirtualArboretum.Core.Application.Interfaces;

public interface IArboretumRepository
{
    Arboretum Open();  // With default configuration
    Boolean Close();
}