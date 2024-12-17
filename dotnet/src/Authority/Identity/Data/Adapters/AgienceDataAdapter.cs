using Agience.Core.Models.Entities.Abstract;
using Agience.Core.Models.Enums;
using LinqKit;
using Microsoft.EntityFrameworkCore;


namespace Agience.Authority.Identity.Data.Adapters
{
    /// <summary>
    /// Represents a data adapter for Agience, responsible for managing data operations 
    /// such as retrieval, insertion, and updates for Agience-related data sources.
    /// </summary>
    public class AgienceDataAdapter //: IAgienceDataAdapter
    {
        private readonly AgienceDbContext _dbContext;
        private readonly AgienceIdProvider _idProvider;
        private readonly ILogger<AgienceDataAdapter> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgienceDataAdapter"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used for data operations.</param>
        /// <param name="idProvider">The provider responsible for generating identifiers.</param>
        /// <param name="logger">The logger used for logging information and errors.</param>
        /// <returns>
        /// A new instance of <see cref="AgienceDataAdapter"/>.
        /// </returns>
        public AgienceDataAdapter(AgienceDbContext dbContext, AgienceIdProvider idProvider, ILogger<AgienceDataAdapter> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // *** HELPER METHODS *** //

        /// <summary>
        /// Determines whether the specified entity type is a 
        private static bool IsPublic<T>() where T : BaseEntity => typeof(PublicEntity).IsAssignableFrom(typeof(T));


        /// <summary>
        /// Applies pagination to the provided IQueryable by skipping a specified number of records 
        /// and taking a defined number of records from the query.
        /// </summary>
        /// <param name="query">The IQueryable sequence to apply pagination on.</param>
        /// <param name="skip">The number of records to skip. Pass null to skip no records.</param>
        /// <param name="take">The maximum number of records to take. Pass null to take all remaining records.</param>
        /// <returns>
        /// An IQueryable<T> that represents the paginated result of the original query.
        /// </returns>
        private IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int? skip, int? take)
        {
            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query;
        }

        public IQueryable<T> GetQueryable<T>() where T : class
        {
            return _dbContext.Set<T>().AsQueryable(); // Assuming _dbContext is the DbContext instance in AgienceDataAdapter
        }

        // *** CREATE OPERATIONS *** //

        /// <summary>
        /// Asynchronously creates a collection of records of type T in the database.
        /// Each record's ID is generated using an ID provider if it is not already set.
        /// </summary>
        /// <typeparam name="T">The type of the records to be created, which must inherit from BaseEntity and have a parameterless constructor.</typeparam>
        /// <param name="records">The collection of records to be created.</param>
        /// <returns>A task that represents the asynchronous operation, containing the created records.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the records collection is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when any record has a non-null ID.</exception>
        public async Task<IEnumerable<T>> CreateRecordsAsync<T>(IEnumerable<T> records) where T : BaseEntity, new()
        {
            if (records == null || !records.Any()) throw new ArgumentNullException(nameof(records));

            foreach (var record in records)
            {
                if (!string.IsNullOrWhiteSpace(record.Id)) throw new InvalidOperationException("Record IDs must be null.");
                record.Id = _idProvider.GenerateId(typeof(T).Name);
                record.CreatedDate = DateTime.UtcNow;
            }

            await _dbContext.SaveEntitiesAsync(records, true);
            _logger.LogInformation($"Created {records.Count()} {typeof(T).Name} records.");
            return records;
        }

        /// <summary>
        /// Asynchronously creates a new record of type T in the database.
        /// The record's ID is generated if it is not already set.
        /// </summary>
        /// <typeparam name="T">The type of the record, which must inherit from BaseEntity.</typeparam>
        /// <param name="record">The record to create. Must not be null and must not have an existing ID.</param>
        /// <returns>A task representing the asynchronous operation, with the created record upon completion.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the record parameter is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the record ID is already set.</exception>
        public async Task<T> CreateRecordAsync<T>(T record) where T : BaseEntity, new()
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (!string.IsNullOrWhiteSpace(record.Id)) throw new InvalidOperationException("Record ID must be null.");

