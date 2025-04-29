using VirtualArboretum.Core.Domain.AggregateRoots;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.Fakes;

public class FakeGardenFactory
{
    public static Garden CreateRawTestGarden(string name)
    {
        var hypha = new HyphaApex(name);
        var strain = new HyphaeStrain(hypha);
        return new Garden(strain, new List<Plant>(), new Fingerprint());
    }
}