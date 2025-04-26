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
            // TODO: Following is not ideal, but idc atm.
            try
            {
                var hypha = ParseHypha(trimmedSerialHyphaeStrain);
                var flatHyphae = HyphaeHierarchy.Flatten(hypha);
                var hyphaeStrain = new HyphaeStrain(flatHyphae);
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
    /// Parses a single Hyphae from a serialized string.
    /// </summary>
    /// <param name="serialHyphae"></param>
    /// <returns></returns>
    private static Hypha ParseHypha(string serialHyphae)
    {
        /*
         * Can be anything after an EXTENSION_MARKER...
         * ... so we split by SERIALIZED_DELIMITER first
         *     and read over empty elements.
         *
         *  #marker : HyphaApex is self containing ValueHypha (key=value).
         *  #key=value : ValueHypha
         *  #key=marker   : Hypha(HyphaApex)
         *  #key=key=value : Hypha(ValueHypha)
         *
         */

        var hyphaHierarchy = serialHyphae.Split(HyphaKey.ExtensionDelimiter)
            .Select(s => s.Trim()) // remove whitespaces at start/end.
            .Where(s => !string.IsNullOrEmpty(s)) // remove empty entries by acceptable: `==` failures.
            .Reverse() // due to bottom-up (rtl) build, after ltr read in.
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
            // single HyphaApex
            return new HyphaApex(hyphaHierarchy.First());
        }
        // otherwise possible ValueType is First and hierarchy is tail.

        var mostInnerHyphaKey = hyphaHierarchy[1];
        var serialHyphaValue = hyphaHierarchy.First();
        var hyphaKeyHierarchy = hyphaHierarchy.RemoveRange(0, 2);

        var hyphaValue = ParseHyphaType(mostInnerHyphaKey, serialHyphaValue);

        var hyphae = new HyphaeBuilder(hyphaValue);

        foreach (var hypha in hyphaKeyHierarchy)
        {
            hyphae.ExtendBy(new HyphaKey(hypha));
        }

        return hyphae.Build();
    }

    /// <summary>
    /// Does Parse a HyphaeType from a serialized string.
    /// By default a basic Hypha(HyphaApex) is returned.
    /// </summary>
    public static Hypha ParseHyphaType(string hyphaKey, string hyphaValue)
    {

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