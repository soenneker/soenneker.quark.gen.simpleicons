using Microsoft.Extensions.DependencyInjection;

namespace Soenneker.Quark.Gen.SimpleIcons.BuildTasks;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISimpleIconsWriteRunner, SimpleIconsWriteRunner>();
        services.AddHostedService<ConsoleHostedService>();
    }
}
