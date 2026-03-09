using Newtonsoft.Json.Linq;

namespace CompareDataTool.Domain.Interfaces
{
    public interface IDataRepository
    {
        public Task<JArray> GetDataAsync();
    }
}