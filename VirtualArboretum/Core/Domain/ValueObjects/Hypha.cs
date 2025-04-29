using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using VirtualArboretum.Core.Domain.Entities;
using VirtualArboretum.Core.Domain.ValueObjects;

namespace VirtualArboretum.Core.Domain.ValueObjects;

/// <summary>
/// Represents a base Hypha, which is a list element by default.<br></br>
/// You specify Hypha (like HyphaApex) can implement a different Value,<br></br>
/// but should also provide an `AsValueType()` method.<br></br>
///
/// <b>Please Note</b>: In order to serialize and deserialize Hyphae,<br></br>
/// you have to extend HyphaeSerializer.ParseHyphaType(...)
/// and make sure your ToString() Method does not contain any reserved characters as per HyphaKey.
/// </summary>
public class Hypha  //: IParsable<Hypha>
{
    public HyphaKey Key { get; }

    /// <summary>
    /// Value of Hyphae, ⚠️ your specfic implementation
    /// should provide: <b>AsXYZ()</b> methods for specific values.
    /// As the Value is not exposed by default.
    /// ALSO remember to define ToString() for serialization
    /// and allow Creation of your hypha by providing a string.
    /// </summary>
    public object Value { get; init; }


    // Nested Label constructor.
    public Hypha(HyphaKey key, Hypha value)
    {
        Key = key;
        Value = value;
    }
    public Hypha(string key, Hypha value)
        : this(new HyphaKey(key), value) { }

    protected Hypha(HyphaKey key, object value)
    {
        Key = key;
        Value = value;
    }

    protected Hypha(string key, object value)
        : this(new HyphaKey(key), value) { }

    // Methods
    public bool DoesExtend()
    {
        return Value is Hypha;
    }

    public Hypha? NextExtension()
    {
        return Value as Hypha;
    }

    // Serialization methods.
    
    public override string ToString()
    {
        return $"{Key}{HyphaKey.ExtensionDelimiter}{Value}";
    }

    public override int GetHashCode()
    {
        return this.Key.GetHashCode();
    }

    public override bool Equals(object? other)
    {
        if (other is not Hypha otherHypha)
        {
            return false;
        }

        return otherHypha.Key.Equals(this.Key);
    }
}
