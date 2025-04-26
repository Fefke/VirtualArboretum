using VirtualArboretum.Core.Application.Services;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.UnitTests.Infrastructure;

[TestClass]
public class TestHyphaSerializer
{
    [TestMethod]
    public void TestSerializationCycleOfHyphaeHierarchyWithValue()
    {
        // Arrange
        var delimiter = HyphaKey.ExtensionDelimiter;
        var serializedHyphae = $"{HyphaKey.StartMarker}keyA{delimiter}keyB{delimiter}value";

        // Act
        var hyphae = HyphaeSerializationService.Deserialize(serializedHyphae).First();
        var newSerializedHyphae = HyphaeSerializationService.Serialize(hyphae);

        // Assert
        Assert.IsNotNull(hyphae);
        Assert.AreEqual(serializedHyphae, newSerializedHyphae);
    }

    [TestMethod]
    public void TestSerializationCycleOfMultipleHyphae()
    {
        var delimiter = HyphaKey.ExtensionDelimiter;
        var validSerialHyphae = $"{HyphaKey.StartMarker}keyA{delimiter}keyB{delimiter}value" +
                                    $"{HyphaKey.StartMarker}keyC{delimiter}keyD{delimiter}value";
        var serialsToBeIgnored = " # ###  ";

        var allSerials = validSerialHyphae + serialsToBeIgnored;

        var hyphaeStrains = HyphaeSerializationService.Deserialize(allSerials);
        var newSerializedHyphae = HyphaeSerializationService.Serialize(hyphaeStrains);

        Assert.IsNotNull(hyphaeStrains);
        Assert.AreEqual(validSerialHyphae, newSerializedHyphae);
    }
}
