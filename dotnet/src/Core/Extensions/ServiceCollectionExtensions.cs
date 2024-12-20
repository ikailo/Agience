using Agience.Core.Logging;
using Agience.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Reflection.PortableExecutable;


namespace Agience.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAgienceHostSingleton(
            this IServiceCollection services,
            string authorityUri,
            string hostId,
            string hostSecret,
            string? customNtpHost = null,
            string? authorityUriInternal = null,
            string? brokerUriInternal = null)
        {
            //services.AddSingleton<IKernelStore, KernelStore>();

            services.AddSingleton(sp => new EventLoggerProvider(sp));

            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Broker>>();
                return new Broker(logger, customNtpHost);
            });

            services.AddSingleton(sp =>
            {
                var broker = sp.GetRequiredService<Broker>();
                var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var logger = sp.GetRequiredService<ILogger<Authority>>();

                return new Authority(authorityUri, broker, serviceScopeFactory, logger, authorityUriInternal, brokerUriInternal);
            });

            services.AddSingleton(sp =>
            {
                var authority = sp.GetRequiredService<Authority>();
                var broker = sp.GetRequiredService<Broker>();
                var agentFactory = sp.GetRequiredService<AgentFactory>();
                //var pluginRuntimeLoader = sp.GetRequiredService<PluginRuntimeLoader>();
                var logger = sp.GetRequiredService<ILogger<Host>>();
                return new Host(hostId, hostSecret, authority, broker, agentFactory, logger);
            });

            services.AddSingleton(sp =>
            {
                var broker = sp.GetRequiredService<Broker>();
                var authority = sp.GetRequiredService<Authority>();                
                var logger = sp.GetRequiredService<ILogger<AgentFactory>>();
                return new AgentFactory(sp, broker, authority, logger);
            });            
        }

        public static void AddAgienceAuthoritySingleton(
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
                var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var logger = sp.GetRequiredService<ILogger<Authority>>();

                return new Authority(authorityUri, broker, serviceScopeFactory, logger, authorityUriInternal, brokerUriInternal);
            });
        }
    }

}