using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.AggregateRoots;

public class HyphaeHierarchy
{

    /// <summary>
    /// Does Aggregate a Hyphae into a flat array of HyphaeKeys.
    /// You can still access each element by its key and resolve recursively.
    /// </summary>
    /// <param name="root"></param>
    /// <returns>Flattened Array of Hyphae Key</returns>
    public static ImmutableArray<HyphaKey> Aggregate(Hypha root)
    {
        return [.. AggregateSubHyphae(root, 1).Item1];
    }

    public static string AsString(Hypha root)
    {
        ImmutableArray<HyphaKey> aggregate = Aggregate(root);
        var serializedAggregate = new StringBuilder(aggregate.Length);

        foreach (var hyphaKey in aggregate)
        {
            serializedAggregate.Append(hyphaKey);
            serializedAggregate.Append(hyphaKey.GetExtensionDelimiter());
        }
        
        var lastElementIdx = serializedAggregate.Length - 1;
        serializedAggregate.Remove(lastElementIdx, 1);
        // ? as we don't want to have a trailing delimiter.

        return serializedAggregate.ToString();
        //return root.ToString(); // Less efficient, as many strings might be created temporarily.
    }

    private static (HyphaKey[], ushort) AggregateSubHyphae(Hypha label, ushort depth = 0)
    {
        if (label.DoesExtend())
        {
            var (list, index) = AggregateSubHyphae(
                label.NextExtension()!,
                ++depth
                );

            list[index] = label.Key;
            return (list, --index);
        }

        // recursion basis.
        HyphaKey[] aggregate = new HyphaKey[depth];
        depth -= 1;
        aggregate[depth] = label.Key;
        depth -= 1;
        return (aggregate, depth);
    }

}
