namespace Agience.Core.Logging
{
    public interface IEventLogHandler
    {
        public void OnLogEntryReceived(object? sender, EventLogArgs args);
    }
}
