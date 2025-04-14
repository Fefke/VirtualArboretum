namespace VirtualArboretum.Presentation.Controllers;

public interface IServerController
{
    Task StartServerAsync();
    Task HandleCliCommand(string command);
    bool IsServerRunning();
}