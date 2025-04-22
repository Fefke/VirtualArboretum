using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.Services;

public class HyphaeHierarchy
{

    /// <summary>
    /// Does AggregateHyphaeKeys a Hyphae into a flat array of HyphaeKeys.
    /// You can still access each element by its key and resolve recursively.
    /// </summary>
    /// <param name="root"></param>
    /// <returns>Flattened Array of Hyphae Key</returns>
    public static ImmutableArray<HyphaKey> AggregateHyphaeKeys(Hypha root)
    {
        var aggregatedHypha = AggregateHyphae(root);
        var hyphaKeys = new List<HyphaKey>(aggregatedHypha.Length);
        foreach (var hypha in aggregatedHypha)
        {
            hyphaKeys.Add(hypha.Key);
        }

        return [..hyphaKeys];
    }

    /// <summary>
    /// Does flatten your linked list hypha into array format,
    /// while persisting its possible last value at that stage.
    /// </summary>
    public static ImmutableArray<Hypha> Flatten(Hypha hypha)
    {
        var flatHyphae = new List<Hypha>() { hypha };
        while (hypha.DoesExtend())
        {
            if (hypha.Value is Hypha continuingHypha)
            {
                flatHyphae.Add(
                    continuingHypha
                    );

                hypha = continuingHypha;
            }
            else
            {
                break;
            }

        }

        return [.. flatHyphae];
    }


    /// <summary>
    /// Does Aggregate Hyphae into a flat array of single Hypha.
    /// You can still access each sub-hypha recursively.
    /// </summary>
    /// <param name="root"></param>
    /// <returns>Flattened Array of Hyphae Key</returns>
    public static ImmutableArray<Hypha> AggregateHyphae(Hypha root)
    {
        return [.. AggregateSubHyphae(root, 1).Item1];
    }

    private static (Hypha[], ushort) AggregateSubHyphae(Hypha hypha, ushort depth = 0)
    {
        if (hypha.DoesExtend())
        {
            var (list, index) = AggregateSubHyphae(
                hypha.NextExtension()!,
                ++depth
            );

            list[index] = hypha;
            return (list, --index);
        }

        // recursion basis.
        Hypha[] aggregate = new Hypha[depth];
        depth -= 1;
        aggregate[depth] = hypha;
        depth -= 1;
        return (aggregate, depth);
    }


    public static string AsString(Hypha root)
    {
        ImmutableArray<Hypha> aggregate = AggregateHyphae(root);
        return HyphaeHierarchy.AsString(aggregate);
    }

    public static string AsString(ImmutableArray<Hypha> flatHypha)
    {
        var serializedAggregate = new StringBuilder(flatHypha.Length);

        foreach (var hypha in flatHypha)
        {
            serializedAggregate.Append(hypha.Key);
            serializedAggregate.Append(hypha.Key.GetExtensionDelimiter());
        }

        var lastElementIdx = serializedAggregate.Length - 1;
        serializedAggregate.Remove(lastElementIdx, 1);
        // ? as we don't want to have a trailing delimiter.

        return serializedAggregate.ToString();
        //return root.ToString(); // Less efficient, as many strings might be created temporarily.
    }
}
