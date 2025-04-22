using System.Net.Mime;

namespace VirtualArboretum.Core.Domain.ValueObjects;

public class CellType : ContentType
{
    public CellType(string name) : base(name)
    { }

    public bool IsOfficiallySupported() => this.Name switch
    {
        "application/json" => true,
        "application/octet-stream" => true,
        _ => false
    };
}