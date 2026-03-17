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
        private readonly ParallelOptions parallelOptions;

        private readonly string runId = Guid.NewGuid().ToString();
        private Stopwatch stopwatch;

        public Orchestrator(ILogger<Orchestrator> logger, DataCompareService dataCompareService, AppConfiguration appConfiguration)
        {
            stopwatch = new Stopwatch();
            this.dataCompareService = dataCompareService;
            this.logger = logger;
            this.appConfiguration = appConfiguration;
            this.parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = this.appConfiguration.CompareSettings.MaxDegreeOfParallelism,
            };
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
                    await this.dataCompareService.SaveRecordCountMismatchAsync(this.runId, entityMapping.SourceEntity, entityMapping.DestinationEntity, sourceCount, destinationCount);
                }
                await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Source.Type, entityMapping.SourceEntity, entityMapping.PrimaryKeyMapping.SourcePrimaryKey, entityMapping.DestinationEntity, entityMapping.FieldMappings);
                //await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Destination.Type, entityMapping.DestinationEntity, entityMapping.PrimaryKeyMapping.DestinationPrimaryKey, entityMapping.SourceEntity, entityMapping.FieldMappings);
            }

            this.stopwatch.Stop();
            var timeTaken = $"{this.stopwatch.Elapsed.Hours:00}:{this.stopwatch.Elapsed.Minutes:00}:{this.stopwatch.Elapsed.Seconds:00}:{this.stopwatch.Elapsed.Milliseconds / 10:00}";
            this.logger.LogInformation($"Total time taken: {timeTaken}");
        }

        private async Task GetDataToCompareAsync(string type, string sourceEntity, string sourcePrimaryKey, string destinationEntity, FieldMapping[] fieldMappings)
        {
            this.logger.LogInformation($"Fetching data for type: {type} and entity: {sourceEntity} : Started");
            int pageNumber = 1;
            IEnumerable<JObject> rows;

            do
            {
                this.logger.LogInformation($"PageNumber : {pageNumber}");
                rows = await this.dataCompareService.GetDataAsync(type, sourceEntity, pageNumber, this.appConfiguration.CompareSettings.PageSize);
                await Parallel.ForEachAsync(rows, this.parallelOptions, async (sourceRow, token) =>
                {
                    try
                    {
                        this.logger.LogInformation("*");
                        await this.dataCompareService.SaveRowIdAsync(this.runId, type, sourceEntity, sourceRow[sourcePrimaryKey].ToString());

                        var destinationType = type;
                        if (type == DataSourceTypes.Source)
                        {
                            destinationType = DataSourceTypes.Destination;
                        }
                        else
                        {
                            destinationType = DataSourceTypes.Source;
                        }

                        var exists = await this.dataCompareService.RecordExistsAsync(destinationType, destinationEntity, sourceRow[sourcePrimaryKey].ToString());
                        if (exists)
                        {
                            var destinationRow = await this.dataCompareService.GetDataAsync(destinationType, destinationEntity, sourceRow[sourcePrimaryKey].ToString());
                            await Parallel.ForEachAsync(fieldMappings, this.parallelOptions, async (fieldMapping, _) =>
                            {
                                var fieldCompareResult = this.dataCompareService.CompareValues(sourceRow, fieldMapping, destinationRow);
                                if (!fieldCompareResult.Same)
                                {
                                    this.logger.LogDebug("Field Mismatchs");
                                    await this.dataCompareService.SaveEntityFieldMismatchAsync(this.runId, sourceEntity, destinationEntity, sourceRow[sourcePrimaryKey].ToString(), fieldMapping.SourceField, fieldMapping.DestinationField, fieldCompareResult.SourceValue, fieldCompareResult.DestinationValue);
                                }
                            });
                        }
                        else
                        {
                            this.logger.LogWarning("Mising record");
                            await this.dataCompareService.SaveEntityRecordMismatchAsync(runId, sourceRow[sourcePrimaryKey].ToString(), sourceEntity, type);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogDebug(sourceRow.ToString());
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
