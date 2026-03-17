using CompareDataTool.Domain.Models;
using CompareDataTool.Domain.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace CompareDataTool.App
{
    public class Orchestrator
    {
        private readonly ILogger<Orchestrator> logger;
        private readonly DataCompareService dataCompareService;
        private readonly AppConfiguration appConfiguration;

        private readonly string RunId = Guid.NewGuid().ToString();
        private Stopwatch stopwatch;

        public Orchestrator(ILogger<Orchestrator> logger, DataCompareService dataCompareService, AppConfiguration appConfiguration)
        {
            stopwatch = new Stopwatch();
            this.dataCompareService = dataCompareService;
            this.logger = logger;
            this.appConfiguration = appConfiguration;
        }

        public async Task RunAsync()
        {
            this.stopwatch.Start();
            foreach (var entityMapping in this.appConfiguration.EntityMappings)
            {
                var sourceCount = await this.dataCompareService.GetCountAsync(this.appConfiguration.EnvironmentSettings.Source.Type, entityMapping.SourceEntity);
                var destinationCount = await this.dataCompareService.GetCountAsync(this.appConfiguration.EnvironmentSettings.Destination.Type, entityMapping.DestinationEntity);
                if (sourceCount != destinationCount)
                {
                    this.logger.LogWarning("Count mismatch");
                    await this.dataCompareService.SaveRecordCountMismatchAsync(RunId, entityMapping.SourceEntity, entityMapping.DestinationEntity, sourceCount, destinationCount);
                }
                await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Source.Type, entityMapping.SourceEntity, entityMapping.PrimaryKeyMapping.SourcePrimaryKey, entityMapping.DestinationEntity);
                await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Destination.Type, entityMapping.DestinationEntity, entityMapping.PrimaryKeyMapping.DestinationPrimaryKey, entityMapping.SourceEntity);
            }

            this.stopwatch.Stop();
            var timeTaken = $"{this.stopwatch.Elapsed.Hours:00}:{this.stopwatch.Elapsed.Minutes:00}:{this.stopwatch.Elapsed.Seconds:00}:{this.stopwatch.Elapsed.Milliseconds / 10:00}";
            this.logger.LogInformation($"Total time taken: {timeTaken}");
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
                        if (exists)
                        {
                            var rowData = await this.dataCompareService.GetDataAsync(destinationType, destinationEntity, row[sourcePrimaryKey].ToString());
                        }
                        else
                        {
                            var rowData = await this.dataCompareService.GetDataAsync(destinationType, destinationEntity, row[sourcePrimaryKey].ToString());
                            this.logger.LogWarning("Mising record");
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
