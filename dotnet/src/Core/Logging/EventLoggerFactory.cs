using Microsoft.Extensions.Logging;

namespace Agience.Core.Logging
{
    public class EventLoggerFactory : ILoggerFactory
    {
        private readonly List<ILoggerProvider> _providers = new();
        private readonly string? _agentId;

        public EventLoggerFactory(string? agentId)
        {
            _agentId = agentId;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return CreateScopedLogger<ILogger>(categoryName, _agentId);
        }

        public ILogger<T> CreateLogger<T>()
        {
            return CreateScopedLogger<ILogger<T>>(typeof(T).FullName ?? typeof(T).Name, _agentId);
        }

        private TLogger CreateScopedLogger<TLogger>(string categoryName, string? agentId) where TLogger : ILogger
        {
            var loggers = _providers.Select<ILoggerProvider, ILogger>(provider =>
                provider is EventLoggerProvider agienceProvider
                    ? agienceProvider.CreateLogger<TLogger>(agentId)
                    : (TLogger)provider.CreateLogger(categoryName)
            ).ToList();

            var scopeData = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(agentId))
            {
                scopeData["AgentId"] = agentId;
            }

            return (TLogger)Activator.CreateInstance(typeof(ScopedCompositeLogger<TLogger>), loggers, scopeData)!;
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
