using Microsoft.Extensions.DependencyInjection;
using Soenneker.Quark.Gen.SimpleIcons.BuildTasks.Abstract;

namespace Soenneker.Quark.Gen.SimpleIcons.BuildTasks;

/// <summary>
/// Represents the startup.
/// </summary>
public static class Startup
{
    /// <summary>
    /// Configures services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISimpleIconsWriteRunner, SimpleIconsWriteRunner>();
        services.AddHostedService<ConsoleHostedService>();
    }
}
