namespace VirtualArboretum.Core.Domain.ValueObjects;

public class HyphaApex : Hypha
{
    // definition of a tag is Name = Value as it is atomic.

    /// <summary>
    /// Does Represent a single atomic connection
    /// and ist just used to represent a connection between plants.
    /// Its just information, no value beside its implicit connection is stored.
    /// aka. Tag.
    /// </summary>
    public HyphaApex(HyphaKey name)
        : base(name, name) { }

    public HyphaApex(string name)
        : this(new HyphaKey(name)) { }

    public override string ToString()
    {
        return Key.ToString();
    }
}
