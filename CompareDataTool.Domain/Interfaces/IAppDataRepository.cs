using Newtonsoft.Json.Linq;

namespace CompareDataTool.Domain.Interfaces
{
    public interface IAppDataRepository
    {
        public Task SaveRowForProcessingAsync(string runId, string type, string entity, JObject data);

        public Task<IEnumerable<JObject>> GetDataForProcessingAsync(string runId, string type, string entity, int pageNumber, int pageSize);
    }
}
