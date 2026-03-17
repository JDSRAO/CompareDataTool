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

        public Task<JObject> GetDataAsync(string type, string entity, string rowId)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(type);
            return dataRepository.GetDataAsync(entity, rowId);
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

        public Task<bool> RecordExistsAsync(string type, string entity, string rowId)
        {
            var dataRepository = this.dataSourceRepositoryFactory.GetDataSourceRepositoryService(type);
            return dataRepository.RecordExistsAsync(entity, rowId);
        }

        public Task SaveRecordCountMismatchAsync(string runId, string sourceEntity, string destinationEntity, int sourceCount, int destinationCount)
        {
            return this.appDataRepository.InsertEntityCountMismatchAsync(runId, sourceEntity, destinationEntity, sourceCount, destinationCount);
        }

        public Task SaveEntityRecordMismatchAsync(string runId, string rowId, string entity, string dataSourceType)
        {
            return this.appDataRepository.InsertEntityRecordMismatchAsync(runId, entity, rowId, dataSourceType == DataSourceTypes.Source, dataSourceType == DataSourceTypes.Destination);
        }

        public Task SaveEntityFieldMismatchAsync(string runId, string sourceEntity, string destinationEntity, string rowId, string sourceField, string destinationField, string sourceValue, string destinationValue)
        {
            return this.appDataRepository.InsertEntityFieldMismatchAsync(runId, sourceEntity, destinationEntity, rowId, sourceField, destinationField, sourceValue, destinationValue);
        }

        public FieldCompareResult CompareValues(JObject sourceRow, FieldMapping fieldMapping, JObject destinationRow)
        {
            var sourceValue = Convert.ToString(sourceRow[fieldMapping.SourceField]);
            var destinationValue = Convert.ToString(destinationRow[fieldMapping.DestinationField]);
            return new FieldCompareResult
            {
               Same = sourceValue.Equals(destinationValue),
               SourceValue = sourceValue,
               DestinationValue = destinationValue,
            };
        }
    }
}
