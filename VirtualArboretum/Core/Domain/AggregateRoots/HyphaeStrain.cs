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

    public override bool Equals(object? other)
    {
        if (other is not HyphaeStrain otherHyphaeStrain)
        {
            return false;
        }

        return Value.SequenceEqual(otherHyphaeStrain.Value);
    }

    public override int GetHashCode()
    {
        if (Value.IsDefaultOrEmpty)
        {
            return 0;
        }

        var hashCombiner = new HashCode();
        foreach (var hypha in Value)
        {
            hashCombiner.Add(hypha);
        }

        return hashCombiner.ToHashCode();
    }

    public override string ToString()
    {
        return HyphaeHierarchy.AsString(Value);
    }
}