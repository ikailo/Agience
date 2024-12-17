using CoreModel = Agience.Core.Models.Entities;

namespace Agience.Core.Interfaces
{
    public interface IAuthorityRecordsRepository
    {
        Task<CoreModel.Host?> GetHostById(string hostId);
        Task<IEnumerable<CoreModel.Agent>> GetAgentsForHostById(string hostId);
        //Task<IEnumerable<CoreModel.Plugin>> GetPluginsForHostById(string hostId);
        Task<string?> GetHostIdForAgentById(string agentId);
        Task<IEnumerable<CoreModel.Plugin>> SyncPluginsForHostById(string hostId, List<CoreModel.Plugin> plugins);
        Task<string> GetCredentialForAgentByName(string agentId, string credentialName);
    }
}
