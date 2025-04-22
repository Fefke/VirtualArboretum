using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.Services;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Infrastructure.Services;

public class HyphaeSerializer
{
    /// <summary>
    /// Deserializes a string into a hierarchy of Hyphae.<br></br>
    /// <b>Please note:</b> You have to provide a valid serialized string (according to HyphaeSerializer.Serialize(...)).
    /// </summary>
    public static ImmutableList<Hypha> Deserialize(string serial)
    {
        var serialHyphae = serial.Split(HyphaKey.StartMarker);
        var hyphae = new List<Hypha>(serialHyphae.Length);

        foreach (var serialHypha in serialHyphae)
        {
            if (String.IsNullOrWhiteSpace(serialHypha))
            {
                continue; // just overread an unpleasenties.
            }

            var hypha = ParseHypha(serialHypha);
            hyphae.Add(hypha);
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
        return HyphaKey.StartMarker + HyphaeHierarchy.AsString(hyphae);
    }

    /// <summary>
    /// Does serialize many hierarchies of a Hyphae into a single string.
    /// </summary>
    public static string Serialize(IEnumerable<Hypha> hyphae)
    {
        var aggregate = new StringBuilder();
        foreach (var hypha in hyphae)
        {
            aggregate.Append(Serialize(hypha));
        }

        return aggregate.ToString();
    }
}