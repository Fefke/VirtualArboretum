using System.Collections.Concurrent;
using System.Linq;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.AggregateRoots;

public class Arboretum
{
    // TODO: Does read in provided user-config (req. config-class)
    //private HyphaeStrain primaryHyphae; // Can be home-dir or user-specified-dir i.R.
    private readonly ConcurrentDictionary<Fingerprint, Garden> _gardens;
    // TODO: Is this is part of IGardenRepository alrdy?

    public Mycelium Mycelium { get; }

    public Arboretum(IEnumerable<Garden> gardens)
    {
        _gardens = new(gardens.ToDictionary(
            garden => garden.UniqueMarker
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


    public Garden? ViewGarden(Fingerprint uniqueMarker)
    {
        _gardens.TryGetValue(uniqueMarker, out var garden);
        return garden;
    }
}

