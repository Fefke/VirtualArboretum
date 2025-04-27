using System.Collections.Concurrent;
using System.Collections.Immutable;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.Entities;

/// <summary>
/// A garden does represent a namespace for a variety of plants,
/// a local mycelium
/// </summary>
public class Garden
{
    /// <summary>
    /// <i>All child plants are automatically being associated with this location.</i>
    /// </summary>
    public HyphaeStrain PrimaryLocation { get; } // does directly reflect into DirectoryInfo Location.

    public readonly Fingerprint UniqueMarker;

    private readonly ConcurrentDictionary<Fingerprint, Plant> _plants;

    public Garden(HyphaeStrain primaryLocation, IList<Plant> plants, Fingerprint? uniqueMarker)
    {
        PrimaryLocation = primaryLocation;
        UniqueMarker = uniqueMarker ?? new Fingerprint();
        _plants = new(
            concurrencyLevel: -1,
            capacity: plants.Count
            );

        foreach (var plant in plants.AsParallel())
        {
            AddPlant(plant);
        }
    }


    public Plant AddPlant(Plant plant)
    {

        plant.AssociateWith(PrimaryLocation);

        _plants.AddOrUpdate(
            plant.UniqueMarker,
            plant,
            (_, presentPlant) => plant);

        return plant;
    }

    public bool ContainsPlant(Fingerprint fingerprint)
    {
        return _plants.ContainsKey(fingerprint);
    }

    public Plant? GetPlant(Fingerprint fingerprint)
    {
        return _plants.GetValueOrDefault(fingerprint);
    }


    public ImmutableArray<Plant> GetPlants()
    {
        return _plants.Values.AsParallel().ToImmutableArray();
    }

    public int AmountOfPlants()
    {
        return _plants.Count;
    }

}
