using Agience.Authority.Manage.Models;
using Agience.Authority.Models.Manage;
using Agience.Core.Models.Entities.Abstract;
using System.Xml;

namespace Agience.Authority.Manage.Services
{
    public class RecordHandler
    {
        private readonly AuthorityService _authorityService;

        public RecordHandler(AuthorityService authorityService)
        {
            _authorityService = authorityService;
        }

        public async Task<IEnumerable<T>> FetchRecordsAsync<T>(
            Func<IEnumerable<T>, Task>? fetchHook = null,
            int pageNumber = 1,
            int pageSize = 10)
            where T : BaseEntity
        {
            var entity = GetEntityDefinition<T>();

            var client = await GetHttpClientAsync();

            var endpoint = $"{entity.GetAll()}?pageNumber={pageNumber}&pageSize={pageSize}";

            var response = await client.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            var records = await response.Content.ReadFromJsonAsync<IEnumerable<T>>() ??
                throw new InvalidDataException("Failed to deserialize records.");

            if (fetchHook != null)
                await fetchHook(records);

            return records;
        }

        public async Task<IEnumerable<DescribedEntity>> SearchRecordsAsync(
            EntityDefinition entity,
            string searchQuery,
            Func<IEnumerable<DescribedEntity>, Task>? fetchHook = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (entity.EntityType == null || !typeof(DescribedEntity).IsAssignableFrom(entity.EntityType))
            {
                throw new InvalidOperationException("EntityType must be a valid type derived from NamedEntity.");
            }

            var client = await GetHttpClientAsync();

            var endpoint = $"{entity.GetAll()}?search={searchQuery}&pageNumber={pageNumber}&pageSize={pageSize}";
            var response = await client.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            // Deserialize into the specified runtime type from EntityType
            var records = await DeserializeToConcreteTypeAsync(response, entity.EntityType);

            if (fetchHook != null)
                await fetchHook(records);

            return records;
        }




        public async Task SaveRecordAsync<T>(T record, Func<T, Task<bool>>? saveHook = null)
            where T : BaseEntity, new()
        {
            if (saveHook != null && !await saveHook(record))
                return;

            record.CreatedDate = null;

            var client = await GetHttpClientAsync();

            var entity = GetEntityDefinition<T>();

            var endpoint = record.Id == null
                ? entity.Post()
                : entity.Put(("id", record.Id));

            var response = record.Id == null
                ? await client.PostAsJsonAsync(endpoint, record)
                : await client.PutAsJsonAsync(endpoint, record);

            response.EnsureSuccessStatusCode();

            if (record.Id == null)
            {
                record.Id = (await response.Content.ReadFromJsonAsync<T>())?.Id ??
                    throw new InvalidDataException("Failed to deserialize saved record.");
            }
        }

