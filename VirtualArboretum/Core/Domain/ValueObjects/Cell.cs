namespace VirtualArboretum.Core.Domain.ValueObjects;

/// <summary>
/// A single cell, just like Jupyter-Notebook cells, can contain any kind of data,
/// meaning just raw data, active code, results of any kind, images... you name it.
/// </summary>
public class Cell
{

    public readonly Fingerprint UniqueMarker;

    public CellType Type { get; }
    public ReadOnlyMemory<byte> Organell { get; }

    public Cell(ReadOnlyMemory<byte> organell, CellType type, Fingerprint uniqueMarker)
    {
        UniqueMarker = uniqueMarker;
        Type = type;
        Organell = organell;
    }

    public Cell(ReadOnlyMemory<byte> organell, Fingerprint uniqueMarker)
        : this(organell, new CellType("application/octet-stream"), uniqueMarker)
    { }

    public Cell(ReadOnlyMemory<byte> organell, CellType type)
        : this(organell, type, new Fingerprint())
    { }

    public Cell(ReadOnlyMemory<byte> organell)
        : this(organell, new CellType("application/octet-stream"), new Fingerprint())
    { }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Cell);
    }

    public bool Equals(Cell? other)
    {
        return other != null
               && UniqueMarker == other.UniqueMarker
               && Equals(Type, other.Type)
               && Organell.Span.SequenceEqual(other.Organell.Span);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueMarker, Type, Organell);
    }
}