using Agience.SDK.Extensions;

namespace Agience.Hosts.Service
{
    internal class Program
    {
        private static ILogger<Program>? _logger;

        internal static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                logger.LogError($"\n\n Unhandled Exception occurred: {e.ExceptionObject}");
            };

            try
            {
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while running the application.");
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddUserSecrets<Program>();
                })
                .ConfigureServices((context, services) =>
                {
                    var config = context.Configuration.Get<AppConfig>() ?? new AppConfig();

                    if (string.IsNullOrWhiteSpace(config.AuthorityUri)) { throw new ArgumentNullException(nameof(config.AuthorityUri)); }
                    if (string.IsNullOrWhiteSpace(config.HostId)) { throw new ArgumentNullException(nameof(config.HostId)); }
                    if (string.IsNullOrWhiteSpace(config.HostSecret)) { throw new ArgumentNullException(nameof(config.HostSecret)); }

                    services.AddAgienceHost(config.AuthorityUri, config.HostId, config.HostSecret);
                    services.AddHostedService<Worker>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });
    }
}