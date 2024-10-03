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
            string? hostOpenAiApiKey = null
            )
        {
            //services.AddSingleton<PluginRuntimeLoader>();
            services.AddSingleton(sp => new Broker(sp.GetRequiredService<ILogger<Broker>>(), customNtpHost));
            services.AddSingleton(sp => new Authority(authorityUri, sp.GetRequiredService<Broker>(), null, sp.GetRequiredService<ILogger<Authority>>(), authorityUriInternal, brokerUriInternal));
            services.AddSingleton(sp => new AgentFactory(sp, sp.GetRequiredService<Authority>(), sp.GetRequiredService<Broker>(), sp.GetRequiredService<ILogger<AgentFactory>>(), hostOpenAiApiKey));
            services.AddSingleton(sp => new Host(hostId, hostSecret, sp.GetRequiredService<Authority>(), sp.GetRequiredService<Broker>(), sp.GetRequiredService<AgentFactory>(), /*sp.GetRequiredService<PluginRuntimeLoader>(),*/ sp.GetRequiredService<ILogger<Host>>()));
        }

        public static void AddAgienceAuthority(
            this IServiceCollection services,
            string authorityUri,
            string? customNtpHost = null,
            string? authorityUriInternal = null,
            string? brokerUriInternal = null)
        {
            services.AddSingleton(sp => new Broker(sp.GetRequiredService<ILogger<Broker>>(), customNtpHost));
            services.AddSingleton(sp => new Authority(authorityUri, sp.GetRequiredService<Broker>(), sp.GetRequiredService<IAuthorityDataAdapter>(), sp.GetRequiredService<ILogger<Authority>>(), authorityUriInternal, brokerUriInternal));
        }


    }
}
