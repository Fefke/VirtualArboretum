namespace VirtualArboretum.Presentation.Services;

public interface IServerService
{
    /// <summary>
    /// Does start HttpListener to accept http requests.
    /// </summary>
    Task RunServerAsync();
    bool ServerIsRunning();

    /// <summary>
    /// Does start server in background.
    /// </summary>
    void StartServerProcess();

    /// <summary>
    /// Does send command via http to server.
    /// </summary>
    Task SendCommandAsync(string command);
}
