using VirtualArboretum.Presentation.Services;

namespace VirtualArboretum.Presentation.Controllers;

public class ServerController : IServerController
{
    private readonly IServerService _serverService;

    public ServerController(ServerService serverService)
    {
        _serverService = serverService
                         ?? throw new ArgumentNullException(nameof(serverService));
    }

    public async Task StartServerAsync()
    {
        await _serverService.RunServerAsync();
    }

    public async Task HandleCliCommand(string command)
    {
        // TODO: Use Interface for command Type (allow for Help & like run)
        for (var i = 0; !IsServerRunning() || i == 10  ; i++)
        {
            Console.WriteLine("Server is not running. Trying to start server...");
            _serverService.StartServerProcess();
            await Task.Delay(1000);
        }

        await _serverService.SendCommandAsync(command);
    }

    public bool IsServerRunning()
    {
        return _serverService.ServerIsRunning();
    }
}