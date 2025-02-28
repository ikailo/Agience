using Agience.Authority.Manage.Models;
using Agience.Core.Models.Entities.Abstract;

namespace Agience.Authority.Manage.Services
{
    public class ContextService
    {
        public event Func<string?, Task>? ContextRecordIdChanged;
        public event Func<Type, Task>? RecordsUpdated;
        public event Func<Task>? ChildRecordsUpdated;

        private string? _contextRecordId;
        private Type? _contextEntityType;

        public string? ContextRecordId => _contextRecordId;
        public Type? ContextEntityType => _contextEntityType;
        public BaseEntity? ContextRecord => _records.FirstOrDefault(r => r.Id == _contextRecordId);

        public Dictionary<Type, List<BaseEntity>> ContextChildRecords =>
        _contextRecordId != null && _childRecords.TryGetValue(_contextRecordId, out var childRecords)
            ? childRecords
            : new();

        private readonly List<BaseEntity> _records = new();
        private readonly Dictionary<string, Dictionary<Type, List<BaseEntity>>> _childRecords = new();
        private readonly RecordHandler _recordHandler;

        public ContextService(RecordHandler recordHandler)
        {
            _recordHandler = recordHandler;
        }

        public IEnumerable<BaseEntity> Records => _records;

        public List<TChild> GetChildRecords<TChild>(string parentId) where TChild : BaseEntity, new()
        {
            return EnsureChildRecords(parentId, typeof(TChild)).Cast<TChild>().ToList();
        }

        public async Task SetContext<TParent>(string? recordId) where TParent : BaseEntity, new()
        {
            if (_contextRecordId != null)
            {
                _childRecords.Remove(_contextRecordId);
                ChildRecordsUpdated?.Invoke();
            }

            _contextRecordId = recordId;
            _contextEntityType = typeof(TParent);

            if (recordId == null)
            {
                _records.Clear();
                RecordsUpdated?.Invoke(typeof(TParent));
            }

            if (ContextRecordIdChanged != null)
            {
                await ContextRecordIdChanged.Invoke(recordId);
            }   
        }

        public async Task FetchRecordsAsync<TParent>(int pageNumber = 1, int pageSize = 10) where TParent : BaseEntity, new()
        {
            try
            {
                var records = await _recordHandler.FetchRecordsAsync<TParent>(null, pageNumber, pageSize);

                _records.Clear();
                _records.AddRange(records);

                EnsurePlaceholderRecord<TParent>();
                RecordsUpdated?.Invoke(typeof(TParent));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching records: {ex.Message}");
            }
        }

        public async Task FetchChildRecordsAsync<TParent, TChild>(Func<IEnumerable<TChild>, Task>? fetchPostHook = null, bool addPlaceholder = true, int pageNumber = 1, int pageSize = 10)
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
        {
            if (string.IsNullOrEmpty(_contextRecordId) || typeof(TParent) != _contextEntityType)
                return;

            try
            {
                var childRecords = await _recordHandler.FetchChildRecordsAsync<TParent, TChild>(_contextRecordId, fetchPostHook, pageNumber, pageSize);
                var records = EnsureChildRecords(_contextRecordId, typeof(TChild));

                records.Clear();
                records.AddRange(childRecords);

                if (addPlaceholder)
                {
                    EnsurePlaceholderChildRecord<TParent, TChild>();
                }
                

                ChildRecordsUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching child records: {ex.Message}");
            }
        }

        public async Task AddChildRecordAsNew<TParent, TChild>(TChild newRecord)
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()    
        {
            var childRecord = ContextChildRecords[typeof(TChild)].FirstOrDefault(f => f.Id == "new");
                        
            if (childRecord == null) return;

            // Copy properties from record to childRecord
            foreach (var property in typeof(TChild).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
            {
                property.SetValue(childRecord, property.GetValue(newRecord));
            }

            EnsurePlaceholderChildRecord<TParent, TChild>();
            ChildRecordsUpdated?.Invoke();

            await Task.CompletedTask;
        }


        public async Task SaveRecordAsync<TParent>(TParent record) where TParent : BaseEntity, new()
        {
            try
            {
                record.Id = record.Id == "new" ? null : record.Id;
                await _recordHandler.SaveRecordAsync(record);
                EnsurePlaceholderRecord<TParent>();
                RecordsUpdated?.Invoke(typeof(TParent));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving record: {ex.Message}");
            }
        }



        public async Task SaveChildRecordAsync<TParent, TChild>(TChild childRecord, Func<TChild, Task<bool>>? savePreHook = null, Func<TChild, Task>? savePostHook = null)
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
        {
            if (string.IsNullOrEmpty(_contextRecordId) || typeof(TParent) != _contextEntityType)
                return;

            try
            {
                var records = EnsureChildRecords(_contextRecordId, typeof(TChild));
                childRecord.Id = childRecord.Id == "new" ? null : childRecord.Id;

                await _recordHandler.SaveChildRecordAsync<TParent, TChild>(_contextRecordId, childRecord, savePreHook, savePostHook);

                EnsurePlaceholderChildRecord<TParent, TChild>();
                ChildRecordsUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving child record: {ex.Message}");
            }
        }

        public async Task DeleteRecordAsync<TParent>(string recordId) where TParent : BaseEntity, new()
        {
            if (string.IsNullOrEmpty(recordId) || recordId == "new")
                return;

            try
            {
                if (await _recordHandler.DeleteRecordAsync<TParent>(recordId))
                {
                    _records.RemoveAll(r => r.Id == recordId);
                    RecordsUpdated?.Invoke(typeof(TParent));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting record {recordId}: {ex.Message}");
            }
        }

        public async Task DeleteChildRecordAsync<TParent, TChild>(string childRecordId)
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
        {
            if (string.IsNullOrEmpty(_contextRecordId) || typeof(TParent) != _contextEntityType || string.IsNullOrEmpty(childRecordId) || childRecordId == "new")
                return;

            try
            {
                var records = EnsureChildRecords(_contextRecordId, typeof(TChild));

                if (await _recordHandler.DeleteChildRecordAsync<TParent, TChild>(_contextRecordId, childRecordId))
                {
                    records.RemoveAll(r => r.Id == childRecordId);
                    ChildRecordsUpdated?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting child record {childRecordId}: {ex.Message}");
            }
        }

        private List<BaseEntity> EnsureChildRecords(string parentId, Type childType)
        {
            if (!_childRecords.ContainsKey(parentId))
            {
                _childRecords[parentId] = new Dictionary<Type, List<BaseEntity>>();
            }

            if (!_childRecords[parentId].ContainsKey(childType))
            {
                _childRecords[parentId][childType] = new List<BaseEntity>();
            }

            return _childRecords[parentId][childType];
        }

        private void EnsurePlaceholderRecord<TParent>() where TParent : BaseEntity, new()
        {
            if (!_records.Any(r => r.Id == "new"))
            {
                _records.Add(new TParent { Id = "new" });
            }
        }

        private void EnsurePlaceholderChildRecord<TParent, TChild>()
            where TParent : BaseEntity, new()
            where TChild : BaseEntity, new()
        {
            var records = EnsureChildRecords(_contextRecordId, typeof(TChild));

            if (!records.Any(r => r.Id == "new"))
            {
                records.Add(new TChild { Id = "new" });
            }
        }
    }
}
