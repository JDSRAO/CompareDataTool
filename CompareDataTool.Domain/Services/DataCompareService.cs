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

        public Task SaveRowIdAsync(string runId, string type, string entity, string rowId)
        {
            return this.appDataRepository.SaveRowIdAsync(runId, type, entity, rowId);
        }

        public Task<int> GetCountAsync(string type, string entity)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(type);
            return dataRepository.GetCountAsync(entity);
        }

        public Task<bool> RecordExistsInSourceAsync(string entity, string rowId)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(DataSourceTypes.Source);
            return dataRepository.RecordExistsAsync(entity, rowId);
        }

        public Task<bool> RecordExistsInDestinationAsync(string entity, string rowId)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(DataSourceTypes.Destination);
            return dataRepository.RecordExistsAsync(entity, rowId);
        }

        public Task SaveRecordCountMismatchAsync(string runId, string sourceEntity, string destinationEntity, int sourceCount, int destinationCount)
        {
            return this.appDataRepository.InsertEntityCountMismatchAsync(runId, sourceEntity, destinationEntity, sourceCount, destinationCount);
        }

        public Task SaveFieldMismatchAsync(string runId, string sourceEntity, string destinationEntity, int sourceCount, int destinationCount)
        {
            throw new NotImplementedException();
        }

        public Task SaveEntityRecordMismatchAsync(string runId, string rowId, string entity, string dataSourceType)
        {
            return this.appDataRepository.InsertEntityRecordMismatchAsync(runId, entity, rowId, dataSourceType == DataSourceTypes.Source, dataSourceType == DataSourceTypes.Destination);
        }

        public Task SaveEntityFieldMismatchAsync(string runId, string sourceEntity, string destinationEntity, int sourceCount, int destinationCount)
        {
            throw new NotImplementedException();
        }
    }
}
