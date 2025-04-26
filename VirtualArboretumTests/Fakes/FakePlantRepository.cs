using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.Fakes;

public class FakePlantRepository : IPlantRepository
{
    public Task<Plant?> GetByFingerprintAsync(Fingerprint id)
    {
        throw new NotImplementedException();
    }

    public Task<Plant?> GetByPrimaryHyphaeAsync(HyphaeStrain strain)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Plant candidate)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Plant candidate)
    {
        throw new NotImplementedException();
    }
}