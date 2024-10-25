namespace Agience.SDK.Logging
{
    public interface IAgienceEventLogHandler
    {
        public void OnLogEntryReceived(object? sender, AgienceEventLogArgs args);
    }
}
