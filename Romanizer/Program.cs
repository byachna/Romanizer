using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Romanizer;

class Program
{
    static void Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        //Setup Dependency Injection
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<MainApplication>();

        var serviceProvider = services.BuildServiceProvider();

        // Run the application
        var app = serviceProvider.GetRequiredService<MainApplication>();
        app.Run();
    }
}
