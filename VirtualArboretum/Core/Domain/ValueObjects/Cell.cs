using VirtualArboretum.Core.Domain.AggregateRoots;

namespace VirtualArboretum.Core.Domain.ValueObjects;

/// <summary>
/// A single cell does represent a reference to any kind of data (its organell),<br/>
/// represented by its organellLocation, which should be an Extension of Plant Name:<br/>
/// `#my-plant` -> `#my-plant-has-a-cell` (although not enforced).
/// </summary>
public class Cell
{

    public readonly Fingerprint UniqueMarker;

    public CellType Type { get; }
    public HyphaeStrain OrganellLocation { get; }

    public Cell(CellType type, HyphaeStrain organellLocation, Fingerprint uniqueMarker)
    {
        UniqueMarker = uniqueMarker;
        OrganellLocation = organellLocation;
        Type = type;
    }

    public Cell(HyphaeStrain organellLocation, Fingerprint uniqueMarker)
        : this(new CellType("application/octet-stream"), organellLocation, uniqueMarker)
    { }

    public Cell(CellType type, HyphaeStrain organellLocation)
        : this(type, organellLocation, new Fingerprint())
    { }

    public Cell(HyphaeStrain organellLocation)
        : this(new CellType("application/octet-stream"), organellLocation, new Fingerprint())
    { }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Cell);
    }

    public bool Equals(Cell? other)
    {
        return other != null
               && UniqueMarker.Equals(other.UniqueMarker)
               && Type.Equals(other.Type)
               && OrganellLocation.Equals(other.OrganellLocation);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueMarker, Type, OrganellLocation);
    }
}