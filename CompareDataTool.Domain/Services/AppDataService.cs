using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;

namespace CompareDataTool.Domain.Services
{
    public class AppDataService
    {
        private readonly IAppDataRepository appDataRepository;
        
        public AppDataService(IAppDataRepository appDataRepository)
        {
            this.appDataRepository = appDataRepository;
        }

        public Task SaveRowIdAsync(string runId, string type, string entity, string rowId)
        {
            return this.appDataRepository.SaveRowIdAsync(runId, type, entity, rowId);
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

    }
}
