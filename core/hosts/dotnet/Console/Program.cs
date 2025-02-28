using Agience.Core.Extensions;
using Agience.Core.Interfaces;
using Agience.Plugins.Core.Interaction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Host = Agience.Core.Host;

namespace Agience.Hosts._Console
{
    internal class Program
    {
        private static ILogger<Program>? _logger;

        internal static async Task Main(string[] args)
        {
            var appBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

            appBuilder.Logging.ClearProviders();
            //appBuilder.Logging.AddConsole();

            appBuilder.Services.AddSingleton<InteractiveConsole>();
            appBuilder.Services.AddSingleton<IEventLogHandler>(sp => sp.GetRequiredService<InteractiveConsole>());
            appBuilder.Services.AddSingleton<IInteractionService>(sp => sp.GetRequiredService<InteractiveConsole>());            

            appBuilder.Configuration.AddUserSecrets<AppConfig>();

            var config = appBuilder.Configuration.Get<AppConfig>() ?? new AppConfig();

            if (string.IsNullOrWhiteSpace(config.AuthorityUri)) { throw new ArgumentNullException("AuthorityUri"); }
            if (string.IsNullOrWhiteSpace(config.HostId)) { throw new ArgumentNullException("HostId"); }
            if (string.IsNullOrWhiteSpace(config.HostSecret)) { throw new ArgumentNullException("HostSecret"); }

            appBuilder.Services.AddAgienceHostSingleton(config.AuthorityUri, config.HostId, config.HostSecret, config.CustomNtpHost, null, null);

            var app = appBuilder.Build();

            var host = app.Services.GetRequiredService<Host>();

#pragma warning disable SKEXP0050            
            host.AddPluginFromType<Plugins.Core.OpenAI.ChatCompletionPlugin>();
            host.AddPluginFromType<Plugins.Core.Interaction.InteractionPlugin>();
            host.AddPlugin(new Plugins.Core.Code.Git(config.WorkspacePath));
            //host.AddPlugin(new Plugins.Core.System.Files(config.WorkspacePath));

            host.AddPluginFromType<Microsoft.SemanticKernel.Plugins.Core.ConversationSummaryPlugin>();
            host.AddPluginFromType<Microsoft.SemanticKernel.Plugins.Core.FileIOPlugin>();
            host.AddPluginFromType<Microsoft.SemanticKernel.Plugins.Core.MathPlugin>();
            host.AddPluginFromType<Microsoft.SemanticKernel.Plugins.Core.TextPlugin>();
            host.AddPluginFromType<Microsoft.SemanticKernel.Plugins.Core.TimePlugin>();            
            host.AddPluginFromType<Microsoft.SemanticKernel.Plugins.Core.WaitPlugin>();

            

            //host.AddPluginFromType<Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.SessionsPythonPlugin>();
#pragma warning restore SKEXP0050

            await host.StartAsync();

            await app.Services.GetRequiredService<IInteractionService>().Start();

            //await app.RunAsync();
        }
    }
}