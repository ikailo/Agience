using DotNetEnv;
using DotNetEnv.Configuration;

namespace Agience.Authority.Manage
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Env.Load($".env.{builder.Environment.EnvironmentName}");

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddDotNetEnv($".env.{builder.Environment.EnvironmentName}")
                .AddEnvironmentVariables();

            if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName.ToLower() == "debug")
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            var app = builder.ConfigureServices().Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                logger.LogError($"\n\n Unhandled Exception occurred: {e.ExceptionObject}");
            };

            try
            {
                app.ConfigurePipeline();

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while running the application.");
            }
            finally 
            {
                logger.LogInformation("Shutting down");
            }
        }
    }
}