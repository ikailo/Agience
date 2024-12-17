using Agience.Authority.Identity.Data.Adapters;
using Agience.Core.Models.Entities.Abstract;
using Agience.Core.Models.Enums;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Reflection.Metadata.Ecma335;

namespace Agience.Authority.Identity.Data.Repositories
{
    public class RecordsRepository
    {
        protected readonly AgienceDataAdapter _dataAdapter;
        private readonly ILogger<RecordsRepository> _logger;

        public RecordsRepository(AgienceDataAdapter dataAdapter, ILogger<RecordsRepository> logger)
        {
            _dataAdapter = dataAdapter ?? throw new ArgumentNullException(nameof(dataAdapter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // *** HELPERS *** //

        public static bool IsOwned<T>() => typeof(OwnedEntity).IsAssignableFrom(typeof(T));
        public static bool IsPublic<T>() => typeof(PublicEntity).IsAssignableFrom(typeof(T));
        public static void ValidateNotNull(object parameter, string parameterName)
        {
            if (parameter == null) { throw new ArgumentNullException(parameterName); }
        }

        // *** CREATE *** //

        public async Task<T> CreateRecordAsSystemAsync<T>(T record) where T : BaseEntity, new()
        {
            ValidateNotNull(record, nameof(record));
            return await _dataAdapter.CreateRecordAsync(record);
        }

        public async Task<T> CreateRecordAsPersonAsync<T>(T record, string personId) where T : OwnedEntity, new()
        {
            ValidateNotNull(record, nameof(record));
            ValidateNotNull(personId, nameof(personId));

            if (!IsOwned<T>())
            {
                throw new InvalidOperationException($"{typeof(T).Name} does not support ownership.");
            }

            if (record is not OwnedEntity ownedRecord || ownedRecord.OwnerId != null)
            {
                throw new InvalidOperationException("The record is already owned or is not a valid owned entity.");
            }

            ownedRecord.OwnerId = personId;
            return await _dataAdapter.CreateRecordAsync(record);
        }

        // *** READ *** //

        public async Task<IEnumerable<T>> GetRecordsAsSystemAsync<T>(int? skip = null, int? take = null) where T : BaseEntity, new()
        {
            return await _dataAdapter.GetAllRecordsAsync<T>(skip, take);
        }

        public async Task<IEnumerable<T>> GetRecordsAsPersonAsync<T>(string personId, bool includePublic = false, int? skip = null, int? take = null) where T : OwnedEntity, new()
        {
            ValidateNotNull(personId, nameof(personId));
            return await _dataAdapter.GetAllOwnedRecordsAsync<T>(personId, includePublic, skip, take);
        }
        
        public async Task<IEnumerable<T>> GetRecordsByIdsAsync<T>(IEnumerable<string> ids) where T : BaseEntity, new()
        {
            return await _dataAdapter.GetRecordsByIdsAsync<T>(ids);
        }

        public async Task<T?> GetRecordByIdAsSystemAsync<T>(string id) where T : BaseEntity, new()
        {
            ValidateNotNull(id, nameof(id));
            return await _dataAdapter.GetRecordByIdAsync<T>(id);
        }

        public async Task<T?> GetRecordByIdAsPersonAsync<T>(string id, string personId, bool includePublic = false) where T : OwnedEntity, new()
        {
            ValidateNotNull(id, nameof(id));
            ValidateNotNull(personId, nameof(personId));
            return await _dataAdapter.GetOwnedRecordByIdAsync<T>(id, personId, includePublic);
        }

        public async Task<IEnumerable<TChild>> GetChildRecordsAsSystemAsync<TParent, TChild>(
            string foreignKey,
            string parentId,
            int? skip = null,
            int? take = null
        )
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
        {
            ValidateNotNull(foreignKey, nameof(foreignKey));
            ValidateNotNull(parentId, nameof(parentId));

            return await _dataAdapter.QueryRecordsAsync<TChild>(new Dictionary<string, object> { { foreignKey, parentId } }, skip, take);
        }

        public async Task<IEnumerable<TChild>> GetChildRecordsAsPersonAsync<TParent, TChild>(
            string foreignKey,
            string parentId,
            string personId,
            bool includePublic = false,
            int? skip = null,
            int? take = null
        )
            where TParent : OwnedEntity, new()
            where TChild : BaseEntity, new()
        {
            ValidateNotNull(foreignKey, nameof(foreignKey));

            var parent = await GetRecordByIdAsPersonAsync<TParent>(parentId, personId);

            if (parent == null)
            {
                throw new KeyNotFoundException($"Parent record not found.");
            }

            return await _dataAdapter.QueryRecordsAsync<TChild>(new Dictionary<string, object> { { foreignKey, parentId } }, skip, take);
        }

              
        public async Task<TChild?> GetChildRecordByIdAsPersonAsync<TParent, TChild>(
            string foreignKey,
            string childId,
            string personId,
            bool includePublic = false,
            int? skip = null,
            int? take = null
        )
            where TParent : OwnedEntity, new()
            where TChild : BaseEntity, new()
        {
            ValidateNotNull(foreignKey, nameof(foreignKey));
            ValidateNotNull(childId, nameof(childId));
            ValidateNotNull(personId, nameof(personId));

            // Retrieve the child record by its ID
            var childRecord = await _dataAdapter.GetRecordByIdAsync<TChild>(childId);
            if (childRecord == null)
            {
                throw new KeyNotFoundException($"Child record with ID {childId} not found.");
            }

            // Extract the parent ID from the child record
            var parentIdProperty = typeof(TChild).GetProperty(foreignKey);
            if (parentIdProperty == null)
            {
                throw new ArgumentException($"Foreign key property '{foreignKey}' not found in type '{typeof(TChild).Name}'.");
            }

            var parentId = parentIdProperty.GetValue(childRecord)?.ToString();
            if (string.IsNullOrEmpty(parentId))
            {
                throw new KeyNotFoundException($"Parent ID not found in child record with ID {childId}.");
            }

            // Verify the parent record belongs to the current person
            var parentRecord = await GetRecordByIdAsPersonAsync<TParent>(parentId, personId, includePublic);
            if (parentRecord == null)
            {
                throw new KeyNotFoundException($"Parent record with ID {parentId} not found or not accessible by person {personId}.");
            }

            // Return the child record
            return childRecord;
        }


        public async Task<TChild?> GetChildRecordByIdAsPersonAsync<TParent, TChild>(
            string foreignKey,
            string parentId,
            string childId,
            string personId,
            bool includePublic = false,
            int? skip = null,
            int? take = null
        )
            where TParent : OwnedEntity, new()
            where TChild : BaseEntity, new()
        {
            ValidateNotNull(foreignKey, nameof(foreignKey));

            var parent = await GetRecordByIdAsPersonAsync<TParent>(parentId, personId);

            if (parent == null)
            {
                throw new KeyNotFoundException($"Parent record not found.");
            }

            return await _dataAdapter.GetRecordByIdAsync<TChild>(childId);
        }

        public async Task<IEnumerable<TChild>> GetChildRecordsByIdsAsSystemAsync<TParent, TChild>(
            string foreignKey,
            IEnumerable<string> parentIds,
            int? skip = null,
            int? take = null
        )
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
        {
            ValidateNotNull(foreignKey, nameof(foreignKey));
            ValidateNotNull(parentIds, nameof(parentIds));

            if (!parentIds.Any())
            {
                return Enumerable.Empty<TChild>();
            }

            // Build a dynamic query using LINQ
            var query = _dataAdapter.GetQueryable<TChild>()
                .Where(child => parentIds.Contains(EF.Property<string>(child, foreignKey)));

            // Apply pagination if necessary
            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }


        public async Task<IEnumerable<TChild>> GetChildRecordsWithJoinAsPersonAsync<TParent, TChild, TJoin>(
             string parentForeignKey,
             string childForeignKey,
             string personId,
             bool includePublic = false,
             int? skip = null,
             int? take = null
         )
             where TParent : OwnedEntity, new()
             where TChild : BaseEntity, new()
             where TJoin : BaseEntity, new()
        {
            // Step 1: Validate required parameters
            ValidateNotNull(parentForeignKey, nameof(parentForeignKey));
            ValidateNotNull(childForeignKey, nameof(childForeignKey));
            ValidateNotNull(personId, nameof(personId));

            // Step 2: Fetch accessible parent IDs for the person
            var parentIds = await _dataAdapter.GetOwnedParentIdsAsync<TParent>(personId, includePublic);
            if (!parentIds.Any())
            {
                return Enumerable.Empty<TChild>();
            }

            // Step 3: Fetch child IDs via join table
            var childIds = await _dataAdapter.GetChildIdsFromJoinAsync<TJoin>(parentForeignKey, parentIds, childForeignKey);
            if (!childIds.Any())
            {
                return Enumerable.Empty<TChild>();
            }

            // Step 4: Fetch child records by their IDs
            var childRecords = await _dataAdapter.GetRecordsByIdsAsync<TChild>(childIds);

            // Step 5: Apply optional pagination (skip/take)
            if (skip.HasValue || take.HasValue)
            {
                childRecords = childRecords.Skip(skip ?? 0).Take(take ?? int.MaxValue);
            }

            return childRecords;
        }


        public async Task<IEnumerable<TChild>> GetChildRecordsWithJoinAsPersonAsync<TParent, TChild, TJoin>(
            string parentForeignKey,
            string childForeignKey,
            string parentId,
            string personId,
            bool includePublic = false,
            int? skip = null,
            int? take = null
        )
            where TParent : OwnedEntity, new()
            where TChild : BaseEntity, new()
            where TJoin : BaseEntity, new()
        {
            // Validate inputs
            ValidateNotNull(parentForeignKey, nameof(parentForeignKey));
            ValidateNotNull(childForeignKey, nameof(childForeignKey));
            ValidateNotNull(parentId, nameof(parentId));
            ValidateNotNull(personId, nameof(personId));

            // Ensure the parent exists for the current user
            var parentRecord = await GetRecordByIdAsPersonAsync<TParent>(parentId, personId, includePublic);
            if (parentRecord == null)
            {
                throw new KeyNotFoundException($"Parent record with ID {parentId} not found or not accessible.");
            }

            // Query the join table for child IDs
            var joinRecords = await _dataAdapter.QueryRecordsAsync<TJoin>(
                new Dictionary<string, object> { { parentForeignKey, parentId } },
                skip,
                take
            );

            if (!(joinRecords?.Any() ?? false))
            {
                return Enumerable.Empty<TChild>();
            }

            // Validate that the childForeignKey property exists
            var propertyInfo = typeof(TJoin).GetProperty(childForeignKey);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{childForeignKey}' does not exist in type '{typeof(TJoin).Name}'.");
            }

            // Extract distinct child IDs from the join records
            IEnumerable<string> childIds = joinRecords
                .Select(jr => propertyInfo.GetValue(jr)?.ToString())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList()!;

            if (!childIds.Any())
            {
                return Enumerable.Empty<TChild>();
            }

            // Query the child records by their IDs
            return await _dataAdapter.GetRecordsByIdsAsync<TChild>(childIds);
        }

        public async Task<IEnumerable<TChild>> GetChildRecordsByIdsWithJoinAsSystemAsync<TParent, TChild, TJoin>(
            string parentForeignKey,
            string childForeignKey,
            IEnumerable<string> parentIds,
            int? skip = null,
            int? take = null
        )
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
            where TJoin : BaseEntity, new()
        {
            // Validate inputs
            ValidateNotNull(parentForeignKey, nameof(parentForeignKey));
            ValidateNotNull(childForeignKey, nameof(childForeignKey));
            ValidateNotNull(parentIds, nameof(parentIds));

            if (!parentIds.Any())
            {
                return Enumerable.Empty<TChild>();
            }

            // Step 1: Retrieve child IDs via join table
            var childIds = await _dataAdapter.GetChildIdsFromJoinAsync<TJoin>(parentForeignKey, parentIds, childForeignKey);

            if (!childIds.Any())
            {
                return Enumerable.Empty<TChild>();
            }

            // Step 2: Retrieve child records by their IDs
            var childRecords = await _dataAdapter.GetRecordsByIdsAsync<TChild>(childIds);

            // Step 3: Apply optional pagination
            if (skip.HasValue || take.HasValue)
            {
                childRecords = childRecords.Skip(skip ?? 0).Take(take ?? int.MaxValue);
            }

            return childRecords;
        }


        public async Task<TChild?> GetChildRecordByIdWithJoinAsPersonAsync<TParent, TChild, TJoin>(
            string parentForeignKey,
            string childForeignKey,
            string childId,
            string personId,
            bool includePublic = false
        )
        where TParent : OwnedEntity, new()
        where TChild : BaseEntity, new()
        where TJoin : BaseEntity, new()
        {
            // Validate inputs
            ValidateNotNull(parentForeignKey, nameof(parentForeignKey));
            ValidateNotNull(childForeignKey, nameof(childForeignKey));
            ValidateNotNull(childId, nameof(childId));
            ValidateNotNull(personId, nameof(personId));

            // Step 1: Query the join table to ensure the child is related to the parent
            var joinRecords = await _dataAdapter.QueryRecordsAsync<TJoin>(
                new Dictionary<string, object> { { childForeignKey, childId } }
            );

            if (!(joinRecords?.Any() ?? false))
            {
                throw new KeyNotFoundException($"Child record with ID {childId} not found in the join table.");
            }

            // Step 2: Validate that the parent ID exists in the join records
            var parentId = joinRecords
                .Select(jr => typeof(TJoin).GetProperty(parentForeignKey)?.GetValue(jr)?.ToString())
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

            if (string.IsNullOrEmpty(parentId))
            {
                throw new KeyNotFoundException($"No parent record found in join table for child ID {childId}.");
            }

            // Step 3: Ensure the parent record is accessible to the person
            var parentRecord = await GetRecordByIdAsPersonAsync<TParent>(parentId, personId, includePublic);
            if (parentRecord == null)
            {
                throw new KeyNotFoundException($"Parent record with ID {parentId} not found or not accessible.");
            }

            // Step 4: Retrieve the child record
            var childRecord = await _dataAdapter.GetRecordByIdAsync<TChild>(childId);
            if (childRecord == null)
            {
                throw new KeyNotFoundException($"Child record with ID {childId} not found.");
            }

            return childRecord;
        }


        public async Task<IEnumerable<T>> SearchRecordsAsSystemAsync<T>(
            IEnumerable<string> searchFields,
            string searchTerm,
            int? skip = null,
            int? take = null
        )
            where T : BaseEntity, new()
        {
            ValidateNotNull(searchFields, nameof(searchFields));
            return await _dataAdapter.SearchRecordsAsync<T>(searchFields, searchTerm, skip, take);
        }

        public async Task<IEnumerable<T>> SearchRecordsAsPersonAsync<T>(
            IEnumerable<string> searchFields,
            string searchTerm,
            string personId,
            bool includePublic = false,
            int? skip = null,
            int? take = null
        )
            where T : OwnedEntity, new()
        {
            ValidateNotNull(searchFields, nameof(searchFields));
            ValidateNotNull(personId, nameof(personId));
            return await _dataAdapter.SearchOwnedRecordsAsync<T>(searchFields, searchTerm, personId, includePublic, skip, take);
        }

        public async Task<IEnumerable<TChild>> SearchChildRecordsWithJoinAsPersonAsync<TParent, TChild, TJoin>(
            string parentForeignKey,
            string childForeignKey,
            string personId,
            string? searchTerm = null,
            IEnumerable<string>? searchFields = null,
            bool includePublic = false,
            int? skip = null,
            int? take = null
        )
            where TParent : OwnedEntity, new()
            where TChild : BaseEntity, new()
            where TJoin : BaseEntity, new()
        {
            // Validate required parameters
            ValidateNotNull(parentForeignKey, nameof(parentForeignKey));
            ValidateNotNull(childForeignKey, nameof(childForeignKey));
            ValidateNotNull(personId, nameof(personId));

            // Step 1: Fetch Parent IDs
            var parentIds = await _dataAdapter.GetOwnedParentIdsAsync<TParent>(personId, includePublic);
            if (!parentIds.Any())
            {
                return Enumerable.Empty<TChild>();
            }

            // Step 2: Fetch Child IDs via Join Table
            var childIds = await _dataAdapter.GetChildIdsFromJoinAsync<TJoin>(parentForeignKey, parentIds, childForeignKey);
            if (!childIds.Any())
            {
                return Enumerable.Empty<TChild>();
            }

            // Step 3: Fetch Filtered Child Records
            return await _dataAdapter.GetFilteredChildrenAsync<TChild>(childIds, searchTerm, searchFields, skip, take);
        }



        public async Task<IEnumerable<T>> QueryRecordsAsSystemAsync<T>(Dictionary<string, object> criteria, int? skip = null, int? take = null)
            where T : BaseEntity, new()
        {
            ValidateNotNull(criteria, nameof(criteria));
            return await _dataAdapter.QueryRecordsAsync<T>(criteria, skip, take);
        }

        public async Task<IEnumerable<T>> QueryRecordsAsPersonAsync<T>(Dictionary<string, object> criteria, string personId, bool includePublic = false, int? skip = null, int? take = null)
            where T : OwnedEntity, new()
        {
            ValidateNotNull(criteria, nameof(criteria));
            ValidateNotNull(personId, nameof(personId));
            return await _dataAdapter.QueryOwnedRecordsAsync<T>(criteria, personId, includePublic, skip, take);
        }

        // *** UPDATE *** //

        public async Task<T> UpdateRecordAsSystemAsync<T>(T record) where T : BaseEntity, new()
        {
            ValidateNotNull(record, nameof(record));
            return await _dataAdapter.UpdateRecordAsync(record);
        }

        public async Task<T> UpdateRecordAsPersonAsync<T>(T record, string personId) where T : OwnedEntity, new()
        {
            ValidateNotNull(record, nameof(record));
            ValidateNotNull(personId, nameof(personId));

            var existing = await GetRecordByIdAsPersonAsync<T>(record.Id, personId);

            if (existing == null)
            {
                throw new KeyNotFoundException("Record not found or access denied.");
            }

            record.OwnerId = existing.OwnerId; // Preserve

            return await _dataAdapter.UpdateRecordAsync(record);
        }

        // *** DELETE *** //

        public async Task<bool> DeleteRecordAsSystemAsync<T>(string id) where T : BaseEntity, new()
        {
            ValidateNotNull(id, nameof(id));
            return await _dataAdapter.DeleteRecordAsync<T>(id);
        }

        public async Task<bool> DeleteRecordAsPersonAsync<T>(string id, string personId) where T : OwnedEntity, new()
        {
            ValidateNotNull(id, nameof(id));
            ValidateNotNull(personId, nameof(personId));

            var existing = await GetRecordByIdAsPersonAsync<T>(id, personId);
            if (existing == null)
            {
                throw new KeyNotFoundException("Record not found or access denied.");
            }

            return await _dataAdapter.DeleteRecordAsync<T>(id);
        }
    }

}