using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.IO;

namespace VirtualArboretum.Presentation.Services;

// Refactor-Stage: 🚦(orange)
public class ServerService : IServerService
{
    public const int Port = 7531;
    private const string ServerFlag = "--server";
    private const string Hostname = "localhost";

    // LockFile
    private static readonly string LockFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".VirtualArboretum"
        );
    private static readonly string LockFileName = ".va.server.lock";

    private static readonly string LockFile = Path.Combine(LockFilePath, LockFileName);


    /// <summary>
    /// Does test wheter server is already running, by trying to connect to http listener
    /// and does CleanupLockFile() if connection does fail after Timeout.
    /// </summary>
    public bool ServerIsRunning()
    {
        if (!File.Exists(LockFilePath)) return false;

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(720);
            var response = client.GetAsync($"http://{Hostname}:{Port}/status").Result;
            return response.IsSuccessStatusCode;
        }
        catch
        {
            CleanupLockFile();
        }

        return false;
    }

    private void CleanupLockFile()
    {
        try
        {
            if (File.Exists(LockFilePath))
            {
                File.Delete(LockFilePath);
            }
            ;
        }
        catch
        {
            Console.Error.Write("Unable to reset LockFile due to missing backend-server.");
        }
    }


    public void StartServerProcess()
    {
        string? executablePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (string.IsNullOrEmpty(executablePath)) return;

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            Arguments = ServerFlag,
            UseShellExecute = true,
            CreateNoWindow = true
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Server start error: {ex.Message}");
        }
    }


    public async Task SendCommandAsync(string command)
    {
        try
        {
            using var client = new HttpClient();
            // create Json object for message
            var commandObject = new { Command = command };
            var json = JsonSerializer.Serialize(commandObject);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"http://localhost:{Port}/command", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
            }
            else
            {
                Console.WriteLine($@"HTTP Error: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Command error: {ex.Message}");
        }
    }

    /// <summary>
    /// Does start HTTP server and enters RunHttpServerLoop.
    /// </summary>
    public async Task RunServerAsync()
    {

        if (ServerIsRunning())
        {
            Console.WriteLine(@"Server already running.");
            return;
        }

        Console.WriteLine(@"Server started");
        Directory.CreateDirectory(LockFilePath);

        await File.WriteAllTextAsync(
            LockFile,
            Process.GetCurrentProcess().Id.ToString()
            );

        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{Port}/");
        listener.Start();
        
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            await RunHttpServerLoop(listener, cts.Token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            // TODO: Write Exception to file as Server in background (fine with debugger tho :)
            await Console.Error.WriteLineAsync($"Server error: {ex.Message}");
        }
        finally
        {
            listener.Stop();
            CleanupLockFile();
        }
    }

    /// <summary>
    /// Intake for Http requests, which are being spawned in handler: HandleHttpRequestAsync().
    /// </summary>
    private async Task RunHttpServerLoop(HttpListener listener, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var contextTask = listener.GetContextAsync();

            var completedTask = await Task.WhenAny(
                contextTask, Task.Delay(Timeout.Infinite, cancellationToken)
                );

            if (cancellationToken.IsCancellationRequested)
                break;

            var context = await contextTask;
            _ = Task.Run(async () => await HandleHttpRequestAsync(context), cancellationToken);
        }
    }

    /// <summary>
    /// Handler does resolve all incoming requests according to their Http Option<br></br>
    /// and does route them to their according Method: `HandleXYZRequestAsync()` <br></br>
    /// and does return any http or json answer or a unified default error.
    /// </summary>
    private async Task HandleHttpRequestAsync(HttpListenerContext context)
    {
        try
        {
            string response;

            // Route Handling
            // TODO: Refactor into handler by Option > Resource
            if (context.Request.HttpMethod == "GET"
                && context.Request.Url?.AbsolutePath == "/status")
            {
                response = await HandleStatusRequestAsync();
            }
            else if (context.Request.HttpMethod == "POST"
                     && context.Request.Url?.AbsolutePath == "/command")
            {
                response = await HandleCommandRequestAsync(context.Request);
            }
            else
            {
                context.Response.StatusCode = 404;
                response = "Not Found";
            }

            // Send Answer
            var buffer = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.ContentType = "application/json";

            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            // TODO: Actual Logging when I care for it :)
            await Console.Error.WriteLineAsync($"Request handler error: {ex.Message}");
            context.Response.StatusCode = 500;
        }
        finally
        {
            context.Response.Close();
        }
    }


    private Task<string> HandleStatusRequestAsync()
    {
        if (!File.Exists(LockFilePath))
        {
            return Task.FromResult(JsonSerializer.Serialize(
                new
                {
                    Status = "Error",
                    Message = "Server status unknown (lock file not found)"
                }
            ));
        }

        DateTime startTime = File.GetCreationTime(LockFilePath);
        TimeSpan uptime = DateTime.Now - startTime;

        var statusInfo = new
        {
            Status = "Running",
            StartTime = startTime.ToString(CultureInfo.InvariantCulture),
            Uptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m"
        };

        return Task.FromResult(JsonSerializer.Serialize(statusInfo));
    }

    /// <summary>
    /// HTTP POST handler does handle such requests with a request body.
    /// </summary>
    private async Task<string> HandleCommandRequestAsync(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream);
        var requestBody = await reader.ReadToEndAsync();

        // TODO: Put JsonResponses somewhere else mybe..
        try
        {
            var commandData = JsonSerializer.Deserialize<CommandRequest>(requestBody);
            if (commandData == null || string.IsNullOrEmpty(commandData.Command))
            {
                return JsonSerializer.Serialize(
                    new
                    {
                        Status = "Error",
                        Message = "Empty command"
                    });
            }

            string commandResult = await ProcessCommandAsync(commandData.Command);
            return JsonSerializer.Serialize(
                new
                {
                    Status = "Success",
                    Message = commandResult
                });
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(
                new
                {
                    Status = "Error",
                    Message = "Invalid request format"
                });
        }
    }

    /// <summary>
    /// Does run valid Command on server...<br></br>
    /// Actual Commands running are being defined here for now.
    /// </summary>
    private async Task<string> ProcessCommandAsync(string command)
    {
        var args = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0) return "Empty command";

        return args[0].ToLower() switch
        {
            "exit" or "stop" => HandleExitCommand(),
            "process" => await HandleProcessCommand(args),
            "status" => $"Server running since {File.GetCreationTime(LockFilePath)}",
            _ => $"Unknown command: {args[0]}"
        };
    }

    private string HandleExitCommand()
    {
        Task.Run(() =>
        {
            Thread.Sleep(100); Environment.Exit(0);
        });
        return "Server shutting down...";
    }

    private async Task<string> HandleProcessCommand(string[] args)
    {
        if (args.Length <= 1) return "Error: No file path provided";
        if (!File.Exists(args[1])) return $"Error: File not found: {args[1]}";

        try
        {
            await using var stream = File.OpenRead(args[1]);
            var fileType = new PipeService().DetectMediaType(stream);
            await Task.Delay(100); // Placeholder
            return $"File processed: {args[1]} (Type: {fileType})";
        }
        catch (Exception ex)
        {
            return $"Processing error: {ex.Message}";
        }
    }

    /// <summary>
    /// Helper for deserialization of incoming commands...
    /// </summary>
    private record CommandRequest(string Command);
}
