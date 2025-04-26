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
    public ImmutableArray<Hypha> Value { get; }

    public HyphaeStrain(ImmutableArray<Hypha> hyphae)
    {
        Value = hyphae;
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

        return Value.SequenceEqual(second.Value);
    }
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return HyphaeHierarchy.AsString(Value);
    }
}