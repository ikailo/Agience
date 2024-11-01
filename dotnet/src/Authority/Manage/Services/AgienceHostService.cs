using Agience.Authority.Models.Manage;

namespace Agience.Authority.Manage.Services
{
    public class AgienceHostService : IHostedService
    {
        private readonly AppConfig _appConfig;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ILogger<AgienceHostService> _logger;
        private readonly Core.Host _host;

        public AgienceHostService(
            Core.Host host, 
            AppConfig appConfig, 
            IHostApplicationLifetime appLifetime,
            ILogger<AgienceHostService> logger
            )
        {
            _host = host;
            _appConfig = appConfig;
            _appLifetime = appLifetime;            
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(async () =>
            {
                _logger.LogInformation("Starting Host");

                _host.AgentConnected += _host_AgentConnected;

                await _host.RunAsync();
                
            });

            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_host != null)
            {
                _logger.LogInformation("Stopping Agience Host");

                await _host.Stop();

                _logger.LogInformation("Agience Host Stopped");
            }
        }

        public void AddPluginFromType<T>(string name) where T : class
        {
            _host.AddPluginFromType<T>(name);
        }

        private Task _host_AgentConnected(Core.Agent agent)
        {
            _logger.LogInformation($"{agent.Name} Connected");

            return Task.CompletedTask;
        }
    }
}
