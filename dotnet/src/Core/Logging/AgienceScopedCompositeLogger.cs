using Microsoft.Extensions.Logging;

namespace Agience.SDK.Logging
{
    public class AgienceScopedCompositeLogger<T> : AgienceScopedCompositeLoggerBase, ILogger<T>
    {
        public AgienceScopedCompositeLogger(List<ILogger> loggers, Dictionary<string, object>? scope = null)
            : base(loggers, scope) { }
    }

    public class AgienceScopedCompositeLogger : AgienceScopedCompositeLoggerBase
    {
        public AgienceScopedCompositeLogger(List<ILogger> loggers, Dictionary<string, object>? scope = null)
            : base(loggers, scope) { }
    }

    public abstract class AgienceScopedCompositeLoggerBase : ILogger
    {
        private readonly IEnumerable<ILogger> _loggers;
        private readonly Dictionary<string, object>? _scope;

        public AgienceScopedCompositeLoggerBase(IEnumerable<ILogger> loggers, Dictionary<string, object>? scope = null)
        {
            _loggers = loggers;
            _scope = scope;
        }

        private IDisposable? CurrentScope()
        {
            if (_scope == null || !_scope.Any())
            {
                return null;
            }

            return this.BeginScope(string.Join(", ", _scope.Select(kvp => $"{kvp.Key}:{kvp.Value}")));
        }

        IDisposable? ILogger.BeginScope<TState>(TState state)
        {
            var scopes = new List<IDisposable>();

            foreach (var logger in _loggers)
            {
                var scope = logger.BeginScope(state);

                if (scope != null)
                {
                    scopes.Add(scope);
                }
            }

            return new CompositeDisposable(scopes);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _loggers.Any(logger => logger.IsEnabled(logLevel));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            using (CurrentScope())
            {
                foreach (var logger in _loggers)
                {
                    if (logger.IsEnabled(logLevel))
                    {
                        logger.Log(logLevel, eventId, state, exception, formatter);
                    }
                }
            }
        }

        private class CompositeDisposable : IDisposable
        {
            private readonly IEnumerable<IDisposable> _disposables;

            public CompositeDisposable(IEnumerable<IDisposable> disposables)
            {
                _disposables = disposables;
            }

            public void Dispose()
            {
                foreach (var disposable in _disposables)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}