            record.Id = _idProvider.GenerateId(typeof(T).Name);
            record.CreatedDate = DateTime.UtcNow;

            await _dbContext.SaveEntityAsync(record, true);
            _logger.LogInformation($"Created {typeof(T).Name} record with ID: {record.Id}");
            return record;
        }
        // *** GET OPERATIONS *** //


        /// <summary>
        /// Asynchronously retrieves a collection of owned records of type T based on specified criteria for a given personId.
        /// </summary>
        /// <typeparam name="T">The type of records to retrieve, constrained to be a subclass of BaseEntity.</typeparam>
        /// <param name="criteria">A dictionary containing the criteria for filtering the records.</param>
        /// <param name="personId">The identifier of the owner whose records are to be queried.</param>
        /// <param name="includePublic">Indicates whether to include public records in the results.</param>
        /// <param name="skip">An optional parameter to specify the number of records to skip.</param>
        /// <param name="take">An optional parameter to specify the maximum number of records to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, containing an enumerable collection of records of type T.</returns>
        public async Task<IEnumerable<T>> QueryOwnedRecordsAsync<T>(
            Dictionary<string, object> criteria,
            string personId,
            bool includePublic = false,
            int? skip = null,
            int? take = null) where T : OwnedEntity, new()
        {
            if (criteria == null || !criteria.Any())
                throw new ArgumentException("Criteria cannot be null or empty.", nameof(criteria));
            if (string.IsNullOrWhiteSpace(personId))
                throw new ArgumentException("Person ID cannot be null or empty.", nameof(personId));           

            var query = _dbContext.Set<T>().AsQueryable();
            var predicate = PredicateBuilder.New<T>(true); // Start with 'true' for building AND conditions

            // Apply criteria
            foreach (var kvp in criteria)
            {
                var property = typeof(T).GetProperty(kvp.Key);
                if (property == null)
                    throw new InvalidOperationException($"Property '{kvp.Key}' does not exist on type '{typeof(T).Name}'.");

                var value = kvp.Value;
                predicate = predicate.And(x => EF.Property<object>(x, kvp.Key).Equals(value));
            }

            // Apply ownership filter
            if (includePublic && typeof(PublicEntity).IsAssignableFrom(typeof(T)))
            {
                predicate = predicate.And(x =>
                    EF.Property<string>(x, "OwnerId") == personId ||
                    EF.Property<Visibility>(x, "Visibility") == Visibility.Public);
            }
            else
            {
                predicate = predicate.And(x => EF.Property<string>(x, "OwnerId") == personId);
            }

            query = query.Where(predicate);

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                 .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            query = ApplyPagination(query, skip, take);

            //_logger.LogDebug($"QueryOwnedRecords query for {typeof(T).Name}: {query.Expression}");
            return await query.ToListAsync();
        }


        /// <summary>
        /// Asynchronously queries records from the database based on specified criteria, with optional pagination.
        /// </summary>
        /// <typeparam name="T">The entity type to query, which must inherit from <c>BaseEntity</c>.</typeparam>
        /// <param name="criteria">A dictionary of property names and their corresponding values to filter the records.</param>
        /// <param name="skip">The number of records to skip (for pagination purposes).</param>
        /// <param name="take">The maximum number of records to take (for pagination purposes).</param>
        /// <returns>A task that represents the asynchronous operation, containing a collection of records that match the specified criteria.</returns>
        public async Task<IEnumerable<T>> QueryRecordsAsync<T>(Dictionary<string, object> criteria, int? skip = null, int? take = null) where T : BaseEntity, new()
        {
            if (criteria == null || !criteria.Any())
            {
                throw new ArgumentException("Criteria cannot be null or empty.", nameof(criteria));
            }

            var query = _dbContext.Set<T>().AsQueryable();
            var predicate = PredicateBuilder.New<T>(true); // Start with 'true' to allow 'And' conditions

            foreach (var kvp in criteria)
            {
                var property = typeof(T).GetProperty(kvp.Key);
                if (property == null)
                {
                    throw new InvalidOperationException($"Property '{kvp.Key}' does not exist on type '{typeof(T).Name}'.");
                }

                var value = kvp.Value;
                predicate = predicate.And(x => EF.Property<object>(x, kvp.Key).Equals(value));
            }

            query = query.Where(predicate);

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                 .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            query = ApplyPagination(query, skip, take);

            //_logger.LogDebug($"QueryRecords query for {typeof(T).Name}: {query.Expression}");
            return await query.ToListAsync();
        }


