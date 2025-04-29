using System.Collections.Immutable;
using System.Text;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.Services;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Application.Services;

public class HyphaeSerializationService
{
    /// <summary>
    /// Deserializes a string into a hierarchy of Hyphae.<br></br>
    /// <b>Please note:</b> You have to provide a valid serialized string (according to HyphaeSerializer.Serialize(...)).
    /// </summary>
    public static ImmutableList<HyphaeStrain> Deserialize(string serialHyphaeStrains)
    {
        var manyHyphae = serialHyphaeStrains.Split(HyphaKey.StartMarker);
        var hyphae = new List<HyphaeStrain>(manyHyphae.Length);

        var errors = new StringBuilder();

        foreach (var serialHyphaeStrain in manyHyphae)
        {
            if (string.IsNullOrWhiteSpace(serialHyphaeStrain))
            {
                continue; // skip whitespace between hyphaeStrains & empty ones.
            }

            var trimmedSerialHyphaeStrain = serialHyphaeStrain.Trim();
            try
            {
                var hyphaeStrain = ParseHypha(trimmedSerialHyphaeStrain);
                hyphae.Add(hyphaeStrain);
            }
            catch (Exception e)
            {
                errors.AppendLine($" - '{serialHyphaeStrain}'.");
            }
        }

        if (errors.Length != 0)
        {
            throw new ArgumentException(
                $"The following hyphae are invalid:\n{errors}"
                );
        }

        return [.. hyphae];
    }


    /// <summary>
    /// Parses a single Hyphae from a serialized string.<br/>
    /// </summary>
    private static HyphaeStrain ParseHypha(string serialHyphae)
    {
        /*
         * Can be anything after an EXTENSION_MARKER...
         * ... so we split by SERIALIZED_DELIMITER first
         *     and read over empty elements.
         *
         *  #marker : HyphaApex is self containing ValueHypha (key=value).
         *  #key-value : ValueHypha
         *  #key-marker   : HyphaStrain(..., HyphaApex)
         *  #key-key-value : Hypha(..., ValueHypha)
         *
         */

        var hyphaHierarchy = serialHyphae.Split(HyphaKey.ExtensionDelimiter)
            .Select(s => s.Trim()) // remove whitespaces at start/end.
            .Where(s => !string.IsNullOrEmpty(s)) // remove empty entries by acceptable: `==` failures.
            .ToImmutableList();

        if (hyphaHierarchy.Count == 0)
        {
            // illegal case, if: '#' | '#=='
            throw new InvalidOperationException(
                $"Cannot parse empty Hyphae: '{serialHyphae}'"
            );
        }

        if (hyphaHierarchy.Count == 1)
        {
            // single HyphaApex (just on Hypha)
            return new(
                new HyphaApex(hyphaHierarchy.First())
                );
        }
        // Now at least 2 hypha are present.
        // Determine possible ValueType of last element.
        var lastHypha = hyphaHierarchy.Last();
        var predToLastHypha = hyphaHierarchy.TakeLast(2).First();


        var hyphaeValue = ParseHyphaType(predToLastHypha, lastHypha);

        var hyphaeBuilder = new HyphaeBuilder(hyphaeValue);

        var possibleHyphaExtensionLength = hyphaHierarchy.Count - 2;
        var possibleHyphaExtensions = hyphaHierarchy.Take(possibleHyphaExtensionLength);

        foreach (var hypha in possibleHyphaExtensions)
        {
            hyphaeBuilder.ExtendBy(
                new HyphaKey(hypha)
                );
            // ? will also construct legacy inner/recursive structure.
        }

        return new(
            hyphaeBuilder.Build()
            );
    }

    /// <summary>
    /// Does Parse a HyphaeType from a serialized string.
    /// By default a basic Hypha(HyphaApex) is returned.
    /// </summary>
    public static Hypha ParseHyphaType(string hyphaKey, string hyphaValue)
    {
        // TODO: Should be losely coupled thorwards HyphaType...
        // meaning, your Hypha-Instances should implement parse, but thats overkill for now.
        if (decimal.TryParse(hyphaValue, out decimal number))
        {
            return new DecimalHypha(hyphaKey, number);
        }

        return new Hypha(
            hyphaKey,
            new HyphaApex(hyphaValue)
            );
    }

    /// <summary>
    /// Does serialize a hierarchy of a Hyphae into a string.
    /// </summary>
    public static string Serialize(Hypha hyphae)
    {

        return Serialize(
            HyphaeHierarchy.AggregateHyphae(hyphae)
            );
    }

    public static string Serialize(HyphaeStrain hyphae)
    {
        return $"{HyphaKey.StartMarker}{HyphaeHierarchy.AsString(hyphae.Value)}";
    }

    /// <summary>
    /// Does serialize many hierarchies of a Hyphae into a single string.
    /// </summary>
    public static string Serialize(IEnumerable<Hypha> manyHyphae)
    {
        var aggregate = new StringBuilder();
        foreach (var hypha in manyHyphae)
        {
            aggregate.Append(Serialize(hypha));
        }

        return aggregate.ToString();
    }


    public static string Serialize(IEnumerable<HyphaeStrain> manyHyphae)
    {
        var aggregate = new StringBuilder();
        foreach (var hypha in manyHyphae)
        {
            aggregate.Append(Serialize(hypha));
        }

        return aggregate.ToString();
    }

    public static ImmutableList<string> SerializeEachListElement(IList<HyphaeStrain> manyHyphae)
    {
        var aggregate = new List<string>(manyHyphae.Count);
        foreach (var hypha in manyHyphae)
        {
            aggregate.Add(Serialize(hypha));
        }

        return aggregate.ToImmutableList();
    }
}