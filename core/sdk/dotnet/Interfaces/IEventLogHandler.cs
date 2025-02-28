using Agience.Core.Logging;

namespace Agience.Core.Interfaces
{
    public interface IEventLogHandler
    {
        public void OnLogEntryReceived(object? sender, EventLogArgs args);
    }
}