        /// <summary>
        /// Asynchronously searches for owned records of type T based on the provided search fields and term.
        /// </summary>
        /// <typeparam name="T">The type of the entity that derives from BaseEntity.</typeparam>
        /// <param name="searchFields">The fields to search against within the entity.</param>
        /// <param name="searchTerm">The term to search for within the specified fields.</param>
        /// <param name="personId">The ID of the person whose records are being searched.</param>
        /// <param name="includePublic">Indicates whether to include 
        public async Task<IEnumerable<T>> SearchOwnedRecordsAsync<T>(
            IEnumerable<string> searchFields,
            string searchTerm,
            string personId,
            bool includePublic = false,
            int? skip = null,
            int? take = null) where T : OwnedEntity, new()
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be null or empty.", nameof(searchTerm));
            if (string.IsNullOrWhiteSpace(personId))
                throw new ArgumentException("Person ID cannot be null or empty.", nameof(personId));
                      

            var query = _dbContext.Set<T>().AsQueryable();
            var predicate = PredicateBuilder.New<T>(false);

            foreach (var field in searchFields)
            {
                var property = typeof(T).GetProperty(field);
                if (property == null || property.PropertyType != typeof(string))
                    throw new InvalidOperationException($"Invalid search field: {field}");

                predicate = predicate.Or(x => EF.Functions.Like(EF.Property<string>(x, field), $"%{searchTerm}%"));
            }

            query = query.Where(predicate);

            if (includePublic && typeof(PublicEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(x =>
                    EF.Property<string>(x, "OwnerId") == personId ||
                    EF.Property<Visibility>(x, "Visibility") == Visibility.Public);
            }
            else
            {
                query = query.Where(x => EF.Property<string>(x, "OwnerId") == personId);
            }

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                 .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            query = ApplyPagination(query, skip, take);
            //_logger.LogDebug($"SearchOwnedRecords query for {typeof(T).Name}: {query.Expression}");
            return await query.ToListAsync();
        }

