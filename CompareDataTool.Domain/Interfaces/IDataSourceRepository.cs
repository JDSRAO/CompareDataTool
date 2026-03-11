using Newtonsoft.Json.Linq;

namespace CompareDataTool.Domain.Interfaces
{
    public interface IDataSourceRepository
    {
        public Task<IEnumerable<JObject>> GetDataAsync(string entity, int pageNumber, int pageSize);

        public Task<int> GetCountAsync(string entity);

        public Task<bool> RecordExistsAsync(string entity, string rowId);
    }
}