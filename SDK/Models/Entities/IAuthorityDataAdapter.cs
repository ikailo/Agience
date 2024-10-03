namespace Agience.SDK.Models.Entities
{
    public interface IAuthorityDataAdapter
    {
        Task<Host> GetHostByIdNoTrackingAsync(string hostId);
        Task<IEnumerable<Agent>> GetAgentsForHostIdNoTrackingAsync(string hostId);
        Task<IEnumerable<Plugin>> GetPluginsForHostIdNoTrackingAsync(string hostId);
        Task<string?> GetHostIdForAgentIdNoTrackingAsync(string agentId);
    }
}
