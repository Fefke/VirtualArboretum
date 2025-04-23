using System.Collections.Concurrent;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.AggregateRoots;

public class Arboretum
{
    // TODO: Does read in provided user-config (req. config-class)

    //private HyphaeStrain primaryHyphae; // Can be home-dir or user-specified-dir i.R.

    private readonly ConcurrentDictionary<Fingerprint, Garden> _gardens;

    private readonly Mycelium _mycelium;

    public Arboretum(IEnumerable<Garden> gardens)
    {
        _gardens = new(gardens.ToDictionary(
            garden => garden.UniqueMarker
            ));

        _mycelium = new Mycelium();
        InitializeMyceliumWith(_gardens.Values);
    }

    private void InitializeMyceliumWith(ICollection<Garden> gardens)
    {

        // Associate all Gardens, Plants and their hyphae
        gardens.AsParallel().ForAll(
            garden =>
            {
                _mycelium.AssociateWith(
                    garden.PrimaryLocation, garden.UniqueMarker
                );

                // Associate all Plants and their hyphae
                garden.GetPlants().AsParallel()
                    .ForAll(Mycorrhizate);
            });
    }

    /// <summary>
    /// Does associate the plant with the mycelium by all the plants HyphaeStrains (including Name).
    /// </summary>
    /// TODO: This Method might be more appropriate in Garden, with local Mycelium. Not for now tho.
    public void Mycorrhizate(Plant plant)
    {
        // Plants identity
        _mycelium.AssociateWith(
            plant.Name, plant.UniqueMarker
        );
        // Plants associations
        _mycelium.AssociateWith(
            plant.AssociatedHyphae, plant.UniqueMarker
        );
    }


    public Garden? ViewGarden(Fingerprint uniqueMarker)
    {
        _gardens.TryGetValue(uniqueMarker, out var garden);
        return garden;
    }
}

