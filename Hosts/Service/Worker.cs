using Agience.SDK;

namespace Agience.Hosts.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private SDK.Host _host;        

        public Worker(SDK.Host host, ILogger<Worker> logger)
        {
            _logger = logger;
            _host = host;           
            _host.AgentConnected += _host_AgentConnected;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting Host");

            await _host.RunAsync();

            _logger.LogInformation("Host Stopped");
        }


        private Task _host_AgentConnected(Agent agent)
        {
            _logger.LogInformation($"{agent.Name} Ready");

            return Task.CompletedTask;
        }
    }
}
