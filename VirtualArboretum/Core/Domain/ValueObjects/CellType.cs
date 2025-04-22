using System.Net.Mime;

namespace VirtualArboretum.Core.Domain.ValueObjects;

public class CellType : ContentType
{
    public CellType(string name) : base(name)
    {
        if (!IsOfficiallySupported())
        {
            Console.Error.WriteLine(
                $"Warning: {name} is not an officially supported content type, " +
                $"will be handled as raw octet-stream."
            );
        }
    }

    public bool IsOfficiallySupported() => this.Name switch
    {
        "application/json" => true,
        "application/octet-stream" => true,
        "application/ld+json" => false,  // TODO: Once Core is stable.
        _ => false
    };
}