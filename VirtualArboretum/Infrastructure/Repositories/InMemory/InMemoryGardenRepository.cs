using System.Collections.Concurrent;
using VirtualArboretum.Core.Application.DataTransferObjects.MappedToDomain;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Infrastructure.Repositories.InMemory;

/// <summary>
/// In-memory implementation of IGardenRepository for temporary & testing purposes.
/// </summary>
public class InMemoryGardenRepository : IGardenRepository
{
    private readonly ConcurrentDictionary<Fingerprint, Garden> _gardens = new();

    public InMemoryGardenRepository(IEnumerable<Garden> gardens)
    {
        foreach (var garden in gardens)
        {
            _gardens[garden.UniqueMarker] = garden;
        }
    }

    public InMemoryGardenRepository(IList<GardenDto> gardenTemplates)
    {
        _gardens = new ConcurrentDictionary<Fingerprint, Garden>(
            capacity: gardenTemplates.Count, concurrencyLevel: -1
            );

        foreach (var gardenTemplate in gardenTemplates)
        {
            var newGarden = GardenMapper.IntoGarden(gardenTemplate);
            // throws exception.

            _gardens.AddOrUpdate(
                newGarden.UniqueMarker,
                newGarden,
                (_, presentGarden) => throw new ArgumentException(
                    "You cannot define multiple gardens with same uniqueMarker" +
                    $" ({presentGarden.UniqueMarker})!"
                    )
            );
        }
    }

    public Task<Garden?> GetByFingerprintAsync(Fingerprint id)
    {
        _gardens.TryGetValue(id, out Garden? garden);
        return Task.FromResult(garden);
    }

    public Task<Garden?> GetByPrimaryHyphaeAsync(HyphaeStrain primaryStrain)
    {
        var garden = _gardens.Values.FirstOrDefault(g => g.PrimaryLocation.Equals(primaryStrain));
        return Task.FromResult(garden);
    }

    public Task AddAsync(Garden garden)
    {
        _gardens[garden.UniqueMarker] = garden;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Garden garden)
    {
        _gardens[garden.UniqueMarker] = garden;
        return Task.CompletedTask;
    }
}