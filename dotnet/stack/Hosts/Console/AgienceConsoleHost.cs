using Microsoft.Extensions.Logging;
using Agience.SDK;
using Host = Agience.SDK.Host;

namespace Agience.Hosts._Console
{
    public class AgienceConsoleHost
    {
        private readonly Host _host;
        private readonly ILogger<AgienceConsoleHost> _logger;
        private Agent? _contextAgent;

        public AgienceConsoleHost(Host host, ILogger<AgienceConsoleHost> logger)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _host.AgentConnected += _host_AgentConnected;
        }

        public async Task Run()
        {
            await _host.RunAsync();
        }

        private async Task _host_AgentConnected(Agent agent)
        {
            _logger.LogInformation($"{agent.Name} Ready");

            // TODO: Read the input and set the context agent. For now, we will just use the first agent.

            if (_contextAgent == null)
            {
                _contextAgent = agent;

                _logger.LogInformation($"* Switched context to {agent.Name ?? "Unknown"} *");

                await RunConsole();
            }
        }

        private async Task RunConsole()
        {
            // TODO: Add a way to switch context agents

            if (_contextAgent == null) throw new InvalidOperationException("No context agent set");

            string? userInput;
            Console.Write("User > ");

            while ((userInput = Console.ReadLine()) != null)
            {
                await _contextAgent.PromptAsync(userInput);
                                
                //Console.WriteLine($"{message}");

                Console.Write("User > ");
            }
        }
    }
}
