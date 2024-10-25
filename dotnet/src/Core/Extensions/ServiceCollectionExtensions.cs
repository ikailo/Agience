using Agience.SDK.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agience.SDK.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAgienceHost(
            this IServiceCollection services,
            string authorityUri,
            string hostId,
            string hostSecret,
            string? customNtpHost = null,
            string? authorityUriInternal = null,
            string? brokerUriInternal = null,
            string? hostOpenAiApiKey = null)
        {
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Broker>>();
                return new Broker(logger, customNtpHost);
            });

            services.AddSingleton(sp =>
            {
                var broker = sp.GetRequiredService<Broker>();
                var logger = sp.GetRequiredService<ILogger<Authority>>();
                return new Authority(authorityUri, broker, null, logger, authorityUriInternal, brokerUriInternal);
            });

            services.AddSingleton(sp =>
            {
                var authority = sp.GetRequiredService<Authority>();
                var broker = sp.GetRequiredService<Broker>();
                var logger = sp.GetRequiredService<ILogger<AgentFactory>>();
                return new AgentFactory(sp, authority, broker, logger, hostOpenAiApiKey);
            });

            services.AddSingleton(sp =>
            {
                var authority = sp.GetRequiredService<Authority>();
                var broker = sp.GetRequiredService<Broker>();
                var agentFactory = sp.GetRequiredService<AgentFactory>();
                var logger = sp.GetRequiredService<ILogger<Host>>();
                return new Host(hostId, hostSecret, authority, broker, agentFactory, logger);
            });
        }

        public static void AddAgienceAuthority(
            this IServiceCollection services,
            string authorityUri,
            string? customNtpHost = null,
            string? authorityUriInternal = null,
            string? brokerUriInternal = null)
        {
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Broker>>();
                return new Broker(logger, customNtpHost);
            });

            services.AddSingleton(sp =>
            {
                var broker = sp.GetRequiredService<Broker>();
                var dataAdapter = sp.GetRequiredService<IAuthorityDataAdapter>();
                var logger = sp.GetRequiredService<ILogger<Authority>>();
                return new Authority(authorityUri, broker, dataAdapter, logger, authorityUriInternal, brokerUriInternal);
            });
        }
    }
}