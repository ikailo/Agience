using Agience.Authority.Identity.Data;
using Serilog;
using Microsoft.EntityFrameworkCore;
using DotNetEnv.Configuration;

namespace Agience.Authority.Identity;
internal class Program
{
    private static async Task Main(string[] args)
    {
        //Log.Logger = new LoggerConfiguration()
            //.WriteTo.Console()
           // .CreateBootstrapLogger();

        Log.Information("Starting up");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            if (Environment.GetEnvironmentVariable("EF_MIGRATION")?.ToUpper() == "TRUE")
            {   
                return;
            }

            builder.Logging.ClearProviders();
            //builder.Logging.AddConsole();

            builder.Host.UseSerilog((ctx, lc) => lc
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(ctx.Configuration));

            builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            //.AddDotNetEnv($".env.{builder.Environment.EnvironmentName}")
            .AddEnvironmentVariables();

            if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName.ToLower() == "debug")
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            var app = builder.ConfigureServices();
            
            var appConfig = app.Services.GetRequiredService<AppConfig>();

            app.ConfigurePipeline(appConfig);

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var logger = services.GetService<ILogger<Program>>();

                try
                {
                    var dbContext = services.GetRequiredService<AgienceDbContext>();

                    dbContext.Database.Migrate();

                    var seedArgs = new Dictionary<string, string>();

                    // TODO: Use a Model. Move to some config or util class. Get from Configuration.

                    if (builder.Environment.EnvironmentName == "development" || builder.Environment.EnvironmentName == "debug")
                    {
                        seedArgs["web_domain"] = "localhost";
                        seedArgs["web_port"] = ":5002";
                        seedArgs["host_name"] = $"public.web.localhost";
                    }

                    else if (builder.Environment.EnvironmentName == "local")
                    {
                        seedArgs["web_domain"] = "web.local.agience.ai";
                        seedArgs["web_port"] = string.Empty;
                        seedArgs["host_name"] = $"public.web.local.agience.ai";
                    }

                    else if (builder.Environment.EnvironmentName == "preview")
                    {
                        seedArgs["web_domain"] = "web.preview.agience.ai";
                        seedArgs["web_port"] = string.Empty;
                        seedArgs["host_name"] = $"public.web.preview.agience.ai";
                    }

                    seedArgs["first_name"] = "Internal";
                    seedArgs["last_name"] = "User";
                    seedArgs["email"] = $"agience.internal.user@{seedArgs["web_domain"]}";
                    seedArgs["provider_id"] = "internal";
                    seedArgs["provider_person_id"] = "000000000000000";

                    logger?.LogInformation("Seeding database");

                    dbContext.SeedDatabase(services.GetRequiredService<AgienceIdProvider>(), seedArgs);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while migrating or initializing the database.");
                }
            }

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            Log.Information("Shut down complete");
            Log.CloseAndFlush();
        }
    }
}