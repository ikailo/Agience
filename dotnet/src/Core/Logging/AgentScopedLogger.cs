using Microsoft.Extensions.Logging;

namespace Agience.Core.Logging
{
    public class AgentScopedLogger<T> : ILogger<T>, IDisposable
    {
        private readonly ILogger<T> _innerLogger;
        private readonly IDisposable _scope;

        public AgentScopedLogger(ILogger<T> innerLogger, string agentId)
        {
            _innerLogger = innerLogger;
            _scope = _innerLogger.BeginScope(new Dictionary<string, object> { { "AgentId", agentId } });
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Allow additional scopes to be added on top of the AgentId scope
            return _innerLogger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _innerLogger.IsEnabled(logLevel);
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _innerLogger.Log(logLevel, eventId, state, exception, formatter);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
