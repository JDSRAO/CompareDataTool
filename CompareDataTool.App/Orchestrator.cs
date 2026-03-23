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
        private readonly ParallelOptions fieldCompareParallelOptions;
        private readonly ReportingService reportingService;
        private readonly DataSourceService dataSourceService;
        private readonly AppDataService appDataService;

        private readonly string runId = Guid.NewGuid().ToString();
        private Stopwatch stopwatch;

        public Orchestrator(ILogger<Orchestrator> logger, DataCompareService dataCompareService, AppConfiguration appConfiguration, ReportingService reportingService, DataSourceService dataSourceService, AppDataService appDataService)
        {
            stopwatch = new Stopwatch();
            this.dataCompareService = dataCompareService;
            this.logger = logger;
            this.appConfiguration = appConfiguration;
            this.reportingService = reportingService;
            this.dataSourceService = dataSourceService;
            this.appDataService = appDataService;
            this.parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = this.appConfiguration.CompareSettings.MaxDegreeOfParallelism,
            };
            this.fieldCompareParallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10,
            };
        }

        public async Task RunAsync()
        {
            this.stopwatch.Start();
            foreach (var entityMapping in this.appConfiguration.EntityMappings)
            {
                var sourceCount = await this.dataSourceService.GetSourceCountAsync(entityMapping.SourceEntity);
                var destinationCount = await this.dataSourceService.GetDestinationCountAsync(entityMapping.DestinationEntity);
                if (sourceCount != destinationCount)
                {
                    this.logger.LogWarning("Count mismatch");
                    await this.appDataService.SaveRecordCountMismatchAsync(this.runId, entityMapping.SourceEntity, entityMapping.DestinationEntity, sourceCount, destinationCount);
                }
                await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Source.Type, entityMapping.SourceEntity, entityMapping.PrimaryKeyMapping.SourcePrimaryKey, entityMapping.DestinationEntity, entityMapping.FieldMappings);
                //await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Destination.Type, entityMapping.DestinationEntity, entityMapping.PrimaryKeyMapping.DestinationPrimaryKey, entityMapping.SourceEntity, entityMapping.FieldMappings);
            }

            this.logger.LogInformation("Generating data reconciliation report : Start");
            var reportPath = await this.reportingService.GenerateReportAsync(this.runId);
            this.logger.LogInformation("Generating data reconciliation report : Completed");
            this.logger.LogInformation($"Reports generated at {reportPath}");

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
                rows = await this.dataSourceService.GetSourceDataAsync(sourceEntity, pageNumber, this.appConfiguration.CompareSettings.PageSize);
                await Parallel.ForEachAsync(rows, this.parallelOptions, async (sourceRow, token) =>
                {
                    try
                    {
                        this.logger.LogInformation("*");
                        if (this.appConfiguration.CompareSettings.SnapshotRowId)
                        {
                            await this.appDataService.SaveRowIdAsync(this.runId, type, sourceEntity, sourceRow[sourcePrimaryKey].ToString());
                        }

                        var destinationType = type;
                        if (type == DataSourceTypes.Source)
                        {
                            destinationType = DataSourceTypes.Destination;
                        }
                        else
                        {
                            destinationType = DataSourceTypes.Source;
                        }

                        //var exists = await this.dataCompareService.RecordExistsAsync(destinationType, destinationEntity, sourceRow[sourcePrimaryKey].ToString());
                        var (exists, destinationRow) = await this.dataSourceService.GetDestinationDataAsync(destinationEntity, sourceRow[sourcePrimaryKey].ToString());
                        if (exists)
                        {
                            await Parallel.ForEachAsync(fieldMappings, this.fieldCompareParallelOptions, async (fieldMapping, _) =>
                            {
                                var fieldCompareResult = this.dataCompareService.CompareValues(sourceRow, fieldMapping, destinationRow);
                                if (!fieldCompareResult.Equal)
                                {
                                    this.logger.LogDebug("Field Mismatch");
                                    await this.appDataService.SaveEntityFieldMismatchAsync(this.runId, sourceEntity, destinationEntity, sourceRow[sourcePrimaryKey].ToString(), fieldMapping.SourceField, fieldMapping.DestinationField, fieldCompareResult.SourceValue, fieldCompareResult.DestinationValue);
                                }
                            });
                        }
                        else
                        {
                            this.logger.LogWarning("Mising record");
                            await this.appDataService.SaveEntityRecordMismatchAsync(runId, sourceRow[sourcePrimaryKey].ToString(), sourceEntity, type);
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
