using System.Net.Mime;

namespace VirtualArboretum.Core.Domain.ValueObjects;

public class CellType : ContentType
{
    public CellType(string mediaType) : base(mediaType)
    {
        if (!IsOfficiallySupported(this.MediaType))
        {
            Console.Error.WriteLine(
                $"Warning: {mediaType} is not an officially supported content type, " +
                $"will be handled as raw octet-stream."
            );
        }
    }

    public bool IsOfficiallySupported(string mediaType) => mediaType switch
    {
        "application/json" => true,
        "application/octet-stream" => true,
        "application/ld+json" => false,  // TODO: Once Core is stable.
        _ => false
    };
}