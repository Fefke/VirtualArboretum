namespace VirtualArboretum.Core.Domain.ValueObjects;

public class HyphaKey
{
    public static readonly char StartMarker = '#';
    public static readonly char ExtensionDelimiter = '-';
    // is effectively static! - but easily overridable in derived classes.
    // but has to be accessed through instance.

    // possible strong alternative `=`: #va=setting=some=thing
    // possible broad alternative `)`: #va)setting)some)thing

    // static for now, as referencing readonly in [] is not allowed in C# for some reason:
    public static readonly char[] ReservedChars = [StartMarker, ExtensionDelimiter];

    public string Value { get; init; }

    public HyphaKey(string value)
    {

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Key cannot be null, empty, or whitespace."
                );
        }

        if (ReservedChars.Any(value.Contains))
        {
            var reservedChars = string.Join("','", ReservedChars);
            throw new ArgumentException(
                $"Key cannot contain any reserved character: ['${reservedChars}']"
                );
        }

        Value = value;
    }

    public override string ToString() => Value;

    public virtual char GetExtensionDelimiter()
    {
        return ExtensionDelimiter;
    }

    public virtual char GetStartMarker()
    {
        return StartMarker;
    }


    public override bool Equals(object? other)
    {
        if (other is not HyphaKey otherHyphaKey)
        {
            return false;
        }

        return this.Value == otherHyphaKey.Value;

    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}