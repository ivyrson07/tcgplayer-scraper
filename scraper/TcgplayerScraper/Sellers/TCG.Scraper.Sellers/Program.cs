using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TCG.Scraper.Common;
using TCG.Scraper.Sellers.Models;
using TCG.Scraper.Sellers.Services;

namespace TCG.Scraper.Sellers
{
    public class Program
    {
        private static ILogger<Program> _logger;
        private static IServiceProvider serviceProvider;
        private static IConfigurationRoot configurationProvider;
        private static IConfigurationBuilder configurationBuilder;

        public Program(ILogger<Program> logger)
        {
            _logger = logger;
        }

        static void Main(string[] args) => Run().GetAwaiter().GetResult();

        public static async Task Run()
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File("logs.txt")
                    .CreateLogger();

                configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();

                configurationProvider = configurationBuilder.Build();

                serviceProvider = new ServiceCollection()
                    .AddOptions()

                    .AddSingleton<IConfiguration>(configurationProvider)

                    .AddSingleton<IOrchestrator, Orchestrator>()
                    .AddSingleton<IProcessTimer, ProcessTimer>()

                    .AddHttpClient()

                    .AddLogging(configure => configure.AddConsole())
                    .AddLogging(configure => configure.AddDebug())
                    .AddLogging(configure => configure.AddSerilog(Log.Logger))

                    //.AddDbContext<DbContextClassHere>(options => options.UseSqlServer(configurationProvider.GetConnectionString("DbContextHere")))

                    .Configure<AppConfig>(options => configurationProvider.GetSection("AppConfig").Bind(options))

                    .BuildServiceProvider();

                await serviceProvider.GetService<IOrchestrator>().StartAsync(new CancellationToken());
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}