        /// <summary>
        /// Asynchronously searches for records of type T in the database based on specified search fields and a search term.
        /// The search is conducted by applying a "like" filter to the fields, and results can be paginated.
        /// </summary>
        /// <typeparam name="T">The type of the records to search, constrained to BaseEntity.</typeparam>
        /// <param name="searchFields">A collection of field names to search within the entity.</param>
        /// <param name="searchTerm">The term to search for within the specified fields.</param>
        /// <param name="skip">The number of records to skip for pagination (optional).</param>
        /// <param name="take">The number of records to take for pagination (optional).</param>
        /// <returns>A task that represents the asynchronous operation, containing a collection of records that match the search criteria.</returns>
        /// <exception cref="ArgumentException">Thrown when the search term is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when an invalid search field is specified.</exception>
        public async Task<IEnumerable<T>> SearchRecordsAsync<T>(
            IEnumerable<string> searchFields,
            string searchTerm,
            int? skip = null,
            int? take = null) where T : BaseEntity, new()
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term cannot be empty.", nameof(searchTerm));
            }

            var query = _dbContext.Set<T>().AsQueryable();
            var predicate = PredicateBuilder.New<T>(false);

            foreach (var field in searchFields)
            {
                var property = typeof(T).GetProperty(field);
                if (property == null || property.PropertyType != typeof(string))
                {
                    throw new InvalidOperationException($"Invalid search field: {field}");
                }

                predicate = predicate.Or(x => EF.Functions.Like(EF.Property<string>(x, field), $"%{searchTerm}%"));
            }

            query = query.Where(predicate);

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                 .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            query = ApplyPagination(query, skip, take);

            //_logger.LogDebug($"Search query for {typeof(T).Name}: {query.Expression}");
            return await query.ToListAsync();
        }

        /// <summary>
        /// Retrieves all records of type T from the database asynchronously, 
        /// applying optional pagination parameters for skipping and taking a specified number of records.
        /// </summary>
        /// <typeparam name="T">The type of the records to retrieve, which must inherit from BaseEntity.</typeparam>
        /// <param name="skip">The number of records to skip (optional).</param>
        /// <param name="take">The maximum number of records to take (optional).</param>
        /// <returns>A task that represents the asynchronous operation, containing a collection of records of type T.</returns>
        public async Task<IEnumerable<T>> GetAllRecordsAsync<T>(int? skip = null, int? take = null) where T : BaseEntity, new()
        {
            var query = _dbContext.Set<T>().AsQueryable();

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                 .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            query = ApplyPagination(query, skip, take);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a collection of records of type T from the database based on the specified record IDs.
        /// The method throws an exception if the provided record IDs are null or empty.
        /// </summary>
        /// <typeparam name="T">The type of the records to retrieve, which must inherit from BaseEntity and have a parameterless constructor.</typeparam>
        /// <param name="recordIds">The collection of record IDs to filter the records by.</param>
        /// <returns>A task that represents the asynchronous operation, with a value of an enumerable collection of records of type T.</returns>
        public async Task<IEnumerable<T>> GetRecordsByIdsAsync<T>(IEnumerable<string> recordIds) where T : BaseEntity, new()
        {
            if (recordIds == null || !recordIds.Any())
                throw new ArgumentException("Record IDs cannot be null or empty.", nameof(recordIds));

            return await _dbContext.Set<T>()
                .Where(x => recordIds.Contains(x.Id))
                .OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null) // Nulls first
                .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate")) // Sort ascending by CreatedDate
                .ToListAsync();
        }



        /// <summary>
        /// Retrieves all records owned by the specified owner ID, with an option to include 
        public async Task<IEnumerable<T>> GetAllOwnedRecordsAsync<T>(
            string ownerId,
            bool includePublic = false,
            int? skip = null,
            int? take = null) where T : OwnedEntity, new()
        {
            if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("Owner ID cannot be null or whitespace.", nameof(ownerId));

            var query = _dbContext.Set<T>().AsQueryable();

            if (includePublic && IsPublic<T>())
            {
                query = query.Where(x => EF.Property<string>(x, "OwnerId") == ownerId || EF.Property<Visibility>(x, "Visibility") == Visibility.Public);
            }
            else
            {
                query = query.Where(x => EF.Property<string>(x, "OwnerId") == ownerId);
            }

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            query = ApplyPagination(query, skip, take);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a record of type <typeparamref name="T"/> by its identifier.
        /// </summary>
        /// <typeparam name="T">The type of the record that inherits from <see cref="BaseEntity"/>.</typeparam>
        /// <param name="recordId">The identifier of the record to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation, containing the record of type <typeparamref name="T"/> if found; otherwise, null.</returns>

        public async Task<T?> GetRecordByIdAsync<T>(string id) where T : BaseEntity
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("The record ID cannot be null or whitespace.", nameof(id));
            }

            return await _dbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id);
        }

        /// <summary>
        /// Asynchronously retrieves an owned record by its ID and the owner's person ID.
        /// Validates the provided record ID and person ID, and checks for ownership of the entity type.
        /// If specified, it can include 
        public async Task<T?> GetOwnedRecordByIdAsync<T>(
        string recordId,
        string personId,
        bool includePublic = false) where T : OwnedEntity, new()
        {
            if (string.IsNullOrWhiteSpace(recordId))
                throw new ArgumentException("Record ID cannot be null or empty.", nameof(recordId));
            if (string.IsNullOrWhiteSpace(personId))
                throw new ArgumentException("Person ID cannot be null or empty.", nameof(personId));

            var query = _dbContext.Set<T>().AsQueryable();

            if (includePublic && typeof(PublicEntity).IsAssignableFrom(typeof(T)))
            {
                query = query.Where(x =>
                    x.Id == recordId &&
                    (EF.Property<string>(x, "OwnerId") == personId ||
                     EF.Property<Visibility>(x, "Visibility") == Visibility.Public));
            }
            else
            {
                query = query.Where(x =>
                    x.Id == recordId &&
                    EF.Property<string>(x, "OwnerId") == personId);
            }

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            //_logger.LogDebug($"GetOwnedRecordById query for {typeof(T).Name}: {query.Expression}");
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<string>> GetOwnedParentIdsAsync<TParent>(
            string personId,
            bool includePublic = false
        ) where TParent : OwnedEntity, new()
        {
            var query = _dbContext.Set<TParent>().AsQueryable();

            query = query.Where(p => EF.Property<string>(p, "OwnerId") == personId);

            if (includePublic && typeof(PublicEntity).IsAssignableFrom(typeof(TParent)))
            {
                query = query.Where(p => EF.Property<Visibility>(p, "Visibility") == Visibility.Public);
            }

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            return await query.Select(p => p.Id).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<string>> GetChildIdsFromJoinAsync<TJoin>(
            string parentForeignKey,
            IEnumerable<string> parentIds,
            string childForeignKey
        ) where TJoin : BaseEntity, new()
        {
            if (parentIds == null || !parentIds.Any())
                return Enumerable.Empty<string>();

            var query = _dbContext.Set<TJoin>()
                .Where(j => parentIds.Contains(EF.Property<string>(j, parentForeignKey)));

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            return await query.Select(j => EF.Property<string>(j, childForeignKey)).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<TChild>> GetFilteredChildrenAsync<TChild>(
            IEnumerable<string> childIds,
            string? searchTerm = null,
            IEnumerable<string>? searchFields = null,
            int? skip = null,
            int? take = null
        ) where TChild : BaseEntity, new()
        {
            if (childIds == null || !childIds.Any())
                return Enumerable.Empty<TChild>();

            var query = _dbContext.Set<TChild>().Where(c => childIds.Contains(c.Id));

            if (!string.IsNullOrEmpty(searchTerm) && searchFields != null && searchFields.Any())
            {
                var searchPredicate = PredicateBuilder.New<TChild>(false);

                foreach (var field in searchFields)
                {
                    var property = typeof(TChild).GetProperty(field);
                    if (property == null || property.PropertyType != typeof(string))
                        throw new InvalidOperationException($"Invalid search field: {field}");

                    searchPredicate = searchPredicate.Or(c =>
                        EF.Functions.Like(EF.Property<string>(c, field), $"%{searchTerm}%"));
                }

                query = query.Where(searchPredicate);
            }

            query = query.OrderBy(x => EF.Property<DateTime?>(x, "CreatedDate") == null)
                .ThenBy(x => EF.Property<DateTime?>(x, "CreatedDate"));

            query = ApplyPagination(query, skip, take);

            return await query.ToListAsync();
        }



        // *** UPDATE OPERATIONS *** //

        /// <summary>
        /// Asynchronously updates a record in the database.
        /// </summary>
        /// <typeparam name="T">The type of the record, constrained to BaseEntity.</typeparam>
        /// <param name="record">The record to be updated.</param>
        /// <returns>The updated record.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the record is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the record ID is null or whitespace.</exception>
        public async Task<T> UpdateRecordAsync<T>(T record) where T : BaseEntity, new()
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            if (string.IsNullOrWhiteSpace(record.Id)) throw new InvalidOperationException("Record ID cannot be null or whitespace.");

            if (record.CreatedDate != null) throw new InvalidOperationException("CreatedDate must be null for an update.");
                        
            var dbRecord = await GetRecordByIdAsync<T>(record.Id);

            // TODO: Verify we got back the same record

            record.CreatedDate = dbRecord?.CreatedDate;            

            await _dbContext.SaveEntityAsync(record, false);
            _logger.LogInformation($"Updated {typeof(T).Name} record with ID: {record.Id}");
            return record;
        }

        /// <summary>
        /// Asynchronously updates a collection of records in the database.
        /// </summary>
        /// <typeparam name="T">The type of the records, which must derive from BaseEntity.</typeparam>
        /// <param name="records">An IEnumerable of records to be updated.</param>
        /// <returns>An IEnumerable of the updated records.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the records collection is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a record ID is null or whitespace.</exception>
        public async Task<IEnumerable<T>> UpdateRecordsAsync<T>(IEnumerable<T> records) where T : BaseEntity, new()
        {
            if (records == null || !records.Any()) throw new ArgumentNullException(nameof(records));

            var recordDict = new Dictionary<string, T>();            

            foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record.Id))
                {
                    throw new InvalidOperationException("Record ID must not be null or whitespace.");
                }

                if (record.CreatedDate == null)
                {
                    throw new InvalidOperationException("CreatedDate must be null for an update.");
                }

                recordDict.Add(record.Id, record);
            }

            foreach (var dbRecord in await GetRecordsByIdsAsync<T>(recordDict.Keys))
            {
                // TODO: Verify we got all the same records back.

                if (dbRecord.CreatedDate == null)
                {
                    recordDict[dbRecord.Id].CreatedDate = dbRecord.CreatedDate;
                }
            }

            await _dbContext.SaveEntitiesAsync(records, false);

            _logger.LogInformation($"Updated {records.Count()} {typeof(T).Name} records.");
            return records;
        }

        // *** DELETE OPERATIONS *** //

        /// <summary>
        /// Asynchronously deletes a record of type T from the database given its record ID.
        /// </summary>
        /// <typeparam name="T">The type of the record, which must inherit from BaseEntity.</typeparam>
        /// <param name="recordId">The ID of the record to be deleted.</param>
        /// <returns>A task that represents the asynchronous operation, containing a boolean value indicating whether the deletion was successful.</returns>
        /// <exception cref="ArgumentException">Thrown when the record ID is null or whitespace.</exception>
        public async Task<bool> DeleteRecordAsync<T>(string recordId) where T : BaseEntity, new()
        {
            if (string.IsNullOrWhiteSpace(recordId)) throw new ArgumentException("Record ID cannot be null or whitespace.", nameof(recordId));

            var record = await GetRecordByIdAsync<T>(recordId);
            if (record == null) return false;

            _dbContext.Set<T>().Remove(record);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Deleted {typeof(T).Name} record with ID: {recordId}");
            return true;
        }

        /// <summary>
        /// Deletes a collection of records identified by their IDs from the database asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the entity that extends BaseEntity.</typeparam>
        /// <param name="recordIds">An enumerable collection of record IDs to be deleted.</param>
        /// <returns>Task that represents the asynchronous operation, containing true if records were deleted, otherwise false.</returns>
        /// <exception cref="ArgumentException">Thrown when the recordIds parameter is null or empty.</exception>
        public async Task<bool> DeleteRecordsAsync<T>(IEnumerable<string> recordIds) where T : BaseEntity, new()
        {
            if (recordIds == null || !recordIds.Any()) throw new ArgumentException("Record IDs cannot be null or empty.", nameof(recordIds));

            var records = await GetRecordsByIdsAsync<T>(recordIds);
            if (!records.Any()) return false;

            _dbContext.Set<T>().RemoveRange(records);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Deleted {records.Count()} {typeof(T).Name} records.");
            return true;
        }
    }
}