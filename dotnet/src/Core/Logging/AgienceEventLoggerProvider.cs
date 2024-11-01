using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Agience.Core.Logging
{
    public class AgienceEventLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly List<ILogger> _createdLoggers = new();

        public AgienceEventLoggerProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return NullLogger.Instance;
        }

        public ILogger CreateLogger(string categoryName, string agencyId, string? agentId)
        {
            return CreateLoggerInternal(typeof(object), agencyId, agentId);
        }

        public ILogger<T> CreateLogger<T>(string agencyId, string? agentId)
        {
            return (ILogger<T>)CreateLoggerInternal(typeof(T), agencyId, agentId);            
        }

        private ILogger CreateLoggerInternal(Type loggerType, string agencyId, string? agentId)
        {
            var logger = loggerType != typeof(object)
                ? (Activator.CreateInstance(typeof(AgienceEventLogger<>).MakeGenericType(loggerType), agencyId, agentId)
                    ?? throw new InvalidOperationException("Failed to create logger instance."))
                : new AgienceEventLogger(agencyId, agentId);

            if (logger is AgienceEventLoggerBase agienceEventLogger)
            {
                
                    var handlers = _serviceProvider.GetServices<IAgienceEventLogHandler>();

                    foreach (var handler in handlers)
                    {
                        agienceEventLogger.LogEntryReceived += (sender, e) => handler?.OnLogEntryReceived(sender, e);
                    }
                
            }

            _createdLoggers.Add((ILogger)logger);

            return (ILogger)logger;
        }

        public void Dispose()
        {
            foreach (var logger in _createdLoggers)
            {
                if (logger is IDisposable disposableLogger)
                {
                    disposableLogger.Dispose();
                }
            }
            _createdLoggers.Clear();
        }
    }
}