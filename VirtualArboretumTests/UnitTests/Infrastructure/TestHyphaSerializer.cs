using VirtualArboretum.Core.Domain.ValueObjects;
using VirtualArboretum.Infrastructure.Services;

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
        var hyphae = HyphaeSerializer.Deserialize(serializedHyphae);

        Assert.IsNotNull(hyphae);

        var newSerializedHyphae = HyphaeSerializer.Serialize(hyphae);

        // Assert
        Assert.AreEqual(serializedHyphae, newSerializedHyphae);

    }
}
