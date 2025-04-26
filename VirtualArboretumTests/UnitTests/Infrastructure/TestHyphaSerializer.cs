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
        var hyphae = HyphaeSerializerService.Deserialize(serializedHyphae);

        Assert.IsNotNull(hyphae);

        var newSerializedHyphae = HyphaeSerializerService.Serialize(hyphae);

        // Assert
        Assert.AreEqual(serializedHyphae, newSerializedHyphae);

    }
}
