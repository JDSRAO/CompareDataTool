using CompareDataTool.Domain.Models;
using CompareDataTool.Domain.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CompareDataTool.App
{
    public class Orchestrator
    {
        private readonly ILogger<Orchestrator> logger;
        private readonly DataCompareService dataCompareService;
        private readonly AppConfiguration appConfiguration;

        private readonly string RunId = Guid.NewGuid().ToString();

        public Orchestrator(ILogger<Orchestrator> logger, DataCompareService dataCompareService, AppConfiguration appConfiguration)
        {
            this.dataCompareService = dataCompareService;
            this.logger = logger;
            this.appConfiguration = appConfiguration;
        }

        public async Task RunAsync()
        {
            foreach (var entityMapping in this.appConfiguration.EntityMappings)
            {
                var sourceCount = await this.dataCompareService.GetCountAsync(this.appConfiguration.EnvironmentSettings.Source.Type, entityMapping.SourceEntity);
                var destinationCount = await this.dataCompareService.GetCountAsync(this.appConfiguration.EnvironmentSettings.Destination.Type, entityMapping.DestinationEntity);
                if (sourceCount != destinationCount)
                {
                    this.logger.LogInformation("Count mismatch");
                    await this.dataCompareService.SaveRecordCountMismatchAsync(RunId, entityMapping.SourceEntity, entityMapping.DestinationEntity, sourceCount, destinationCount);
                }
                await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Source.Type, entityMapping.SourceEntity, entityMapping.PrimaryKeyMapping.SourcePrimaryKey, entityMapping.DestinationEntity);
                await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Destination.Type, entityMapping.DestinationEntity, entityMapping.PrimaryKeyMapping.DestinationPrimaryKey, entityMapping.SourceEntity);
            }
        }

        private async Task GetDataToCompareAsync(string type, string sourceEntity, string sourcePrimaryKey, string destinationEntity)
        {
            this.logger.LogInformation($"Fetching data for type: {type} and entity: {sourceEntity} : Started");
            int pageNumber = 1;
            IEnumerable<JObject> rows;
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = this.appConfiguration.CompareSettings.MaxDegreeOfParallelism,
            };

            do
            {
                this.logger.LogInformation($"PageNumber : {pageNumber}");
                rows = await this.dataCompareService.GetDataAsync(type, sourceEntity, pageNumber, this.appConfiguration.CompareSettings.PageSize);
                await Parallel.ForEachAsync(rows, parallelOptions, async (row, token) =>
                {
                    try
                    {
                        this.logger.LogInformation("*");
                        await this.dataCompareService.SaveRowIdAsync(RunId, type, sourceEntity, row[sourcePrimaryKey].ToString());

                        var destinationType = type;
                        if (type == DataSourceTypes.Source)
                        {
                            destinationType = DataSourceTypes.Destination;
                        }
                        else
                        {
                            destinationType = DataSourceTypes.Source;
                        }

                        var exists = await this.dataCompareService.RecordExistsAsync(destinationType, destinationEntity, row[sourcePrimaryKey].ToString());


                        if (!exists)
                        {
                            await this.dataCompareService.SaveEntityRecordMismatchAsync(RunId, row[sourcePrimaryKey].ToString(), sourceEntity, type);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogDebug(row.ToString());
                        this.logger.LogError(ex, ex.Message);
                        throw;
                    }
                    
                });
                pageNumber++;
            }
            while (rows.Any());

            this.logger.LogInformation($"Fetching data for type: {type} and entity: {sourceEntity} : Completed");
            this.logger.LogInformation($"");
        }
    }
}
