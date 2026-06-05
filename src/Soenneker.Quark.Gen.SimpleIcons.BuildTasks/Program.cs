using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Quark.Gen.SimpleIcons.BuildTasks;

/// <summary>
/// Represents the program.
/// </summary>
public sealed class Program
{
    private static CancellationTokenSource? _cts;

    /// <summary>
    /// Executes the main operation.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        _cts = new CancellationTokenSource();
        Console.CancelKeyPress += OnCancelKeyPress;

        try
        {
            await CreateHostBuilder(args).RunConsoleAsync(_cts.Token);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Stopped program because of exception: {e}");
            throw;
        }
        finally
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
            _cts.Dispose();
        }
    }

    /// <summary>
    /// Creates host builder.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>The result of the operation.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton(new BuildTasksCommandLineArgs(args));
                Startup.ConfigureServices(services);
            });
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
    {
        eventArgs.Cancel = true;
        _cts?.Cancel();
    }
}
