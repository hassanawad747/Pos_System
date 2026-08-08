using Microsoft.Extensions.DependencyInjection;

namespace BikeZonePOS.Modern;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        ServiceCollection services = new();
        services.AddSingleton<DatabaseConfiguration>();
        services.AddSingleton<DatabaseHealthService>();
        services.AddTransient<ModernShellForm>();

        using ServiceProvider provider = services.BuildServiceProvider();
        Application.Run(provider.GetRequiredService<ModernShellForm>());
    }
}
