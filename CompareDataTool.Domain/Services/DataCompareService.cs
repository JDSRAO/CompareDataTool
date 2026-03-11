using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using Newtonsoft.Json.Linq;

namespace CompareDataTool.Domain.Services
{
    public class DataCompareService
    {
        private readonly IDataSourceRepositoryFactory dataSourceRepositoryFactory;
        private readonly IAppDataRepository appDataRepository;
        private readonly AppConfiguration appConfiguration;

        public DataCompareService(IDataSourceRepositoryFactory dataSourceRepositoryFactory, AppConfiguration appConfiguration, IAppDataRepository appDataRepository)
        {
            this.dataSourceRepositoryFactory = dataSourceRepositoryFactory;
            this.appConfiguration = appConfiguration;
            this.appDataRepository = appDataRepository;
        }

        public Task<IEnumerable<JObject>> GetDataAsync(string type, string entity, int pageNumber, int pageSize)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(type);
            return dataRepository.GetDataAsync(entity, pageNumber, pageSize);
        }

        public Task<int> GetCountAsync(string type, string entity)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(type);
            return dataRepository.GetCountAsync(entity);
        }

        public Task SaveRecordCountMismatchAsync(string runId, string sourceEntity, string destinationEntity, int sourceCount, int destinationCount)
        {
            throw new NotImplementedException();
        }

        public Task SaveDataForProcessingAsync(string runId, string type, string entity, JObject data)
        {
            return this.appDataRepository.SaveRowForProcessingAsync(runId, type, entity, data);
        }
    }
}
