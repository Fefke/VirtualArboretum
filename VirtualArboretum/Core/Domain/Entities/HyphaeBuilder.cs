using VirtualArboretum.Core.Domain.Services;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.Entities;

/// <summary>
/// Does Build a HyphaeString out of a list of HyphaeKeys
/// with a Hyphae as head, building the final Hyphae bottom up.
/// </summary>
public class HyphaeBuilder
{

    private readonly List<HyphaKey> _hyphaeStrain;

    private readonly Hypha _root;

    public HyphaeBuilder(Hypha root, List<HyphaKey>? hyphaeStrain)
    {
        this._root = root;
        this._hyphaeStrain = hyphaeStrain ?? new List<HyphaKey>();
    }

    public HyphaeBuilder(Hypha root, List<Hypha> hyphaeStrain)
        : this(root, hyphaeStrain.Select(hypha => hypha.Key).ToList()) { }

    public HyphaeBuilder(Hypha root, Hypha hyphaeStrain)
        : this(
            root,
            HyphaeHierarchy
                .AggregateHyphaeKeys(hyphaeStrain)
                .ToList()
            )
    { }

    public HyphaeBuilder(Hypha root)
        : this(root, new List<HyphaKey>()) { }


    /// <summary>
    /// Adds a new Hyphae to the tail (head:tail) of the HyphaeString,
    /// meaning it <b>will be new first</b> Hyphae in the hierarchy once you build.
    /// </summary>
    /// <example>
    /// new HyphaeBuilder(new MarkerHyphae("Final Marker"))
    ///     .ExtendBy(HyphaA)
    ///     .ExtendBy(HyphaB)
    ///     .ExtendBy(HyphaC)  // new entry-point.
    ///     .Build();
    ///
    /// will produce: HyphaC(HyphaB(HyphaA(MarkerHyphae)))
    ///  => as string: #HyphaC-HyphaB-HyphaA-MarkerHyphae
    /// 
    /// </example>
    public HyphaeBuilder ExtendBy(HyphaKey key)
    {
        _hyphaeStrain.Add(key);
        return this;
    }

    public HyphaeBuilder ExtendBy(Hypha hypha)
    {
        return this.ExtendBy(hypha.Key);
    }

    public Hypha Build()
    {
        Hypha? hyphae = this._root;

        if (hyphae == null && _hyphaeStrain.Count == 0)
        {
            throw new InvalidOperationException(
                "No Hyphae to build. " +
                "Please add at least one key " +
                "or one specific Hyphae as head and build with that."
                );
        }

        _hyphaeStrain.Reverse();
        // as we have to build the hierarchy from the bottom up.
        // HyphaA(HyphaB(HyphaC(MarkerHypha)))
        // will be built as: #HyphaA=HyphaB -> HyphaC -> MarkerHypha

        if (hyphae == null)
        {
            // will represent itself...
            hyphae = new HyphaApex(_hyphaeStrain[0]);
            _hyphaeStrain.RemoveAt(0);
        }


        // build the hierarchy from the bottom up.
        foreach (var key in _hyphaeStrain)
        {
            hyphae = new Hypha(key, hyphae);
        }

        return hyphae;
    }

    public override string ToString()
    {
        return HyphaeHierarchy.AsString(
            this.Build()
            );
    }
}