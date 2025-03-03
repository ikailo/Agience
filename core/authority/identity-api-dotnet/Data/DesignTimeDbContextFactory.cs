using DotNetEnv.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Agience.Authority.Identity.Data
{
    public class AgienceDbContextFactory : IDesignTimeDbContextFactory<AgienceDbContext>
    {
        public AgienceDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder();

            configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var envFilePath = Environment.GetEnvironmentVariable("ENV_FILE_PATH");

            if (envFilePath != null)
            {
                configuration.AddDotNetEnv(envFilePath);
            }
            else
            {
                configuration.AddEnvironmentVariables();
            }

            var config = configuration.Build();

            var appConfig = new AppConfig();
            config.Bind(appConfig);

            var connectionString =
                $"Host={appConfig.DatabaseHost};" +
                $"Port={appConfig.LanDatabasePort};" +
                $"Database={appConfig.DatabaseName};" +
                $"Username={appConfig.DatabaseUsername};" +
                $"Password={appConfig.DatabasePassword};" +
                $"SSL Mode=VerifyFull;";

            var optionsBuilder = new DbContextOptionsBuilder<AgienceDbContext>();
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseNpgsql(connectionString);

            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AgienceDbContext>();

            logger.LogInformation("Creating DbContext");
            logger.LogDebug($"Connection String: {connectionString}");

            return new AgienceDbContext(optionsBuilder.Options, logger);
        }
    }
}