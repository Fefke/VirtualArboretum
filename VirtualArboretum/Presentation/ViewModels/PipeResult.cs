using System.Net.Http.Headers;

namespace VirtualArboretum.Presentation.ViewModels;

public record PipeResult(MediaType Type, MemoryStream Context);