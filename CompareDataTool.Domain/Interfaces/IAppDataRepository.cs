using CompareDataTool.Domain.Models;

namespace CompareDataTool.Domain.Interfaces
{
    public interface IAppDataRepository
    {
        public Task SaveRowIdAsync(string runId, string type, string entity, string rowId);

        public Task<IEnumerable<string>> GetRowIdAsync(string runId, string type, string entity, int pageNumber, int pageSize);

        public Task InsertEntityCountMismatchAsync(string runId, string sourceEntity, string destinationEntity, int sourceCount, int destinationCount);

        public Task InsertEntityRecordMismatchAsync(string runId, string entity, string rowId, bool existsInSource, bool existsInDestination);

        public Task InsertEntityFieldMismatchAsync(string runId, string sourceEntity, string destinationEntity, string rowId, string sourceField, string destinationField, string sourceValue, string destinationValue);

        public Task<IEnumerable<EntityCountMismatch>> GetCountMismatchesAsync(string runId, int pageNumber, int pageSize);

        public Task<IEnumerable<EntityRecordMismatch>> GetEntityRecordMismatchAsync(string runId, int pageNumber, int pageSize);

        public Task<IEnumerable<EntityFieldMismatch>> GetEntityFieldMismatchAsync(string runId, int pageNumber, int pageSize);
    }
}
