using System.Collections.Immutable;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.Entities;

/// <summary>  
/// A Phenotyped Plant is a <b>Memento</b> of a plant,  
/// meaning it has been characterized and frozen in time  
/// based on its observable traits.  
/// </summary>  
public class PhenotypedPlant
{

    public Fingerprint UniqueMarker { get; init; }

    public ImmutableDictionary<Fingerprint, Cell> Cells { get; init; }
    public ImmutableList<HyphaeStrain> AssociateHyphae { get; init; }
    // sorted dict may make hyphae more accessible...

    public PhenotypedPlant(Plant originator, DateTimeOffset? timeSliceIdentifier)
    {
        UniqueMarker = new Fingerprint(
            Guid.CreateVersion7(timeSliceIdentifier ?? DateTimeOffset.Now)
            );

        Cells = originator.Cells.ToImmutableDictionary(
            cell => cell.Key,
            cell => cell.Value
        );

        AssociateHyphae = originator.AssociatedHyphae.ToImmutableList();
    }
}