        public async Task<bool> DeleteRecordAsync<T>(string recordId, Func<string, Task<bool>>? deleteHook = null)
            where T : BaseEntity, new()
        {
            if (deleteHook != null && !await deleteHook(recordId))
                return false;

            var client = await GetHttpClientAsync();

            var entity = GetEntityDefinition<T>();

            var endpoint = entity.Delete(("id", recordId));

            var response = await client.DeleteAsync(endpoint);

            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<TChild>> FetchChildRecordsAsync<TParent, TChild>(
            string parentId,
            Func<IEnumerable<TChild>, Task>? fetchPostHook = null,
            int pageNumber = 1,
            int pageSize = 10)
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
        {
            var client = await GetHttpClientAsync();

            var parentEntity = GetEntityDefinition<TParent>();
            var childEntity = GetEntityDefinition<TChild>();

            var endpoint = $"{parentEntity.GetAllChildren(parentId, childEntity)}?pageNumber={pageNumber}&pageSize={pageSize}";

            var response = await client.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            var stringContent = await response.Content.ReadAsStringAsync();

            var records = await response.Content.ReadFromJsonAsync<List<TChild>>() ??
                throw new InvalidDataException("Failed to deserialize child records.");

            if (fetchPostHook != null) await fetchPostHook(records);

            return records;
        }

        public async Task SaveChildRecordAsync<TParent, TChild>(
            string parentId,
            TChild childRecord,
            Func<TChild, Task<bool>>? savePreHook = null,
            Func<TChild?, Task>? savePostHook = null
            )
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
        {
            if (savePreHook != null && !await savePreHook(childRecord))
                return;

            childRecord.CreatedDate = null;

            var client = await GetHttpClientAsync();

            var parentEntity = GetEntityDefinition<TParent>();
            var childEntity = GetEntityDefinition<TChild>();

            var endpoint = childRecord.Id == null
                ? parentEntity.PostChild(parentId, childEntity)
                : parentEntity.PostChild(parentId, childEntity, childRecord.Id);

            var response = await client.PostAsJsonAsync(endpoint, childRecord);

            response.EnsureSuccessStatusCode();

            TChild? newChildRecord = null;

            try
            {
                if (childRecord.Id == null)
                    newChildRecord = await response.Content.ReadFromJsonAsync<TChild>();

                childRecord.Id ??= newChildRecord?.Id;
            }
            catch
            {
                // TODO: Log Error
            }

            if (savePostHook != null)
                await savePostHook(newChildRecord);
        }

        public async Task<bool> DeleteChildRecordAsync<TParent, TChild>(
            string parentId,
            string childRecordId,
            Func<string, Task<bool>>? deleteHook = null)
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
        {
            if (deleteHook != null && !await deleteHook(childRecordId))
                return false;

            var client = await GetHttpClientAsync();

            var parentEntity = GetEntityDefinition<TParent>();
            var childEntity = GetEntityDefinition<TChild>();

            var endpoint = parentEntity.DeleteChild(parentId, childEntity, childRecordId);

            var response = await client.DeleteAsync(endpoint);

            return response.IsSuccessStatusCode;
        }
        /*
        public async Task<string> UploadPluginLibraryAsync(ByteArrayContent fileContent)
        {
            var client = await GetHttpClientAsync();

            using var form = new MultipartFormDataContent();
            form.Add(fileContent, "file", "plugin-package.zip"); // Add file content with a field name and file name

            var response = await client.PostAsync($"manage/plugin/library/import/upload", form);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(); // Assume it returns the importId
        }


        public async Task<string> StartPluginLibraryImportAsync(string repoFileUri)
        {
            var client = await GetHttpClientAsync();
            var response = await client.PostAsync($"manage/plugin/library/import/csproj?fileUrl={repoFileUri}", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(); // Assume it returns the importId
        }

        public async Task<string> GetPluginLibraryImportStatusAsync(string importId)
        {
            var client = await GetHttpClientAsync();
            var response = await client.GetAsync($"manage/plugin/library/import/{importId}/status");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(); // Assume it returns status as a string
        }

        public async Task<PluginLibrary> GetPluginLibraryAsync(string recordId)
        {
            var client = await GetHttpClientAsync();
            var response = await client.GetAsync($"manage/plugin/library/{recordId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PluginLibrary>() ??
                throw new InvalidOperationException("Failed to fetch PluginLibrary.");
        }
        */

        private async Task<HttpClient> GetHttpClientAsync()
        {
            var httpClient = await _authorityService.GetHttpClientAsync();

            if (httpClient == null)
                throw new InvalidOperationException("Unable to get a configured HttpClient.");

            return httpClient;
        }

        private EntityDefinition GetEntityDefinition<T>() where T : BaseEntity
        {
            var entityDef = EntityRegistry.GetEntityDefinition<T>();

            if (entityDef == null)
                throw new InvalidOperationException($"Entity definition not found for type {typeof(T).Name}.");

            return entityDef;
        }

        private async Task<IEnumerable<DescribedEntity>> DeserializeToConcreteTypeAsync(
           HttpResponseMessage response, Type concreteType)
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            // Deserialize to List<TConcrete> using System.Text.Json
            var method = typeof(System.Text.Json.JsonSerializer)
                .GetMethod(nameof(System.Text.Json.JsonSerializer.Deserialize), new[] { typeof(string), typeof(System.Text.Json.JsonSerializerOptions) })
                ?.MakeGenericMethod(typeof(List<>).MakeGenericType(concreteType));

            if (method == null)
                throw new InvalidOperationException("Failed to reflect deserialize method.");

            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var deserialized = method.Invoke(null, new object[] { jsonString, options });

            if (deserialized is IEnumerable<DescribedEntity> describedEntities)
                return describedEntities;

            throw new InvalidDataException($"Deserialized data is not of type IEnumerable<{concreteType.Name}>.");
        }
    }

}
