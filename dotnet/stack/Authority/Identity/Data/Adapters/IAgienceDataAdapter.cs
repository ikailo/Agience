using Agience.Authority.Identity.Models;
using Microsoft.IdentityModel.Tokens;
using AgienceEntity = Agience.SDK.Models.Entities.AgienceEntity;

namespace Agience.Authority.Identity.Data.Adapters
{
    public interface IAgienceDataAdapter : SDK.Models.Entities.IAuthorityDataAdapter
    {
        /** PERSON SCOPED **/

        Task<IEnumerable<T>> GetRecordsAsPersonAsync<T>(string personId, bool includePublic = false) where T : AgienceEntity;        
        Task<T?> GetRecordByIdAsPersonAsync<T>(string recordId, string personId) where T : AgienceEntity;
        Task<T?> CreateRecordAsPersonAsync<T>(T record, string personId) where T : AgienceEntity;
        Task<T?> CreateRecordAsPersonAsync<T>(T record, string parentId, string personId) where T : AgienceEntity;
        Task UpdateRecordAsPersonAsync<T>(T record, string personId) where T : AgienceEntity;
        Task DeleteRecordAsPersonAsync<T>(string recordId, string personId) where T : AgienceEntity;
        Task<Key?> GenerateHostKeyAsPersonAsync(string hostId, string keyName, JsonWebKey? jsonWebKey, string personId);
        Task<IEnumerable<Plugin>> FindPluginsAsPersonAsync(string searchTerm, bool includePublic, string personId);
        Task AddPluginToHostAsPersonAsync(string hostId, string pluginId, string personId);
        Task RemovePluginFromHostAsPersonAsync(string hostId, string pluginId, string personId);
        Task AddPluginToAgentAsPersonAsync(string agentId, string pluginId, string personId);
        Task RemovePluginFromAgentAsPersonAsync(string agentId, string pluginId, string personId);        
        Task<IEnumerable<PluginConnection>> GetPluginConnectionsForAgentAsPersonAsync(string ageintId, string personId);        
        Task UpsertAgentConnectionAsPersonAsync(string agentId, string pluginConnectionId, string? authorizerId, string? credentialId , string personId);
        Task<AgentConnection?> GetAgentConnectionAsPersonAsync(string agentId, string pluginConnectionId, string personId);
        Task<IEnumerable<Function>> GetFunctionsForAgentAsPersonAsync(string agentId, string personId);

        /** SYSTEM SCOPED **/

        Task<T?> GetRecordByIdAsync<T>(string recordId) where T : AgienceEntity;
        Task<T?> CreateRecordAsync<T>(T record) where T : AgienceEntity;
        Task UpdateRecordAsync<T>(T record) where T : AgienceEntity;
        Task<Person?> GetPersonByExternalProviderIdAsync(string provider, string providerPersonId);
        Task<bool> VerifyHostSourceTargetRelationships(string hostId, string? sourceId, string? targetAgencyId, string? targetAgentId);
        
    }
}