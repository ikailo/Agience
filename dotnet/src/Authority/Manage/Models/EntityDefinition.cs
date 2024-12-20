using System;

namespace Agience.Authority.Manage.Models
{
    public class EntityDefinition
    {
        // Base Path
        public string BasePath { get; set; }

        // Names for the entity
        public string Name { get; set; }
        public string PluralName { get; set; }
        public string ApiName { get; set; }
        public string ApiPluralName { get; set; }
        public Type? EntityType { get; set; } // This is set by the EntityRegistry

        // Customizable Paths
        private readonly string? _post;
        private readonly string? _getAll;
        private readonly string? _getOne;
        private readonly string? _put;
        private readonly string? _delete;

        // Constructor
        public EntityDefinition(
            string basePath = "/",
            string name = "entity",
            string pluralName = "entities",
            string? apiName = null,
            string? apiPluralName = null,
            string? post = null,
            string? getAll = null,
            string? getOne = null,
            string? put = null,
            string? delete = null)
        {
            BasePath = basePath.TrimEnd('/'); // Remove trailing slash for consistency
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PluralName = pluralName ?? throw new ArgumentNullException(nameof(pluralName));
            ApiName = apiName ?? Name.ToLower();
            ApiPluralName = apiPluralName ?? PluralName.ToLower();

            _post = post;
            _getAll = getAll;
            _getOne = getOne;
            _put = put;
            _delete = delete;
        }

        // Standard CRUD Methods
        public string Post() => BuildPath(_post ?? $"{ApiName}");
        public string GetAll() => BuildPath(_getAll ?? $"{ApiPluralName}");
        public string GetOne(params (string key, string value)[] parameters) => BuildPath(_getOne ?? $"{ApiName}/{{id}}", parameters);
        public string Put(params (string key, string value)[] parameters) => BuildPath(_put ?? $"{ApiName}/{{id}}", parameters);
        public string Delete(params (string key, string value)[] parameters) => BuildPath(_delete ?? $"{ApiName}/{{id}}", parameters);

        // Parent-Child Relationship Methods
        public string PostChild(string parentId, EntityDefinition childEntity)
        {
            return BuildPath($"{ApiName}/{parentId}/{childEntity._post ?? childEntity.ApiName}");
        }

        public string GetAllChildren(string parentId, EntityDefinition childEntity)
        {
            return BuildPath($"{ApiName}/{parentId}/{childEntity._getAll ?? childEntity.ApiPluralName}");
        }

        public string GetOneChild(string parentId, EntityDefinition childEntity, string childId)
        {
            return BuildPath($"{ApiName}/{parentId}/{childEntity._getOne ?? childEntity.ApiName}/{{childId}}", ("childId", childId));
        }

        public string PutChild(string parentId, EntityDefinition childEntity, string childId)
        {
            return BuildPath($"{ApiName}/{parentId}/{childEntity._put ?? childEntity.ApiName}/{{childId}}", ("childId", childId));
        }

        public string PostChild(string parentId, EntityDefinition childEntity, string childId)
        {
            return BuildPath($"{ApiName}/{parentId}/{childEntity._post ?? childEntity.ApiName}/{{childId}}", ("childId", childId));
        }

        public string DeleteChild(string parentId, EntityDefinition childEntity, string childId)
        {
            return BuildPath($"{ApiName}/{parentId}/{childEntity._delete ?? childEntity.ApiName}/{{childId}}", ("childId", childId));
        }

        // Helper method to construct paths with parameters
        private string BuildPath(string relativePath, params (string key, string value)[] parameters)
        {
            string fullPath = $"{BasePath}/{relativePath}".TrimEnd('/');
            foreach (var (key, value) in parameters)
            {
                fullPath = fullPath.Replace($"{{{key}}}", value);
            }
            return fullPath;
        }
    }
}
