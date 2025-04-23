using System.Collections.Concurrent;
using System.Collections.Immutable;
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
    /// The Mycorrhizal Associations are the connections between hyphae of fungi,<br/>
    /// which are identified by a HyphaeStrain that connects many <br/>
    /// Fingerprints of plants/tools/gardens.
    /// </summary>
    /// <!-- TODO: Should be externalized in a Mycorrhizal Network, which is partializable for each garden. -->
    private readonly ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>> _mycorrhizalAssociations;

    public Mycelium(
        IList<Hypha> hyphae,
        ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>> mycorrhizalAssociations)
    {
        _mycorrhizalAssociations = mycorrhizalAssociations;

        _hyphalPlexus = new(
            concurrencyLevel: -1,
            capacity: hyphae.Count()
            // is larger, if not flat k-v, but not determined at this point.
            );

        this.ExtendWith(hyphae);
    }

    public Mycelium(
        IList<HyphaeStrain> hyphaeStrains,
        ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>> mycorrhizalAssociations)
    {
        _mycorrhizalAssociations = mycorrhizalAssociations;

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
            new ConcurrentDictionary<HyphaeStrain, HashSet<Fingerprint>>())
    { }

    private Mycelium ExtendWith(IList<HyphaeStrain> hyphaeStrains)
    {
        hyphaeStrains.AsParallel().ForAll(
            hyphaeStrain => this.ExtendWith(hyphaeStrain)
            );

        return this;
    }

    private Mycelium ExtendWith(HyphaeStrain hyphaeStrain)
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

        return this;
    }

    private Mycelium ExtendWith(IEnumerable<Hypha> hyphae)
    {
        foreach (var hypha in hyphae)
        {
            this.ExtendWith(hypha);
        }

        return this;
    }


    public Mycelium ExtendWith(Hypha hypha)
    {
        // Make sure hyphae are present.
        ExtendWith(new HyphaeStrain(hypha));

        return this;
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

    public bool Contains(Hypha hypha)
    {
        var flatHypha = HyphaeHierarchy.AggregateHyphae(hypha);
        return this.Contains(flatHypha);
    }


    public Mycelium AssociateWith(Hypha hypha, Fingerprint association)
    {
        // 1. Make sure hypha is present
        if (!Contains(hypha))
        {
            ExtendWith(hypha);
        }

        // 2. Associate it with fingerprint
        var strain = new HyphaeStrain(hypha);
        _mycorrhizalAssociations.AddOrUpdate(
            strain,
            _ => [association],  // add
            (_, existingAssociations) =>  // update
            {
                existingAssociations.Add(association);
                return existingAssociations;
            });

        return this;
    }

    public Mycelium AssociateWith(IEnumerable<Hypha> hyphae, Fingerprint association)
    {
        hyphae.AsParallel().ForAll(
            hypha => this.AssociateWith(hypha, association)
            );

        return this;
    }

    public Mycelium AssociateWith(HyphaeStrain hyphae, Fingerprint association)
    {
        return this.AssociateWith(hyphae.Value, association);
    }

    public void AssociateWith(IEnumerable<HyphaeStrain> hyphaeStrains, Fingerprint plantUniqueMarker)
    {
        hyphaeStrains.AsParallel().ForAll(
            hyphaeStrain => this.AssociateWith(hyphaeStrain, plantUniqueMarker)
            );
    }


    // Test Methods
    public bool ContainsAssociation(Hypha hyphae, Fingerprint association)
    {
        var containsAssociation = _mycorrhizalAssociations.TryGetValue(
            new HyphaeStrain(hyphae), out var associations
            );

        if (!containsAssociation || associations == null)
        {
            return false;
        }

        return associations.Contains(association);
    }

    public bool ContainsAssociations(IEnumerable<Hypha> manyHyphae, Fingerprint association)
    {
        return manyHyphae.AsParallel().All(
            hyphae => this.ContainsAssociation(hyphae, association)
            );
    }

}
