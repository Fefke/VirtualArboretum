using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.Fakes;

public class FakeArboretumRepository : IArboretumRepository
{
    public Task<Arboretum?> GetByFingerprintAsync(Fingerprint id)
    {
        throw new NotImplementedException();
    }

    public Task<Arboretum?> GetByPrimaryHyphaeAsync(HyphaeStrain strain)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Arboretum candidate)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Arboretum candidate)
    {
        throw new NotImplementedException();
    }
}