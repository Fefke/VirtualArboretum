using System.Collections.Immutable;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.Entities;

public class Plant
{
    
    public HyphaApex Name
    {
        // may be random name or a name & mutable
        get; init;
    }
    public readonly Fingerprint UniqueMarker;


    public Dictionary<string, Hypha>? AssociatedHyphae;
    // can only be set by providing Hypha, which is being serialized for key.

    public ImmutableSortedDictionary<Fingerprint, Cell> Cells
    {
        get; init;
    }


    public Plant(
        Fingerprint uniqueMarker,
        HyphaApex name,
        IEnumerable<Cell> cells,
        IEnumerable<Hypha>? associatedHyphae
        )
    {
        UniqueMarker = uniqueMarker;
        Name = name;
        Cells = cells.ToImmutableSortedDictionary(
            cell => cell.UniqueMarker,
            cell => cell
            );

        AssociatedHyphae = associatedHyphae?.ToDictionary(
            HyphaeHierarchy.AsString,
            hypha => hypha
            ) ?? new Dictionary<string, Hypha>();
    }


    public static Plant Cultivate(
        Cell seed, string? name, IEnumerable<Cell> cells, IEnumerable<Hypha>? associatedHyphae
        )
    {
        var uniqueMarker = new Fingerprint();
        return new Plant(
            uniqueMarker,
            new HyphaApex(name ?? uniqueMarker.ToString()),
            cells, associatedHyphae
        );
    }

}