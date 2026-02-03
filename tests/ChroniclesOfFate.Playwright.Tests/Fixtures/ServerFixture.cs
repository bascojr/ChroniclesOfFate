using System.Diagnostics;
using System.Net.Http;
using System.Net.Security;

namespace ChroniclesOfFate.Playwright.Tests.Fixtures;

/// <summary>
/// Manages the lifecycle of API and Blazor servers for Playwright tests.
/// Starts both servers before tests run and stops them after tests complete.
/// </summary>
public class ServerFixture : IDisposable
{
    private Process? _apiProcess;
    private Process? _blazorProcess;
    private bool _disposed;
    
    private static readonly string SolutionRoot = FindSolutionRoot();
    private static readonly string ApiProjectPath = Path.Combine(SolutionRoot, "src", "ChroniclesOfFate.API");
    private static readonly string BlazorProjectPath = Path.Combine(SolutionRoot, "src", "ChroniclesOfFate.Blazor");

    public bool IsApiRunning { get; private set; }
    public bool IsBlazorRunning { get; private set; }

    /// <summary>
    /// Starts both API and Blazor servers.
    /// </summary>
    public async Task StartServersAsync()
    {
        Console.WriteLine("Starting servers for Playwright tests...");
        
        // Start API server
        await StartApiServerAsync();
        
        // Start Blazor server
        await StartBlazorServerAsync();
        
        Console.WriteLine("Both servers started successfully!");
    }

    private async Task StartApiServerAsync()
    {
        Console.WriteLine($"Starting API server from: {ApiProjectPath}");
        
        // Check if API is already running
        if (await IsServerReadyAsync(TestConfig.ApiBaseUrl))
        {
            Console.WriteLine("API server is already running.");
            IsApiRunning = true;
            return;
        }

        _apiProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --no-build",
                WorkingDirectory = ApiProjectPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        _apiProcess.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"[API] {e.Data}");
        };
        _apiProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"[API ERROR] {e.Data}");
        };

        _apiProcess.Start();
        _apiProcess.BeginOutputReadLine();
        _apiProcess.BeginErrorReadLine();

        // Wait for API to be ready
        var apiReady = await WaitForServerAsync(TestConfig.ApiBaseUrl, TimeSpan.FromSeconds(60));
        if (!apiReady)
        {
            throw new Exception("API server failed to start within timeout period.");
        }

        IsApiRunning = true;
        Console.WriteLine("API server started successfully!");
    }

    private async Task StartBlazorServerAsync()
    {
        Console.WriteLine($"Starting Blazor server from: {BlazorProjectPath}");
        
        // Check if Blazor is already running
        if (await IsServerReadyAsync(TestConfig.BlazorBaseUrl))
        {
            Console.WriteLine("Blazor server is already running.");
            IsBlazorRunning = true;
            return;
        }

        _blazorProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --no-build",
                WorkingDirectory = BlazorProjectPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        _blazorProcess.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"[Blazor] {e.Data}");
        };
        _blazorProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine($"[Blazor ERROR] {e.Data}");
        };

        _blazorProcess.Start();
        _blazorProcess.BeginOutputReadLine();
        _blazorProcess.BeginErrorReadLine();

        // Wait for Blazor to be ready
        var blazorReady = await WaitForServerAsync(TestConfig.BlazorBaseUrl, TimeSpan.FromSeconds(60));
        if (!blazorReady)
        {
            throw new Exception("Blazor server failed to start within timeout period.");
        }

        IsBlazorRunning = true;
        Console.WriteLine("Blazor server started successfully!");
    }

    private static async Task<bool> WaitForServerAsync(string url, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        
        while (stopwatch.Elapsed < timeout)
        {
            if (await IsServerReadyAsync(url))
            {
                return true;
            }
            
            await Task.Delay(500);
        }

        return false;
    }

    private static async Task<bool> IsServerReadyAsync(string url)
    {
        try
        {
            using var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            using var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(5);
            
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
        }
        catch
        {
            return false;
        }
    }

    private static string FindSolutionRoot()
    {
        var directory = Directory.GetCurrentDirectory();
        
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory, "ChroniclesOfFate.sln")))
            {
                return directory;
            }
            directory = Directory.GetParent(directory)?.FullName;
        }
        
        // Fallback to relative path from test project
        var testDir = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", "..", ".."));
    }

    /// <summary>
    /// Stops all running servers.
    /// </summary>
    public void StopServers()
    {
        Console.WriteLine("Stopping servers...");
        
        StopProcess(_blazorProcess, "Blazor");
        StopProcess(_apiProcess, "API");
        
        Console.WriteLine("Servers stopped.");
    }

    private void StopProcess(Process? process, string name)
    {
        if (process == null || process.HasExited)
            return;

        try
        {
            Console.WriteLine($"Stopping {name} server (PID: {process.Id})...");
            
            // Try graceful shutdown first
            process.Kill(entireProcessTree: true);
            process.WaitForExit(5000);
            
            if (!process.HasExited)
            {
                process.Kill();
            }
            
            Console.WriteLine($"{name} server stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping {name} server: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopServers();
        
        _apiProcess?.Dispose();
        _blazorProcess?.Dispose();
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~ServerFixture()
    {
        Dispose();
    }
}
