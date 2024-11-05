namespace Agience.Plugins.Core.Interaction
{
    public interface IConsoleService
    {
        public Task<string?> ReadLineAsync();
        public Task WriteLineAsync(string message);
    }
}
