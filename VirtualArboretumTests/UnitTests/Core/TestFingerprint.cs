using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.UnitTests.Core;

[TestClass]
public class TestFingerprint
{
    [TestMethod]
    public void TestFingerprintTimestampExtraction()
    {
        var timestamps = new[]
        {
            DateTimeOffset.FromUnixTimeSeconds(987709362),
            DateTimeOffset.FromUnixTimeSeconds(1615676400),
            GetRandomDateTimeOffset()
        };

        var fingerprints = WrapFingerprints(WrapGuids(timestamps));


        var deserializedDateTime = new List<DateTimeOffset>(timestamps.Length);

        foreach (var fingerprint in fingerprints)
        {
            deserializedDateTime.Add(fingerprint.GetCreationDateTime());
        }


        for (int i = 0; i < fingerprints.Length; i++)
        {
            Assert.AreEqual(timestamps[i], deserializedDateTime[i]);
        }
    }

    // Arrange-Helper
    private DateTimeOffset GetRandomDateTimeOffset()
    {
        return DateTimeOffset.FromUnixTimeSeconds(
            Random.Shared.NextInt64(253402300799)
        );
    }

    private Guid[] WrapGuids(DateTimeOffset[] offsets)
    {
        var guids = new List<Guid>(offsets.Length);

        foreach (var offset in offsets)
        {
            guids.Add(Guid.CreateVersion7(offset));
        }

        return guids.ToArray();
    }

    private Fingerprint[] WrapFingerprints(Guid[] guids)
    {
        var fingerprints = new List<Fingerprint>(guids.Length);

        foreach (var guid in guids)
        {
            fingerprints.Add(new Fingerprint(guid));
        }

        return fingerprints.ToArray();
    }
}