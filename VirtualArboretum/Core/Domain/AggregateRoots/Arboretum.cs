using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.AggregateRoots;

public class Arboretum
{
    // TODO: Does manage multiple Gardens...
    //  - Does read in provided user-config (req. config-class)


    private HyphaeStrain primaryHyphae; // Can be home-dir or user-specified-dir i.R.

    private readonly ConcurrentDictionary<Fingerprint, Garden> _gardens;

    private readonly Mycelium _mycelium;

    public Arboretum(IEnumerable<Garden> gardens, HyphaeStrain primaryHyphae)
    {
        _gardens = new(gardens.ToDictionary(
            garden => garden.UniqueMarker
            ));

        this.primaryHyphae = primaryHyphae;

        InitializeMycelium();
    }

    private void InitializeMycelium()
    {
        
        /*
         * TODO:
         *  1. Read in all Hyphae from all Gardens.
         *  2. Create a Mycelium for all Hyphae.
         *  3. Associate all HyphaeStrains in Mycelium.
         *  4. Associate all HyphaeStrains with their corresponding Fingerprints.
         */
    }


}