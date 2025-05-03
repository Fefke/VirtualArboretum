using System.Collections.Concurrent;

namespace VirtualArboretum.Core.Domain.AggregateRoots;

public class Arboretum
{
    // TODO: Does read in provided user-config (req. config-class)

    private readonly ConcurrentDictionary<HyphaeStrain, Garden> _gardens;

    public Mycelium Mycelium { get; }

    public Arboretum(IEnumerable<Garden> gardens)
    {
        _gardens = new(gardens.ToDictionary(
            garden => garden.PrimaryLocation
            ));

        Mycelium = new Mycelium();
        InitializeMyceliumWith(_gardens.Values);
    }

    private void InitializeMyceliumWith(ICollection<Garden> gardens)
    {

        // Associate all Gardens, Plants and their hyphae
        gardens.AsParallel().ForAll(
            garden =>
            {
                Mycelium.AssociateWith(
                    garden.PrimaryLocation, garden.UniqueMarker
                );

                // Associate all Plants and their hyphae
                garden.GetPlants().AsParallel()
                    .ForAll(Mycelium.Mycorrhizate);
            });
    }


    public Garden? ViewGarden(HyphaeStrain primaryLocation)
    {
        _gardens.TryGetValue(primaryLocation, out var garden);
        return garden;
    }

    public ICollection<HyphaeStrain> GetAllGardensPrimaryLocation()
    {
        return _gardens.Keys;
    }
}

