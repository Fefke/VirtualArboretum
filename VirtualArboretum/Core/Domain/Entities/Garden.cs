using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Data.Common;
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

    private readonly ConcurrentDictionary<Fingerprint, Plant> _plants;

    public Garden(HyphaeStrain primaryLocation, IList<Plant> plants)
    {
        PrimaryLocation = primaryLocation;
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
    
    public Plant? GetPlant(Fingerprint fingerprint)
    {
        return _plants.GetValueOrDefault(fingerprint);
    }

}
