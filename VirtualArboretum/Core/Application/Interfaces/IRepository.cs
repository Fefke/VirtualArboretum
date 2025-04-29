using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.Interfaces;

public interface IRepository<T>
{
    Task<T?> GetByFingerprintAsync(Fingerprint id);
    Task<T?> GetByPrimaryHyphaeAsync(HyphaeStrain strain);

    Task AddAsync(T candidate);
    Task UpdateAsync(T candidate);
}