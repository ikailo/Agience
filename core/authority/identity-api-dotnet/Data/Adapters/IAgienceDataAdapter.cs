using Agience.Core.Models.Entities.Abstract;

public interface IAgienceDataAdapter
{
    Task<T> CreateRecordAsync<T>(T record) where T : BaseEntity, new();
    Task<IEnumerable<T>> CreateRecordsAsync<T>(IEnumerable<T> records) where T : BaseEntity, new();
    Task<bool> DeleteRecordAsync<T>(string recordId) where T : BaseEntity, new();
    Task<bool> DeleteRecordsAsync<T>(IEnumerable<string> recordIds) where T : BaseEntity, new();
    Task<IEnumerable<T>> GetAllOwnedRecordsAsync<T>(string ownerId, bool includePublic = false, int? skip = null, int? take = null) where T : BaseEntity, new();
    Task<IEnumerable<T>> GetAllRecordsAsync<T>(int? skip = null, int? take = null) where T : BaseEntity, new();
    Task<T?> GetOwnedRecordByIdAsync<T>(string recordId, string personId, bool includePublic = false) where T : BaseEntity, new();
    Task<T?> GetRecordByIdAsync<T>(string recordId) where T : BaseEntity, new();
    Task<IEnumerable<T>> GetRecordsByIdsAsync<T>(IEnumerable<string> recordIds) where T : BaseEntity, new();
    Task<IEnumerable<T>> QueryRecordsAsync<T>(Dictionary<string, object> criteria, int? skip = null, int? take = null) where T : BaseEntity, new();
    Task<IEnumerable<T>> SearchOwnedRecordsAsync<T>(IEnumerable<string> searchFields, string searchTerm, string personId, bool includePublic = false, int? skip = null, int? take = null) where T : BaseEntity, new();
    Task<IEnumerable<T>> SearchRecordsAsync<T>(IEnumerable<string> searchFields, string searchTerm, int? skip = null, int? take = null) where T : BaseEntity, new();
    Task<T> UpdateRecordAsync<T>(T record) where T : BaseEntity, new();
    Task<IEnumerable<T>> UpdateRecordsAsync<T>(IEnumerable<T> records) where T : BaseEntity, new();
}
