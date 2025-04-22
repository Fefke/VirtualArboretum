using System.Diagnostics;
using VirtualArboretum.Presentation.Services;
using VirtualArboretum.Presentation.ViewModels;

namespace VirtualArboretum.Presentation.Controllers;

public class CliController
{
    private readonly IPipeService _pipeService;
    private readonly IServerController _serverController;

    public CliController(PipeService pipeService, IServerController serverController)
    {
        _pipeService = pipeService
                       ?? throw new ArgumentNullException(nameof(pipeService));

        _serverController = serverController
                            ?? throw new ArgumentNullException(nameof(serverController));
    }

    /// <summary>
    /// Handle different Pipe input
    /// </summary>
    public async Task HandleInput(string[] args)
    {
        if (args.Length > 0)
        {
            await _serverController.HandleCliCommand(string.Join(" ", args));
            return;
        }

        if (Console.IsInputRedirected)
        {
            var result = _pipeService.ReadPipedInput();

            if (result == null)
            {
                await Console.Error.WriteAsync("Pipe is somehow locked, cannot read input.");
            }
            else
            {
                await ProcessPipedInput(result.Type, result.Context);
            }
        }
    }

    private async Task ProcessPipedInput(MediaType mediaType, Stream content)
    {
        switch (mediaType)
        {
            case MediaType.Text:
                Console.Write(@"You did input some text..");
                break;
            default:
                Console.WriteLine(@"Cannot process your input stream. Please check with debugger.");
                break;
        }
    }
}