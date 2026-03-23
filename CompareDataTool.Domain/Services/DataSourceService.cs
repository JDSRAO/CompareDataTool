using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using Newtonsoft.Json.Linq;

namespace CompareDataTool.Domain.Services
{
    public class DataSourceService
    {
        private readonly IDataSourceRepositoryFactory dataSourceRepositoryFactory;
        private readonly AppConfiguration appConfiguration;

        public DataSourceService(IDataSourceRepositoryFactory dataSourceRepositoryFactory, AppConfiguration appConfiguration)
        {
            this.dataSourceRepositoryFactory = dataSourceRepositoryFactory;
            this.appConfiguration = appConfiguration;
        }

        public Task<IEnumerable<JObject>> GetDataAsync(string type, string entity, int pageNumber, int pageSize)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(type);
            return dataRepository.GetDataAsync(entity, pageNumber, pageSize);
        }

        public async Task<(bool, JObject)> GetDataAsync(string type, string entity, string rowId)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(type);
            var row = await dataRepository.GetDataAsync(entity, rowId);
            return (row.Count > 0, row);
        }

        public Task<IEnumerable<JObject>> GetSourceDataAsync(string entity, int pageNumber, int pageSize)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(DataSourceTypes.Source);
            return dataRepository.GetDataAsync(entity, pageNumber, pageSize);
        }

        public Task<IEnumerable<JObject>> GetDestinationDataAsync(string entity, int pageNumber, int pageSize)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(DataSourceTypes.Destination);
            return dataRepository.GetDataAsync(entity, pageNumber, pageSize);
        }

        public async Task<(bool, JObject)> GetSourceDataAsync(string entity, string rowId)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(DataSourceTypes.Source);
            var row = await dataRepository.GetDataAsync(entity, rowId);
            return (row.Count > 0, row);
        }

        public async Task<(bool, JObject)> GetDestinationDataAsync(string entity, string rowId)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(DataSourceTypes.Destination);
            var row = await dataRepository.GetDataAsync(entity, rowId);
            return (row.Count > 0, row);
        }
    }
}
