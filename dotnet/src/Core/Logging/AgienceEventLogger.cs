using Microsoft.Extensions.Logging;

namespace Agience.SDK.Logging
{
    public class AgienceEventLogger<T> : AgienceEventLoggerBase, ILogger<T>
    {
        public AgienceEventLogger(string agencyId, string? agentId = null)
            : base(agencyId, agentId) { }
    }

    public class AgienceEventLogger : AgienceEventLoggerBase
    {
        public AgienceEventLogger(string agencyId, string? agentId = null)
            : base(agencyId, agentId) { }
    }

    public abstract class AgienceEventLoggerBase : ILogger
    {
        public event EventHandler<AgienceEventLogArgs>? LogEntryReceived;

        protected readonly string _agencyId;
        protected readonly string? _agentId;

        protected AgienceEventLoggerBase(string agencyId, string? agentId = null)
        {
            _agencyId = agencyId;
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

            LogEntryReceived?.Invoke(this, new AgienceEventLogArgs()
            {
                AgencyId = _agencyId,
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
