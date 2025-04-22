using VirtualArboretum.Core.Domain.Services;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretumTests.UnitTests.Core;

[TestClass]
public class TestHyphaeHierarchy
{
    public static Hypha BuildHyphaeOfThree()
    {
        return new Hypha(
            "Primary",
            new Hypha("Secondary",
                new HyphaApex("Tertiary")
            )
        );
    }

    [TestMethod]
    public void ShouldResolveTertiaryHyphaeHierarchy()
    {
        var delimiter = HyphaKey.ExtensionDelimiter;
        var hyphae = BuildHyphaeOfThree();

        var serializedHierarchy = HyphaeHierarchy.AsString(hyphae);

        Assert.AreEqual(
            $"Primary{delimiter}Secondary{delimiter}Tertiary",
            serializedHierarchy
            );
    }


    public static Hypha BuildHyphaeOfFourWithValue()
    {
        return new Hypha(
            "Primary",
            new Hypha("Secondary",
                new DecimalHypha("Tertiary",
                    4
                )
            )
            );
    }

    [TestMethod]
    public void ShouldResolveTertiaryHyphaeHierarchyWithValue()
    {
        var delimiter = HyphaKey.ExtensionDelimiter;
        var hyphae = BuildHyphaeOfFourWithValue();

        var serializedHierarchy = HyphaeHierarchy.AsString(hyphae);

        Assert.AreEqual(
            $"Primary{delimiter}Secondary{delimiter}Tertiary",
            serializedHierarchy
            );

        Assert.AreEqual(
            (hyphae
                .NextExtension()?
                .NextExtension() as DecimalHypha)?
            .AsDecimal(),
            4
            );
    }


 /*
  // TODO: Dont need StrongHyphae right now, just use Name fields  & Fingerprints.
  public static Hypha BuildStrongHyphaeOfFour()
    {
        return new Hypha(
            new StrongHyphaKey("Your"),
            new Hypha(
                new StrongHyphaKey("Unique"),
                new HyphaApex(
                    new StrongHyphaKey("Name")
                )
            )
        );
    }



    [TestMethod]
    public void ShouldResolveStrongHyphaeHierarchy()
    {
        var delimiter = '=';
        var hyphae = BuildStrongHyphaeOfFour();

        var serializedHierarchy = HyphaeHierarchy.AsString(hyphae);

        Assert.AreEqual(
            $"Your{delimiter}Unique{delimiter}Name",
            serializedHierarchy
        );
    }*/
}
