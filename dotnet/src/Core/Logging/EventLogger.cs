using Microsoft.Extensions.Logging;

namespace Agience.Core.Logging
{
    public class EventLogger<T> : AgienceEventLoggerBase, ILogger<T>
    {
        public EventLogger(string agentId)
            : base(agentId) { }
    }

    public class AgienceEventLogger : AgienceEventLoggerBase
    {
        public AgienceEventLogger(string agentId)
            : base(agentId) { }
    }

    public abstract class AgienceEventLoggerBase : ILogger
    {
        public event EventHandler<EventLogArgs>? LogEntryReceived;

        protected readonly string? _agentId;

        protected AgienceEventLoggerBase(string agentId)
        {
            _agentId = agentId;
        }

        IDisposable? ILogger.BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            LogEntryReceived?.Invoke(this, new EventLogArgs()
            {
                AgentId = _agentId,
                LogLevel = logLevel,
                EventId = eventId,
                State = state,
                Exception = exception,
                Formatter = (s, e) => formatter((TState)s, e),
            });
        }
    }
}
