using VirtualArboretum.Presentation.ViewModels;

namespace VirtualArboretum.Presentation.Services;

public interface IPipeService
{
    PipeResult? ReadPipedInput();
    
    // TODO:
    MediaType DetectMediaType(Stream stream);

}