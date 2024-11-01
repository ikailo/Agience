using AutoMapper;
using Agience.Authority.Identity.Models;
using Host = Agience.Authority.Identity.Models.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Agience.Authority.Identity.Validators;
using System.Text.Json;


namespace Agience.Authority.Identity.Data.Adapters
{
    public class AgienceDataAdapter : IAgienceDataAdapter, Core.Models.Entities.IAuthorityDataAdapter
    {
        private readonly AgienceDbContext _context;
        private readonly AgienceIdProvider _idProvider;
        private readonly IMapper _mapper;
        private readonly ILogger<AgienceDataAdapter> _logger;

        public AgienceDataAdapter(AgienceDbContext context, AgienceIdProvider idProvider, IMapper mapper, ILogger<AgienceDataAdapter> logger)
        {
            _context = context;
            _idProvider = idProvider;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetRecordsAsPersonAsync<T>(string personId, bool includePublic = false) where T : Core.Models.Entities.AgienceEntity
        {
            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            if (typeof(T) == typeof(Agency))
            {
                return await _context.Set<Agency>().Include(a => a.Agents).Where(a => a.DirectorId == personId).ToListAsync() as IEnumerable<T> ?? [];
            }

            else if (typeof(T) == typeof(Agent))
            {
                return await _context.Set<Agent>()
                    .Include(a => a.Agency)
                    .Include(a => a.Connections)
                        .ThenInclude(c => c.Authorizer) // TODO: SECURITY: Handle clientSecret correctly
                    .Include(a => a.Connections)
                        .ThenInclude(c => c.PluginConnection)
                    .Where(a => a.Agency != null && a.Agency.DirectorId == personId)
                    .ToListAsync() as IEnumerable<T> ?? [];
            }

            else if (typeof(T) == typeof(Authorizer))
            {
                return await _context.Set<Authorizer>().Where(a => a.ManagerId == personId).ToListAsync() as IEnumerable<T> ?? [];
            }

            else if (typeof(T) == typeof(PluginConnection))
            {
                return await _context.Set<PluginConnection>().Include(c => c.Plugin).Where(c => c.Plugin.CreatorId == personId).ToListAsync() as IEnumerable<T> ?? [];
            }

            else if (typeof(T) == typeof(Function))
            {
                return await _context.Set<Function>().Where(f => f.PluginFunctions.Any(pf => pf.IsRoot && pf.Plugin != null && pf.Plugin.CreatorId == personId)).ToListAsync() as IEnumerable<T> ?? [];
            }

            else if (typeof(T) == typeof(Host))
            {
                if (includePublic)
                {
                    return await _context.Set<Host>().Where(h => h.Visibility == Core.Models.Entities.Visibility.Public || h.OperatorId == personId).ToListAsync() as IEnumerable<T> ?? [];
                }
                else
                {
                    return await _context.Set<Host>().Include(h => h.Keys).Include(h => h.Plugins).Where(h => h.OperatorId == personId).ToListAsync() as IEnumerable<T> ?? []; // TODO: SECURITY: Confirm correct Key secrets handling.
                }
                
            }

            else if (typeof(T) == typeof(Key))
            {
                return await _context.Set<Key>().Include(k => k.Host).Where(k => k.Host != null && k.Host.OperatorId == personId).ToListAsync() as IEnumerable<T> ?? [];
            }

            else if (typeof(T) == typeof(Person))
            {
                return await _context.Set<Person>().Where(p => p.Id == personId).ToListAsync() as IEnumerable<T> ?? []; // NOTE: This returns only one value.
            }

            else if (typeof(T) == typeof(Plugin))
            {
                return await _context.Plugins.Include(p => p.Connections).Include(p => p.PluginFunctions).ThenInclude(pf => pf.Function).Where(p => p.CreatorId == personId).ToListAsync() as IEnumerable<T> ?? [];
            }
            else if (typeof(T) == typeof(Log))
            {
                return await _context.Logs.Include(p => p.Agent).Where(l=>l.AgentId==personId).OrderByDescending(lg=>lg.CreatedDate).ToListAsync() as IEnumerable<T> ?? [];
            }

            throw new InvalidOperationException("unsupported type");
        }

        public async Task<T?> GetRecordByIdAsPersonAsync<T>(string recordId, string personId) where T : Core.Models.Entities.AgienceEntity
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                throw new InvalidOperationException("recordId is invalid");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            if (typeof(T) == typeof(Agency))
            {
                return await _context.Set<Agency>().Include(a => a.Agents).FirstOrDefaultAsync(a => a.Id == recordId && a.DirectorId == personId) as T;
            }

            else if (typeof(T) == typeof(Agent))
            {
                return await _context.Set<Agent>().Include(a => a.Agency).FirstOrDefaultAsync(a => a.Id == recordId && a.Agency != null && a.Agency.DirectorId == personId) as T;
            }

            else if (typeof(T) == typeof(AgentConnection))
            {
                return await _context.Set<AgentConnection>().Include(ac => ac.Agent).ThenInclude(a => a.Agency).FirstOrDefaultAsync(a => a.Id == recordId && a.Agent != null && a.Agent.Agency != null && a.Agent.Agency.DirectorId == personId) as T;
            }

            else if (typeof(T) == typeof(Authorizer))
            {
                return await _context.Set<Authorizer>().FirstOrDefaultAsync(a => a.Id == recordId && a.ManagerId == personId) as T;
            }

            else if (typeof(T) == typeof(PluginConnection))
            {
                return await _context.Set<PluginConnection>().Include(c => c.Plugin).FirstOrDefaultAsync(c => c.Id == recordId && c.Plugin.CreatorId == personId) as T;
            }

            else if (typeof(T) == typeof(Function))
            {
                return await _context.Functions.Where(f => f.Id == recordId && f.PluginFunctions.Any(pf => pf.IsRoot && pf.Plugin != null && pf.Plugin.CreatorId == personId)).FirstOrDefaultAsync() as T;
            }

            else if (typeof(T) == typeof(Host))
            {
                return await _context.Set<Host>().FirstOrDefaultAsync(h => h.Id == recordId && h.OperatorId == personId) as T;
            }

            else if (typeof(T) == typeof(Key))
            {
                return await _context.Set<Key>().Include(k => k.Host).FirstOrDefaultAsync(k => k.Id == recordId && k.Host != null && k.Host.OperatorId == personId) as T;
            }

            else if (typeof(T) == typeof(Person))
            {
                return recordId == personId ? await _context.Set<Person>().FindAsync(recordId) as T : null;
            }

            else if (typeof(T) == typeof(Plugin))
            {
                return await _context.Set<Plugin>().FirstOrDefaultAsync(p => p.Id == recordId && p.CreatorId == personId) as T;
            }

            throw new InvalidOperationException("unsupported type");
        }

