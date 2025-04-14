namespace VirtualArboretum.Core.Domain.ValueObjects;

public class DecimalHypha : Hypha
{

    /// <summary>
    /// This special Hyphae does have a decimal value as substrate.
    /// </summary>
    public DecimalHypha(string name, Decimal value)
        : base(name, value)
    { }

    public DecimalHypha(string name, int value)
        : this(name, new Decimal(value))
    { }

    public Decimal AsDecimal()
    {
        return (Decimal)Value;
        // yep, does not support as.
    }
}