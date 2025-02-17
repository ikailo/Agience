using Agience.Core;

namespace Agience.Plugins.Core.Interaction
{
    public interface IInteractionService
    {
        public Task<string?> SendToAgent(Agent receiver);
        public Task ReceiveFromAgent(Agent sender, string message);
        public Task Start();
    }
}
