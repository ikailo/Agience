using Agience.Authority.Models.Manage;
using Agience.Core.Models.Entities.Abstract;
using Agience.Authority.Manage.Models;
using Host = Agience.Authority.Models.Manage.Host;

namespace Agience.Authority.Manage.Services
{
    public class EntityRegistry
    {
        // Static dictionary to hold EntityDefinitions
        private static readonly Dictionary<Type, EntityDefinition> _entityDefinitions = new();

        public static void RegisterAgienceEntities()
        {

            RegisterEntityDefinition<Agent>(new EntityDefinition(
                basePath: "/manage",
                name: "Agent",
                pluralName: "Agents",
                apiName: "agent",
                apiPluralName: "agents"
            ));           

            RegisterEntityDefinition<AgentLogEntry>(new EntityDefinition(
                basePath: "/manage",
                name: "Agent Log Entry",
                pluralName: "Agent Log Entries",
                apiName: "agent-log-entry",
                apiPluralName: "agent-log-entries"
            ));

            RegisterEntityDefinition<Authorizer>(new EntityDefinition(
                basePath: "/manage",
                name: "Authorizer",
                pluralName: "Authorizers",
                apiName: "authorizer",
                apiPluralName: "authorizers"
            ));

            RegisterEntityDefinition<Connection>(new EntityDefinition(
                basePath: "/manage",
                name: "Connection",
                pluralName: "Connections",
                apiName: "connection",
                apiPluralName: "connections"
            ));

            RegisterEntityDefinition<Credential>(new EntityDefinition(
                basePath: "/manage",
                name: "Credential",
                pluralName: "Credentials",
                apiName: "credential",
                apiPluralName: "credentials"
            ));

            RegisterEntityDefinition<Function>(new EntityDefinition(
                basePath: "/manage",
                name: "Function",
                pluralName: "Functions",
                apiName: "function",
                apiPluralName: "functions"
            ));

            RegisterEntityDefinition<FunctionConnection>(new EntityDefinition(
                basePath: "/manage",
                name: "Function Connection",
                pluralName: "Functions",
                apiName: "functionconnection",
                apiPluralName: "functionconnections",
                post: "functions/connection",
                getAll:"functions/connections",
                getOne: "functions/connection",
                put: "functions/connection",
                delete: "functions/connection"                
            ));

            RegisterEntityDefinition<Host>(new EntityDefinition(
                basePath: "/manage",
                name: "Host",
                pluralName: "Hosts",
                apiName: "host",
                apiPluralName: "hosts"
            ));

            RegisterEntityDefinition<Key>(new EntityDefinition(
                basePath: "/manage",
                name: "Key",
                pluralName: "Keys",
                apiName: "key",
                apiPluralName: "keys",
                post: "key/generate"
            ));

            RegisterEntityDefinition<Person>(new EntityDefinition(
                basePath: "/manage",
                name: "Person",
                pluralName: "People",
                apiName: "person",
                apiPluralName: "people"
            ));

            RegisterEntityDefinition<Plugin>(new EntityDefinition(
                basePath: "/manage",
                name: "Plugin",
                pluralName: "Plugins",
                apiName: "plugin",
                apiPluralName: "plugins"
            ));

            RegisterEntityDefinition<Topic>(new EntityDefinition(
                basePath: "/manage",
                name: "Topic",
                pluralName: "Topics",
                apiName: "topic",
                apiPluralName: "topics"
            ));
        }

        // Register an EntityDefinition for a specific type
        public static void RegisterEntityDefinition<TEntity>(EntityDefinition definition) where TEntity : BaseEntity
        {
            _entityDefinitions[typeof(TEntity)] = definition;
            _entityDefinitions[typeof(TEntity)].EntityType = typeof(TEntity);
        }

        // Retrieve an EntityDefinition for a specific type
        public static EntityDefinition? GetEntityDefinition<TEntity>() where TEntity : BaseEntity
        {
            _entityDefinitions.TryGetValue(typeof(TEntity), out var definition);
            return definition;
        }

        public static EntityDefinition? GetEntityDefinition(Type type)
        {
            _entityDefinitions.TryGetValue(type, out var definition);
            return definition;
        }

        public static EntityDefinition? GetEntityDefinitionByApiName(string apiName)
        {   
            return _entityDefinitions.Values.FirstOrDefault(def => def.ApiName == apiName);
        }
    }
}
