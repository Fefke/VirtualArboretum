using System.Collections.Immutable;
using System.Linq;
using VirtualArboretum.Core.Domain.Services;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.AggregateRoots;

/// <summary>
/// Does represent a strain of Hyphae, which is an aggregation of Hypha<br/> 
/// </summary>
public class HyphaeStrain
{
    public ImmutableArray<Hypha> Hyphae { get; }

    public HyphaeStrain(ImmutableArray<Hypha> hyphae)
    {
        Hyphae = hyphae;
    }

    public HyphaeStrain(Hypha hyphae)
    : this(HyphaeHierarchy.AggregateHyphae(hyphae))
    { }

    public override bool Equals(object? obj)
    {
        return Equals(obj as HyphaeStrain);
    }


    public bool Equals(HyphaeStrain? second)
    {
        if (second == null)
        {
            return false;
        }

        return Hyphae.SequenceEqual(second.Hyphae);
    }
    public override int GetHashCode()
    {
        return Hyphae.GetHashCode();
    }
}