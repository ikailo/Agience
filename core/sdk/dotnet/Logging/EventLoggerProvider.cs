using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Agience.Core.Interfaces;

namespace Agience.Core.Logging
{
    public class EventLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly List<ILogger> _createdLoggers = new();

        public EventLoggerProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return NullLogger.Instance;
        }

        public ILogger CreateLogger(string categoryName, string? agentId)
        {
            return CreateLoggerInternal(typeof(object), agentId);
        }

        public ILogger<T> CreateLogger<T>(string? agentId)
        {
            return (ILogger<T>)CreateLoggerInternal(typeof(T), agentId);            
        }

        private ILogger CreateLoggerInternal(Type loggerType, string? agentId)
        {
            var logger = loggerType != typeof(object)
                ? (Activator.CreateInstance(typeof(EventLogger<>).MakeGenericType(loggerType), agentId)
                    ?? throw new InvalidOperationException("Failed to create logger instance."))
                : new AgienceEventLogger(agentId);

            if (logger is AgienceEventLoggerBase agienceEventLogger)
            {
                
                    var handlers = _serviceProvider.GetServices<IEventLogHandler>();

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