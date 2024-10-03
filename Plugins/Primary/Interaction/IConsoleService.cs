namespace Agience.Plugins.Primary.Interaction
{
    public interface IConsoleService
    {
        public Task<string?> ReadLineAsync();
        public Task WriteLineAsync(string message);
    }
}
