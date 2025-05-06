using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.Services;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.AggregateRoots;

/// <summary>
/// The 'Mycelium' is the information hub...<br/>
/// ...that does contain all the information about all Hyphae, their combinations and associations.
/// </summary>
public class Mycelium
{
    /// <summary>
    /// The Hyphal Plexus does collect all Hyphae of any depth and does present them by their key and individual hierarchy.<br/>
    /// Access to every hypha on every level, while implicitly <b>not</b> being cycle-free!
    /// </summary>
    private readonly ConcurrentDictionary<HyphaKey, HashSet<HyphaeStrain>> _hyphalPlexus;

    /// <summary>
    /// Mycorrhizations are the connections between hyphae of fungi,<br/>
    /// which are identified by a HyphaeStrain that connects many <br/>
    /// Fingerprints of plants to the hyphalPlexus.
    /// </summary>
    private readonly ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>> _plantMycorrhizations;

    /// <summary>
    /// Garden Associations are the hierarchical relationship between gardens and plants,<br/>
    /// which are identified by a HyphaeStrain that connects many <br/>
    /// Fingerprints of gardens to the hyphalPlexus.
    /// </summary>
    private readonly ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>> _gardenAssociations;

    public Mycelium(
        IList<HyphaeStrain> hyphaeStrains,
        ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>> gardenAssociations,
        ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>> plantMycorrhizations)
    {
        _gardenAssociations = gardenAssociations;
        _plantMycorrhizations = plantMycorrhizations;

        var initialPlexusSize = hyphaeStrains.Sum(
            hyphaeStrain => hyphaeStrain.Value.Length
        );

        _hyphalPlexus = new(
            concurrencyLevel: -1,
            capacity: initialPlexusSize
        );

        this.ExtendWith(hyphaeStrains);
    }

    public Mycelium()
        : this(
            new List<HyphaeStrain>(),
            new ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>>(),
            new ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>>())
    { }

    private void ExtendWith(IList<HyphaeStrain> hyphaeStrains)
    {
        hyphaeStrains.AsParallel().ForAll(ExtendWith);
    }

    private void ExtendWith(HyphaeStrain hyphaeStrain)
    {
        hyphaeStrain.Value.AsParallel().ForAll(
            hypha => _hyphalPlexus.AddOrUpdate(
                hypha.Key,
                _ => [hyphaeStrain], // add
                (key, existingValue) =>  // update
                {
                    existingValue.Add(new HyphaeStrain(hypha));
                    // does provide indicator for already present.
                    return existingValue;
                })
            );

    }

    public bool Contains(ImmutableArray<Hypha> flatHyphae)
    {
        var firstHypha = flatHyphae[0];
        var isInPlexus = _hyphalPlexus.TryGetValue(firstHypha.Key, out var strain);

        if (!isInPlexus || strain == null)
        {
            return false;
        }

        if (flatHyphae.Length == 1 && strain.Count == 1)
        {
            return true; // as self is last present.
        }

        // is tail part of the strain?
        var tailLength = flatHyphae.Length - 1;
        var tailHyphae = flatHyphae.Slice(1, tailLength);
        var tail = new HyphaeStrain(tailHyphae);

        return strain.Contains(tail);
    }

    /*public bool Contains(Hypha hypha)
    {
        var flatHypha = HyphaeHierarchy.AggregateHyphae(hypha);
        return this.Contains(flatHypha);
    }*/

    public bool Contains(HyphaeStrain hyphae)
    {
        return this.Contains(hyphae.Value);
    }


    // # Garden Associations
    public void AssociateWithGarden(HyphaeStrain strain, Garden association)
    {
        _gardenAssociations.AddOrUpdate(
            strain,
            _ => [association.UniqueMarker],  // add
            (_, existingAssociations) =>  // update
            {
                existingAssociations.Add(association.UniqueMarker);
                return existingAssociations;
            });

        // extend plexus with each hypha & associate it with this strain.
        ExtendWith(strain);

    }

