using Microsoft.Extensions.Logging;

namespace Agience.SDK.Logging
{
    public class AgienceEventLoggerFactory : ILoggerFactory
    {
        private readonly List<ILoggerProvider> _providers = new();
        private readonly string _agencyId;
        private readonly string? _agentId;

        public AgienceEventLoggerFactory(string agencyId, string? agentId)
        {
            _agencyId = agencyId;
            _agentId = agentId;
        }

        public ILogger CreateLogger(string categoryName)
        {
            string? agentId = IsAgencyLogger(categoryName) ? null : _agentId;
            return CreateScopedLogger<ILogger>(categoryName, agentId);
        }

        public ILogger<T> CreateLogger<T>()
        {
            string? agentId = typeof(T) == typeof(Agency) ? null : _agentId;
            return CreateScopedLogger<ILogger<T>>(typeof(T).FullName ?? typeof(T).Name, agentId);
        }

        private TLogger CreateScopedLogger<TLogger>(string categoryName, string? agentId) where TLogger : ILogger
        {
            var loggers = _providers.Select<ILoggerProvider, ILogger>(provider =>
                provider is AgienceEventLoggerProvider agienceProvider
                    ? agienceProvider.CreateLogger<TLogger>(_agencyId, agentId)
                    : (TLogger)provider.CreateLogger(categoryName)
            ).ToList();

            var scopeData = new Dictionary<string, object> { { "AgencyId", _agencyId } };

            if (!string.IsNullOrEmpty(agentId))
            {
                scopeData["AgentId"] = agentId;
            }

            return (TLogger)Activator.CreateInstance(typeof(AgienceScopedCompositeLogger<TLogger>), loggers, scopeData)!;
        }

        private bool IsAgencyLogger(string categoryName)
        {
            var agencyFullName = typeof(Agency).FullName ?? typeof(Agency).Name;
            return agencyFullName == categoryName;
        }

        public void AddProvider(ILoggerProvider provider)
        {
            _providers.Add(provider);
        }

        public void Dispose()
        {
            foreach (var provider in _providers)
            {
                provider.Dispose();
            }
            _providers.Clear();
        }
    }
}
