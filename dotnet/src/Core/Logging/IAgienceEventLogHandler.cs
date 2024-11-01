namespace Agience.Core.Logging
{
    public interface IAgienceEventLogHandler
    {
        public void OnLogEntryReceived(object? sender, AgienceEventLogArgs args);
    }
}
