using System.Collections.Immutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.Entities;

public class Plant
{

    public HyphaeStrain Name
    {
        // i.R. same as primary location of the plant in filesystem
        // ...relative to garden its placed in.
        get; init;
    }

    public readonly Fingerprint UniqueMarker;

    private readonly List<HyphaeStrain> _associatedHyphae;
    // Updated property to allow internal manipulation but external read-only access
    public ImmutableList<HyphaeStrain> AssociatedHyphae
    {
        get => _associatedHyphae.ToImmutableList();
        init => _associatedHyphae = value.ToList();
    }

    public ImmutableDictionary<Fingerprint, Cell> Cells
    {
        get; init;
    }


    public Plant(
        Fingerprint uniqueMarker,
        HyphaeStrain primaryHyphae,
        IList<Cell> cells,
        IList<HyphaeStrain>? associatedHyphae
        )
    {
        // 1.
        UniqueMarker = uniqueMarker;

        // 2.
        if (primaryHyphae.Value.Last() is not HyphaApex)
        {
            throw new ArgumentException(
                "HyphaeStrain must be a HyphaApex to increment version."
            );
        }

        Name = primaryHyphae;

        // 3.
        Dictionary<Fingerprint, Cell> cellsDict = new(cells.Count);

        foreach (var cell in cells)
        {
            cellsDict.Add(
                cell.UniqueMarker,
                cell
            );
        }

        this.Cells = cellsDict.ToImmutableDictionary();


        // 4.
        _associatedHyphae = associatedHyphae?.ToList()
                           ?? new List<HyphaeStrain>();

    }


    // Manipulate
    // TODO: Supply a Mycelium Factory for each plant, to allow a plant to subscribe to a Mycelium.
    public Plant AssociateWith(HyphaeStrain strain)
    {
        _associatedHyphae.Add(strain);
        return this;
    }

    /// <summary>
    /// Does cultivate this plant into a new type of plant, by adding new cells,<br/>
    /// while preserving a continuous naming convention (-v1 -v2 -v3 ... -vN).<br/>
    /// <b>Does not put primaryHyphae in a Mycelium!</b>
    /// </summary>
    /// <returns>Newly Cultivated Plant</returns>
    public Plant CultivateWith(
        IList<Cell> newCells,
        HyphaeStrain? primaryHyphae, List<HyphaeStrain>? additionalAssociatedHyphae,
        Fingerprint? uniqueMarker
        )
    {
        uniqueMarker ??= new Fingerprint();
        primaryHyphae ??= PlantNameHelper.IncrementVersion(Name);

        return new Plant(
            uniqueMarker,
            primaryHyphae,
            newCells,
            additionalAssociatedHyphae
        );
    }

    public Plant CrossWith(
        Plant otherPlant,
        HyphaeStrain primaryHyphae
    )
    {
        return CultivateWith(
            otherPlant.Cells.Values.ToList(),
            primaryHyphae,
            otherPlant._associatedHyphae,
            null
        );
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Plant);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueMarker, _associatedHyphae, Name, Cells);
    }

    public bool Equals(Plant? other)
    {
        if (other == null)
        {
            return false;
        }

        return Name.Equals(other.Name)
               && UniqueMarker.Equals(other.UniqueMarker)
               && Cells.GetHashCode() == other.Cells.GetHashCode()  // should be fine.
               && _associatedHyphae.SequenceEqual(other._associatedHyphae);
    }
}

public static class PlantNameHelper
{
    private static readonly char VersionSymbol = 'v';
    private static readonly Regex VersionRegex = new(
        $@"{VersionSymbol}(\d+)$", RegexOptions.Compiled
        );  // does check vor 'v1' or 'v134674'


    /// <summary>
    /// Checks if the name ends with a version pattern (-vN) and extracts the version number if present.
    /// </summary>
    /// <returns>The extracted version number if present, otherwise null.</returns>
    public static int ExtractVersion(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return 0;
        }

        var match = VersionRegex.Match(name);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    public static HyphaeStrain IncrementVersion(HyphaeStrain strain)
    {
        var version = 0;

        if (strain.Value.Last() is HyphaApex)
        {
            var versionHypha = (HyphaApex)strain.Value.Last();
            version = ExtractVersion(versionHypha.Value.ToString() ?? string.Empty);
        }

        var newVersion = version + 1;
        var newTail = new HyphaApex(
            $"{VersionSymbol}{newVersion}"
        );

        var newStrain = strain.Value
            .SkipLast(1)
            .Append(newTail)
            .ToImmutableArray();

        return new HyphaeStrain(newStrain);
    }
}
