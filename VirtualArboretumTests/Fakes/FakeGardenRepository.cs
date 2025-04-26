using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.Fakes;

public class FakeGardenRepository : IGardenRepository
{
    public Task<Garden?> GetByFingerprintAsync(Fingerprint id)
    {
        throw new NotImplementedException();
    }

    public Task<Garden?> GetByPrimaryHyphaeAsync(HyphaeStrain strain)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Garden candidate)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Garden candidate)
    {
        throw new NotImplementedException();
    }
}