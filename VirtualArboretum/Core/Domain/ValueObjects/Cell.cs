using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;

namespace VirtualArboretum.Core.Domain.ValueObjects;

/// <summary>
/// A single cell, just like Jupyter-Notebook cells, can contain any kind of data,
/// meaning just raw data, active code, results of any kind, images... you name it.
/// </summary>
public class Cell
{

    public readonly Fingerprint UniqueMarker;

    public ContentType Type { get; }
    public ReadOnlyMemory<byte> Organell { get; }

    public Cell(ReadOnlyMemory<byte> organell, ContentType type, Fingerprint uniqueMarker)
    {
        UniqueMarker = uniqueMarker;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Organell = organell; // ReadOnlyMemory<byte> ist ein Struct, keine Null-Prüfung nötig
    }
}