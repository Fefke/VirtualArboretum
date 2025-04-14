using VirtualArboretum.Presentation.Controllers;
using VirtualArboretum.Presentation.Services;
using VirtualArboretum.Infrastructure.StaticResources;

namespace VirtualArboretum;
public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Any(arg => arg.Contains("help")))
        {
            // TODO: Add a list of all available commands
            
        }

        if (args.Length == 0 || args.Contains("serve"))
        {
            // Server-Mode
            var serverService = new ServerService();
            await serverService.RunServerAsync();
        }
        else
        {
            // Client-Mode - does require commands
            var pipeService = new PipeService();
            var serverService = new ServerService();
            var serverController = new ServerController(serverService);
            var inputController = new CliController(pipeService, serverController);

            await inputController.HandleInput(args);
        }
    }
}