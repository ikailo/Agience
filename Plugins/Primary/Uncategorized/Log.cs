using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Primary.Uncategorized
{
    public class Log
    {
        //public override string[] InputKeys => new[] { "timestamp", "agent_id", "agent_name", "level", "message" };
        /*
        [KernelFunction, Description("Write entries to the Agency log.")]
        public Task<Data?> Process(Runner runner, Data? input = null)
        {
            var agentId = input?["agent_id"];
            var agentName = input?["agent_name"];
            var timestamp = input?["timestamp"];
            var level = input?["level"];
            var message = input?["message"];

            Console.WriteLine($"{timestamp} | default-{level} | {agentName} | {message}");
            
            return Task.FromResult<Data?>(null);
        }*/
    }
}