        public async Task<T?> CreateRecordAsPersonAsync<T>(T record, string parentId, string personId) where T : Core.Models.Entities.AgienceEntity
        {
            if (record == null)
            {
                throw new InvalidOperationException("record is null");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            if (string.IsNullOrWhiteSpace(parentId))
            {
                throw new InvalidOperationException("parentId is invalid");
            }

            if (!string.IsNullOrEmpty(record.Id))
            {
                throw new InvalidOperationException("recordId must not be set");
            }

            if (record is PluginConnection connection)
            {
                var plugin = await GetRecordByIdAsPersonAsync<Plugin>(parentId, personId);

                if (plugin == null)
                {
                    throw new InvalidOperationException("plugin not found");
                }

                connection.Id = _idProvider.GenerateId(typeof(T).Name);

                connection.PluginId = plugin.Id;

                _context.Set<PluginConnection>().Add(_mapper.Map<PluginConnection>(connection));

            }

            else if (record is Function function)
            {
                var plugin = await GetRecordByIdAsPersonAsync<Plugin>(parentId, personId);

                if (plugin == null)
                {
                    throw new InvalidOperationException("plugin not found");
                }

                function.Id = _idProvider.GenerateId(typeof(T).Name);

                _context.Set<Function>().Add(_mapper.Map<Function>(function));

                var pluginFunction = new PluginFunction
                {
                    PluginId = plugin.Id,
                    FunctionId = function.Id,
                    IsRoot = true
                };

                _context.PluginFunctions.Add(pluginFunction);
            }

            else
            {
                throw new InvalidOperationException("unsupported type");
            }

            await _context.SaveChangesAsync();

            return record;

        }

        public async Task<T?> CreateRecordAsPersonAsync<T>(T record, string personId) where T : Core.Models.Entities.AgienceEntity
        {
            if (record == null)
            {
                throw new InvalidOperationException("record is null");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            if (!string.IsNullOrEmpty(record.Id))
            {
                throw new InvalidOperationException("recordId must not be set");
            }

            if (record is Agency agency)
            {
                agency.DirectorId = string.IsNullOrEmpty(agency.DirectorId) ? personId : throw new InvalidOperationException("DirectorId must not be set");
            }

            else if (record is Agent agent)
            {

                if (string.IsNullOrWhiteSpace(agent.AgencyId))
                {
                    throw new InvalidOperationException("agencyId is invalid");
                }

                if (await GetRecordByIdAsPersonAsync<Agency>(agent.AgencyId, personId) == null)
                {
                    throw new InvalidOperationException("agency not found");
                }
            }

            else if (record is AgentConnection agentConnection)
            {
                if (string.IsNullOrWhiteSpace(agentConnection.AgentId))
                {
                    throw new InvalidOperationException("agentId is invalid");
                }

                if (string.IsNullOrWhiteSpace(agentConnection.PluginConnectionId))
                {
                    throw new InvalidOperationException("pluginConnectionId is invalid");
                }

                if (await GetRecordByIdAsPersonAsync<Agent>(agentConnection.AgentId, personId) == null)
                {
                    throw new InvalidOperationException("agent not found");
                }

                var pluginConnection = await _context.PluginConnections.FirstOrDefaultAsync(pc => pc.Id == agentConnection.PluginConnectionId && (pc.Plugin.CreatorId == personId || pc.Plugin.Visibility == Core.Models.Entities.Visibility.Public));

                if (pluginConnection == null)
                {
                    throw new InvalidOperationException("pluginConnection not found");
                }
            }

            else if (record is Authorizer authorizer)
            {
                authorizer.ManagerId = string.IsNullOrEmpty(authorizer.ManagerId) ? personId : throw new InvalidOperationException("ManagerId must not be set");
            }

            else if (record is Credential credential)
            {
                // nothing to check
            }                       

            else if (record is Function)
            {
                throw new InvalidOperationException("pluginId must be provided");
            }

            else if (record is Host host)
            {
                host.OperatorId = string.IsNullOrEmpty(host.OperatorId) ? personId : throw new InvalidOperationException("OperatorId must not be set");
            }

            else if (record is Key key)
            {
                if (string.IsNullOrWhiteSpace(key.HostId))
                {
                    throw new InvalidOperationException("hostId is invalid");
                }

                if (await GetRecordByIdAsPersonAsync<Host>(key.HostId, personId) == null)
                {
                    throw new InvalidOperationException("host not found");
                }
            }

            else if (record is Person)
            {
                throw new InvalidOperationException("Person records cannot be created through the Manage API.");
            }

            else if (record is Plugin plugin)
            {
                plugin.CreatorId = string.IsNullOrEmpty(plugin.CreatorId) ? personId : throw new InvalidOperationException("CreatorId must not be set");
            }

            else if (record is PluginConnection connection)
            {
                throw new NotImplementedException();
            }

            else if (record is Log log)
            {
                // nothing to check
            }

            else
            {
                throw new InvalidOperationException("unsupported type");
            }

            record.Id = _idProvider.GenerateId(typeof(T).Name);

            _context.Set<T>().Add(record);

            await _context.SaveChangesAsync();

            return record;
        }

        public async Task UpdateRecordAsPersonAsync<T>(T newRecord, string personId) where T : Core.Models.Entities.AgienceEntity
        {
            if (newRecord == null)
            {
                throw new InvalidOperationException("record is null");
            }

            if (string.IsNullOrWhiteSpace(newRecord.Id))
            {
                throw new InvalidOperationException("recordId is invalid");
            }

            var oldRecord = await GetRecordByIdAsPersonAsync<T>(newRecord.Id, personId);

            // TODO: Here we need to check if the person has the right to update the record, not just query it.

            if (oldRecord == null) // Currently we only return the record if the person owns it. Maybe we should change the name to GetRecordByIdAsOwnerAsync.
            {
                throw new InvalidOperationException("record not found");
            }

            if (newRecord is Agency newAgency && oldRecord is Agency oldAgency)
            {
                newAgency.DirectorId = oldAgency.DirectorId;
            }

            else if (newRecord is Agent newAgent && oldRecord is Agent oldAgent)
            {
                newAgent.AgencyId = oldAgent.AgencyId;                
            }

            else if (newRecord is AgentConnection newAgentConnection && oldRecord is AgentConnection oldAgentConnection)
            {
                if (newAgentConnection.AgentId != null && newAgentConnection.AgentId != oldAgentConnection.AgentId)
                {
                    throw new InvalidOperationException("AgentId cannot be changed");
                }
                if (newAgentConnection.PluginConnectionId != null && newAgentConnection.PluginConnectionId != oldAgentConnection.PluginConnectionId)
                {
                    throw new InvalidOperationException("PluginConnectionId cannot be changed");
                }

                newAgentConnection.AgentId = oldAgentConnection.AgentId;
                newAgentConnection.PluginConnectionId = oldAgentConnection.PluginConnectionId;
            }

            else if (newRecord is Authorizer newAuthorizer && oldRecord is Authorizer oldAuthorizer)
            {
                newAuthorizer.ManagerId = oldAuthorizer.ManagerId;
            }

            else if (newRecord is PluginConnection newConnection && oldRecord is PluginConnection oldConnection)
            {
                newConnection.PluginId = oldConnection.PluginId;
            }

            else if (newRecord is Function newFunction && oldRecord is Function oldFunction)
            {
                // Nothing to update
            }

            else if (newRecord is Host newHost && oldRecord is Host oldHost)
            {
                newHost.OperatorId = oldHost.OperatorId;
            }

            else if (newRecord is Key newKey && oldRecord is Key oldKey)
            {
                newKey.HostId = oldKey.HostId;
                newKey.CreatedDate = oldKey.CreatedDate;
                newKey.SaltedValue = oldKey.SaltedValue;
            }

            else if (newRecord is Person newPerson && oldRecord is Person oldPerson)
            {
                throw new NotImplementedException();
            }

            else if (newRecord is Plugin newPlugin && oldRecord is Plugin oldPlugin)
            {
                newPlugin.CreatorId = oldPlugin.CreatorId;
            }

            else
            {
                throw new InvalidOperationException("unsupported type");
            }

            _context.Entry(oldRecord).CurrentValues.SetValues(newRecord);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecordAsPersonAsync<T>(string recordId, string personId) where T : Core.Models.Entities.AgienceEntity
        {
            if (string.IsNullOrWhiteSpace(recordId))
            {
                throw new InvalidOperationException("recordId is invalid");
            }

            var record = await GetRecordByIdAsPersonAsync<T>(recordId, personId);

            if (record == null)
            {
                throw new InvalidOperationException("record not found");
            }

            if (record is Function function)
            {
                // Cascade delete
                // TODO: Handle scenarios where a function is shared with other plugins that aren't owned by the person.
                _context.PluginFunctions.RemoveRange(await _context.PluginFunctions.Where(pf => pf.FunctionId == function.Id).ToListAsync());

            }

            _context.Set<T>().Remove(record);

            await _context.SaveChangesAsync();
        }

        public async Task<Key?> GenerateHostKeyAsPersonAsync(string hostId, string name, JsonWebKey? jsonWebKey, string personId)
        {

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            if (string.IsNullOrWhiteSpace(hostId))
            {
                throw new InvalidOperationException("hostId is invalid");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Name is required");
            }

            var host = await GetRecordByIdAsPersonAsync<Host>(hostId, personId);

            if (host == null)
            {
                throw new InvalidOperationException("host not found");
            }

            var secret = HostSecretValidator.Random32ByteString();

            var record = new Key
            {
                Name = name,
                HostId = hostId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                SaltedValue = HostSecretValidator.HashSecret(secret, HostSecretValidator.Random32ByteString())
            };

            record = await CreateRecordAsync(record);

            if (string.IsNullOrWhiteSpace(record?.Id))
            {
                throw new InvalidDataException("record not created");
            }

            if (jsonWebKey == null)
            {
                record.Value = secret;
                record.IsEncrypted = false;
            }
            else
            {
                record.Value = HostSecretValidator.EncryptWithJsonWebKey(secret, jsonWebKey);
                record.IsEncrypted = true;
            }

            return record;
        }

        public async Task<IEnumerable<Plugin>> FindPluginsAsPersonAsync(string searchTerm, bool includePublic, string personId)
        {
            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 3)
            {
                throw new InvalidOperationException("searchTerm is invalid");
            }

            // Full-text search query for the description
            //var fullTextQuery = _context.Plugins.Where(p => p.DescriptionSearchVector.Matches(searchTerm));

            // Trigram similarity search query for the name
            var trigramQuery = _context.Plugins.Where(p => !string.IsNullOrEmpty(p.Name) && EF.Functions.TrigramsSimilarity(p.Name, searchTerm) > 0.3);


            var queryResults = await trigramQuery
                //await fullTextQuery
                //.Union(trigramQuery)                
                //.Where(p => p.CreatorId == personId || (includePublic && p.Visibility == Core.Models.Entities.Visibility.Public))
                //.OrderByDescending(p => EF.Functions.TrigramsSimilarity(p.Name ?? string.Empty, searchTerm)
                //                                      + p.DescriptionSearchVector.Rank(EF.Functions.ToTsQuery("english", searchTerm))
                //                  )
                .ToListAsync();

            _logger.LogInformation("FindPluginsAsPersonAsync: Found {0} plugins", queryResults.Count);

            return queryResults;
        }


        public async Task AddPluginToHostAsPersonAsync(string hostId, string pluginId, string personId)
        {
            if (string.IsNullOrWhiteSpace(hostId))
            {
                throw new InvalidOperationException("hostId is invalid");
            }

            if (string.IsNullOrWhiteSpace(pluginId))
            {
                throw new InvalidOperationException("pluginId is invalid");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            var host = await GetRecordByIdAsPersonAsync<Host>(hostId, personId);

            if (host == null)
            {
                throw new InvalidOperationException("Host not found");
            }

            var plugin = await _context.Plugins
                .FirstOrDefaultAsync(p => p.Id == pluginId && (p.CreatorId == personId || p.Visibility == Core.Models.Entities.Visibility.Public));

            if (plugin == null)
            {
                throw new InvalidOperationException("Plugin not found");
            }

            var hostPlugin = new HostPlugin
            {
                HostId = hostId,
                PluginId = pluginId
            };

            _context.HostPlugins.Add(hostPlugin);

            await _context.SaveChangesAsync();
        }

        public async Task RemovePluginFromHostAsPersonAsync(string hostId, string pluginId, string personId)
        {
            if (string.IsNullOrWhiteSpace(hostId))
            {
                throw new InvalidOperationException("hostId is invalid");
            }

            if (string.IsNullOrWhiteSpace(pluginId))
            {
                throw new InvalidOperationException("pluginId is invalid");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            var hostPlugin = await _context.HostPlugins.Include(hp => hp.Host)
                .FirstOrDefaultAsync(hp =>
                    hp.Host != null && hp.Host.OperatorId == personId &&
                    hp.HostId == hostId && hp.PluginId == pluginId
                );

            if (hostPlugin == null)
            {
                throw new InvalidOperationException("HostPlugin not found");
            }

            _context.HostPlugins.Remove(hostPlugin);

            await _context.SaveChangesAsync();
        }

        public async Task AddPluginToAgentAsPersonAsync(string agentId, string pluginId, string personId)
        {
            if (string.IsNullOrWhiteSpace(agentId))
            {
                throw new InvalidOperationException("agentId is invalid");
            }

            if (string.IsNullOrWhiteSpace(pluginId))
            {
                throw new InvalidOperationException("pluginId is invalid");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            var agent = await GetRecordByIdAsPersonAsync<Agent>(agentId, personId);

            if (agent == null)
            {
                throw new InvalidOperationException("Agent not found");
            }

            var plugin = await _context.Plugins
                .FirstOrDefaultAsync(p => p.Id == pluginId && (p.CreatorId == personId || p.Visibility == Core.Models.Entities.Visibility.Public));

            if (plugin == null)
            {
                throw new InvalidOperationException("Plugin not found");
            }

            var agentPlugin = new AgentPlugin
            {
                AgentId = agentId,
                PluginId = pluginId
            };

            _context.AgentPlugins.Add(agentPlugin);

            await _context.SaveChangesAsync();
        }

        public async Task RemovePluginFromAgentAsPersonAsync(string agentId, string pluginId, string personId)
        {
            if (string.IsNullOrWhiteSpace(agentId))
            {
                throw new InvalidOperationException("agentId is invalid");
            }

            if (string.IsNullOrWhiteSpace(pluginId))
            {
                throw new InvalidOperationException("pluginId is invalid");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            var agentPlugin = await _context.AgentPlugins.Include(ap => ap.Agent).ThenInclude(a => a.Agency)
                .FirstOrDefaultAsync(ap =>
                    ap.Agent != null && ap.Agent.Agency != null && ap.Agent.Agency.DirectorId == personId &&
                    ap.AgentId == agentId && ap.PluginId == pluginId
                );

            if (agentPlugin == null)
            {
                throw new InvalidOperationException("AgentPlugin not found");
            }

            _context.AgentPlugins.Remove(agentPlugin);

            await _context.SaveChangesAsync();
        }


        public async Task<T?> GetRecordByIdAsync<T>(string recordId) where T : Core.Models.Entities.AgienceEntity
        {
            if (typeof(T) == typeof(Host))
            {
                return await _context.Set<T>().Include("Keys").FirstOrDefaultAsync(h => h.Id == recordId);
            }

            return await _context.Set<T>().FindAsync(recordId);
        }

        public async Task<T?> CreateRecordAsync<T>(T record) where T : Core.Models.Entities.AgienceEntity
        {
            if (!string.IsNullOrWhiteSpace(record.Id))
            {
                throw new InvalidOperationException("recordId is invalid");
            }

            record.Id = _idProvider.GenerateId(typeof(T).Name);

            _context.Set<T>().Add(record);

            await _context.SaveChangesAsync();

            return record;
        }

        public async Task UpdateRecordAsync<T>(T record, CancellationToken cancellationToken) where T : Core.Models.Entities.AgienceEntity
        {
            if (string.IsNullOrWhiteSpace(record.Id))
            {
                throw new InvalidOperationException("recordId is invalid");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            _context.Set<T>().Update(record);

            await _context.SaveChangesAsync();
        }

        public async Task<Person?> GetPersonByExternalProviderIdAsync(string providerId, string providerPersonId, CancellationToken cancellationToken)
        {
            return await _context.People.FirstOrDefaultAsync(p => p.ProviderId == providerId && p.ProviderPersonId == providerPersonId, cancellationToken);
        }

        public async Task<bool> VerifyHostSourceTargetRelationships(string hostId, string? sourceId, string? targetAgencyId, string? targetAgentId)
        {
            // hostId is never null
            // sourceId is null, Agency Id, or Agent Id
            // An Agency may only connect with an Agent within the same Agency
            // An Agent may only connect with an Agent within the same Agency or with its Agency
            // An Agent may not connect with itself
            // The Host must be related to the sourceId during WRITE
            // The Host must be related to the target during SUBSCRIBE

            // Validate input parameters
            if (string.IsNullOrEmpty(hostId) || // Host is always required
                (string.IsNullOrEmpty(targetAgencyId) && string.IsNullOrEmpty(targetAgentId)) || // Must have a target
                (!string.IsNullOrEmpty(targetAgencyId) && !string.IsNullOrEmpty(targetAgentId))) // Must not have two targets
            {
                return false;
            }

            IQueryable<Agent> query = _context.Agents
                .Include(a => a.Agency)
                .Include(a => a.Host);

            if (!string.IsNullOrEmpty(sourceId))
            {
                if (!string.IsNullOrEmpty(targetAgencyId))
                {
                    // Ensure the source Agent is related to the Host and target Agency
                    query = query.Where(a =>
                        a.AgencyId == targetAgencyId &&
                        a.HostId == hostId &&
                        a.Id == sourceId);
                }
                else if (!string.IsNullOrEmpty(targetAgentId))
                {
                    // Ensure the source and target Agents are in the same Agency and related to the Host
                    query = query.Where(a =>
                        (a.Id == targetAgentId && a.HostId == hostId && a.Id != sourceId && a.AgencyId == (a.AgencyId)) ||
                        (a.HostId == hostId && a.Id == sourceId && a.Agency != null && a.Agency.Agents.Any(ag => ag.Id == targetAgentId && ag.AgencyId == a.AgencyId)));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(targetAgencyId))
                {
                    // Ensure the Host is related to the target Agency
                    query = query.Where(a =>
                        a.AgencyId == targetAgencyId &&
                        a.HostId == hostId);
                }
                else if (!string.IsNullOrEmpty(targetAgentId))
                {
                    // Ensure the Host is related to the target Agent
                    query = query.Where(a =>
                        a.Id == targetAgentId &&
                        a.HostId == hostId);
                }
            }

            var queryResult = await query.ToListAsync();

            // If there is exactly one valid relationship, return true
            if (queryResult.Count == 1)
            {
                return true;
            }

            // If there are multiple relationships, ensure they all belong to the same Agency
            if (queryResult.Count >= 2)
            {
                string? agencyId = null;

                foreach (var record in queryResult)
                {
                    if (record.Agency?.Id == null)
                    {
                        return false;
                    }
                    else if (agencyId == null)
                    {
                        agencyId = record.Agency.Id;
                    }
                    else if (agencyId != record.Agency.Id)
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        public async Task<Core.Models.Entities.Host> GetHostByIdNoTrackingAsync(string hostId)
        {
            var host = await _context.Hosts.AsNoTracking().FirstOrDefaultAsync(h => h.Id == hostId);
            return _mapper.Map<Core.Models.Entities.Host>(host);
        }

        public async Task<IEnumerable<Core.Models.Entities.Plugin>> GetPluginsForHostIdNoTrackingAsync(string hostId)
        {
            var plugins = await _context.HostPlugins.AsNoTracking().Where(hp => hp.HostId == hostId).Include(hp => hp.Plugin).ThenInclude(p => p.PluginFunctions).ThenInclude(pf => pf.Function).Select(hp => hp.Plugin).ToListAsync();
            return _mapper.Map<IEnumerable<Core.Models.Entities.Plugin>>(plugins);
        }

        public async Task<IEnumerable<Core.Models.Entities.Agent>> GetAgentsForHostIdNoTrackingAsync(string hostId)
        {
            var agents = await _context.Agents.AsNoTracking().Where(a => a.HostId == hostId).Include(a => a.Agency).Include(a => a.Plugins).ThenInclude(p => p.PluginFunctions).ThenInclude(pf => pf.Function).ToListAsync();
            return _mapper.Map<IEnumerable<Core.Models.Entities.Agent>>(agents);
        }

        public async Task<IEnumerable<PluginConnection>> GetPluginConnectionsForAgentAsPersonAsync(string agentId, string personId)
        {
            return await _context.PluginConnections
                        .Include(pc => pc.Plugin)
                        .Where(pc => _context.AgentPlugins.Any(ap => ap.AgentId == agentId && ap.PluginId == pc.PluginId))
                        .ToListAsync();
        }

        public async Task UpsertAgentConnectionAsPersonAsync(string agentId, string pluginConnectionId, string? authorizerId, string? credentialId, string personId)
        {
            if (string.IsNullOrWhiteSpace(agentId))
            {
                throw new InvalidOperationException("agentId is invalid");
            }

            if (string.IsNullOrWhiteSpace(pluginConnectionId))
            {
                throw new InvalidOperationException("pluginConnectionId is invalid");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                throw new InvalidOperationException("personId is invalid");
            }

            var agent = await GetRecordByIdAsPersonAsync<Agent>(agentId, personId);

            if (agent == null)
            {
                throw new InvalidOperationException("Agent not found or access denied.");
            }

            var pluginConnection = await _context.PluginConnections.FirstOrDefaultAsync(pc => pc.Id == pluginConnectionId && (pc.Plugin.CreatorId == personId || pc.Plugin.Visibility == Core.Models.Entities.Visibility.Public));

            if (pluginConnection == null)
            {
                throw new InvalidOperationException("PluginConnection not found or access denied.");
            }

            var existingConnection = await GetAgentConnectionAsPersonAsync(agentId, pluginConnectionId, personId);

            if (existingConnection == null)
            {
                _context.AgentConnections.Add(new AgentConnection
                {
                    AgentId = agentId,
                    PluginConnectionId = pluginConnectionId,
                    AuthorizerId = authorizerId,
                    CredentialId = credentialId,
                    Id = _idProvider.GenerateId(nameof(AgentConnection))
                });
            }
            else
            {
                existingConnection.AuthorizerId = authorizerId;
                existingConnection.CredentialId = credentialId;
                _context.AgentConnections.Update(existingConnection);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<AgentConnection?> GetAgentConnectionAsPersonAsync(string agentId, string pluginConnectionId, string personId)
        {
            return await _context.AgentConnections.FirstOrDefaultAsync(ac => ac.AgentId == agentId && ac.PluginConnectionId == pluginConnectionId);
        }

        public async Task<IEnumerable<Function>> GetFunctionsForAgentAsPersonAsync(string agentId, string personId)
        {
            // Ensure the person is the Agent Director
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.Id == agentId && a.Agency != null && a.Agency.DirectorId == personId);

            if (agent == null)
            {
                _logger.LogWarning("Agent not found or person does not have access. AgentId: {AgentId}, PersonId: {PersonId}", agentId, personId);
                return Enumerable.Empty<Function>();
            }

            // Get the functions loaded to the agent via PluginFunction-Plugin-AgentPlugin-Agent
            var functions = await _context.Functions
                .Include(f => f.PluginFunctions)
                .ThenInclude(pf => pf.Plugin)
                .ThenInclude(p => p.Agents)
                .Where(f => f.PluginFunctions.Any(pf => pf.Plugin != null && pf.Plugin.Agents.Any(a => a.Id == agentId)))
                .ToListAsync();

            /*
            // Unmap the Plugin property for each function
            foreach (var function in functions)
            {
                foreach (var pluginFunction in function.PluginFunctions)
                {
                    pluginFunction.Plugin.Functions = null; // Avoid circular reference
                }
            }*/

            return functions;
        }

        public Task<string?> GetHostIdForAgentIdNoTrackingAsync(string agentId)
        {
            return _context.Agents.AsNoTracking().Where(a => a.Id == agentId).Select(a => a.HostId).FirstOrDefaultAsync();

        }
    }
}