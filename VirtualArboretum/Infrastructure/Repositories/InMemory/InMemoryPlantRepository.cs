using System.Collections.Concurrent;
using VirtualArboretum.Core.Application.DataTransferObjects.MappedToDomain;
using VirtualArboretum.Core.Application.DataTransferObjects.ModelDTOs;
using VirtualArboretum.Core.Application.Interfaces;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Infrastructure.Repositories.InMemory;

public class InMemoryPlantRepository : IPlantRepository
{
    private readonly ConcurrentDictionary<Fingerprint, Plant> _plants;

    private readonly ConcurrentDictionary<HyphaeStrain, Fingerprint> _primaryHyphaeAssociations;
    // i.R. is like the primary location of the plant in filesystem.

    public InMemoryPlantRepository(IEnumerable<Plant> plants)
    {
        var allLocalPlants = plants as Plant[] ?? plants.ToArray();

        _plants = new(
            allLocalPlants.ToDictionary(
                plant => plant.UniqueMarker,
                plant => plant
                )
            );

        _primaryHyphaeAssociations = new(
            allLocalPlants.ToDictionary(
                plant => plant.Name,
                plant => plant.UniqueMarker
            )
        );
    }


    public InMemoryPlantRepository(IList<PlantDto> plantTemplates)
        : this(PlantMapper.IntoPlant(plantTemplates))
    { }

    public InMemoryPlantRepository() 
        : this(new List<Plant>())
    { }

    public Task<Plant?> GetByFingerprintAsync(Fingerprint uniqueMarker)
    {

        return Task.FromResult(_plants.GetValueOrDefault(uniqueMarker));
    }

    public Task<Plant?> GetByPrimaryHyphaeAsync(HyphaeStrain strain)
    {
        var plantIsPresent = _primaryHyphaeAssociations.TryGetValue(
            strain, out var uniqueMarker
            );

        if (!plantIsPresent)
        {
            return Task.FromResult<Plant?>(null);
        }

        // resolve plant by its uniqueMarker
        var possiblePlant = _plants!.GetValueOrDefault(uniqueMarker);

        return Task.FromResult(possiblePlant);
    }

    public Task AddAsync(Plant candidate)
    {
        if (_plants.TryAdd(candidate.UniqueMarker, candidate))
        {
            return Task.CompletedTask;
        }
        return Task.FromCanceled(CancellationToken.None);
    }

    public Task UpdateAsync(Plant candidate)
    {
        if (_plants.TryUpdate(candidate.UniqueMarker, candidate, candidate))
        {
            return Task.CompletedTask;
        }

        return Task.FromCanceled(CancellationToken.None);
    }
}