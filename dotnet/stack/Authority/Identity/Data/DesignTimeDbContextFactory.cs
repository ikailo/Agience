using DotNetEnv.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Agience.Authority.Identity.Data
{
    public class AgienceDbContextFactory : IDesignTimeDbContextFactory<AgienceDbContext>
    {
        const string DEFAULT_ENVIRONMENT_NAME = "local";

        public AgienceDbContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? DEFAULT_ENVIRONMENT_NAME;            

            IConfigurationRoot configuration = new ConfigurationBuilder()                
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddDotNetEnv($".env.{environmentName}")
                .AddEnvironmentVariables()
                .Build();
            
            var appConfig = new AppConfig();
            configuration.Bind(appConfig);

            Uri authorityDbUri = new Uri(appConfig.AuthorityDbUri ?? throw new ArgumentNullException("AuthorityDbUri"));

            var connectionString =
                $"Host={authorityDbUri.Host};" +
                $"Port={authorityDbUri.Port};" +
                $"Database={appConfig.AuthorityDbDatabase};" +
                $"Username={appConfig.AuthorityDbUsername};" +
                $"Password={appConfig.AuthorityDbPassword};" +
                authorityDbUri.Scheme == Uri.UriSchemeHttps ? $"SSL Mode=VerifyFull;" : string.Empty;

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