    public void AssociateWithGarden(IEnumerable<HyphaeStrain> hyphaeStrains, Garden association)
    {
        hyphaeStrains.AsParallel().ForAll(
            hyphaeStrain => this.AssociateWithGarden(hyphaeStrain, association)
            );
    }


    public HashSet<Fingerprint>? GetGardenAssociation(HyphaeStrain hyphae)
    {
        _gardenAssociations.TryGetValue(
            hyphae, out var associations
        );

        return associations;
    }


    /// <summary>
    /// Does associate the plant with the mycelium by all the plants HyphaeStrains (including Name).
    /// </summary>
    public void Mycorrhizate(Plant plant)
    {
        // 1. Plants identity
        ExtendWith(plant.Name);
        _plantMycorrhizations.AddOrUpdate(
            plant.Name,
            _ => [plant.UniqueMarker],  // add
            (_, fingerprints) =>
            {
                fingerprints.Add(plant.UniqueMarker);
                return fingerprints;
            }
        );

        // 2. Plants additional associations
        ExtendWith(plant.AssociatedHyphae);
        plant.AssociatedHyphae.AsParallel().ForAll(strain =>
        {
            _plantMycorrhizations.AddOrUpdate(
                strain,
                _ => [plant.UniqueMarker],  // add
                (_, fingerprints) =>
                {
                    fingerprints.Add(plant.UniqueMarker);
                    return fingerprints;
                }
            );
        });
    }


    // # Test Methods

    /// <summary>
    /// Describes whether a hyphae strain is associated to a specific Fingerprint by the mycelium already.
    /// </summary>
    public bool ContainsMycorrhization(HyphaeStrain hyphae, Fingerprint association)
    {
        var containsAssociation = _plantMycorrhizations.TryGetValue(
            hyphae, out var associations
        );

        if (!containsAssociation || associations == null)
        {
            return false;
        }

        return associations.Contains(association);
    }

    /// <summary>
    /// Describes whether a hyphae strain is associated to a specific Fingerprint by the mycelium already.
    /// </summary>
    public bool ContainsMycorrhization(Hypha hyphae, Fingerprint association)
    {
        return ContainsMycorrhization(new HyphaeStrain(hyphae), association);
    }

    // ### Multiple
    /// <summary>
    /// True, if all manyHyphae are associated with a Fingerprint. False if  at least one is not.
    /// </summary>
    public bool ContainsMycorrhizations(IEnumerable<Hypha> manyHyphae, Fingerprint association)
    {
        return manyHyphae.AsParallel().All(
            hyphae => this.ContainsMycorrhization(hyphae, association)
            );
    }

    /// <summary>
    /// True, if all manyHyphae are associated with a Fingerprint. False if  at least one is not.
    /// </summary>
    public bool ContainsMycorrhizations(IEnumerable<HyphaeStrain> manyHyphae, Fingerprint association)
    {
        return manyHyphae.AsParallel().All(
            hyphae => this.ContainsMycorrhization(hyphae, association)
        );
    }


    public ImmutableList<Fingerprint> GetMycorrhization(HyphaeStrain hyphae)
    {
        _plantMycorrhizations.TryGetValue(
            hyphae, out var associations
        );
        var fingerprints = associations?.ToImmutableList();

        return fingerprints ?? ImmutableList<Fingerprint>.Empty;
    }

    /// <summary>
    /// Does match all your hyphaeStrains to an ImmutableList, which might be empty!
    /// </summary>
    public ImmutableDictionary<HyphaeStrain, ImmutableList<Fingerprint>> GetMycorrhizations(IList<HyphaeStrain> hyphaeStrains)
    {
        return hyphaeStrains.AsParallel()
            .Select(hyphaeStrain => (
                hyphaeStrain, // Item1
                GetMycorrhization(hyphaeStrain) // Item2
                ))
            .ToImmutableDictionary(
                kve => kve.Item1,
                kve => kve.Item2
                );
    }

    public ImmutableDictionary<HyphaeStrain, HashSet<Fingerprint>> GetAllMycorrhizations()
    {
        return _plantMycorrhizations.ToImmutableDictionary();
    }

}