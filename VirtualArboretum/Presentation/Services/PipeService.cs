using VirtualArboretum.Presentation.ViewModels;

namespace VirtualArboretum.Presentation.Services;

public class PipeService : IPipeService
{
    public PipeResult? ReadPipedInput()
    {
        var resultStream = new MemoryStream();
        Console.OpenStandardInput().CopyTo(resultStream);
        resultStream.Position = 0;

        if (!resultStream.CanRead)
        {
            Console.Error.WriteLine("Error: Could not read from input stream.");
            return null;
        }

        var mediaType = DetectMediaType(resultStream);
        resultStream.Position = 0;

        return new PipeResult(mediaType, resultStream);
    }

    public MediaType DetectMediaType(Stream stream)
    {
        // TODO: Evaluate possible pattern (with debugger :)
        byte[] buffer = new byte[4];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        stream.Position = 0;

        if (bytesRead < 4) return MediaType.Text;

        if (buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46)
            return MediaType.PDF;

        if (buffer[0] == 0xFF && buffer[1] == 0xD8)
            return MediaType.JPEG;

        if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
            return MediaType.PNG;

        return MediaType.Text;
    }
}