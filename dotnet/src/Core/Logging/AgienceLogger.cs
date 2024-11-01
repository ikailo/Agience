using Microsoft.Extensions.Logging;

namespace Agience.Core.Logging
{
    public class AgienceLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly AsyncLocal<Scope?> _currentScope = new();

        public AgienceLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public Func<string, string, Task>? AgentLogEntryReceived { get; set; }
        public Func<string, string, Task>? AgencyLogEntryReceived { get; set; }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            var scope = new Scope(this, state);
            return scope;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var logMessage = formatter(state, exception);

            // Retrieve AgentId from the scope
            string? agentId = null;
            var scope = _currentScope.Value;
            while (scope != null)
            {
                if (scope.TryGetValue("AgentId", out var value))
                {
                    agentId = value as string;
                    break;
                }
                scope = scope.Parent;
            }

            // Log the message
            Console.WriteLine($"{logLevel}: {_categoryName} - {logMessage} {(agentId != null ? $"AgentId: {agentId}" : "")}");
        }

        private class Scope : IDisposable
        {
            private readonly AgienceLogger _logger;
            public Scope? Parent { get; }
            private readonly IDictionary<string, object> _state;

            public Scope(AgienceLogger logger, object state)
            {
                _logger = logger;
                Parent = _logger._currentScope.Value;
                _state = state as IDictionary<string, object> ?? new Dictionary<string, object>();
                _logger._currentScope.Value = this;
            }

            public bool TryGetValue(string key, out object? value)
            {
                if (_state.TryGetValue(key, out value))
                {
                    return true;
                }
                return Parent?.TryGetValue(key, out value) ?? false;
            }

            public void Dispose()
            {
                _logger._currentScope.Value = Parent;
            }
        }
    }
}
