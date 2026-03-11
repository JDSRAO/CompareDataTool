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
                }
                await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Source.Type, entityMapping.SourceEntity);
                await this.GetDataToCompareAsync(this.appConfiguration.EnvironmentSettings.Destination.Type, entityMapping.DestinationEntity);
            }
        }

        private async Task GetDataToCompareAsync(string type, string entity)
        {
            this.logger.LogInformation($"Fetching data for type: {type} and entity: {entity} : Started");
            int pageNumber = 1;
            IEnumerable<JObject> rows;
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = this.appConfiguration.CompareSettings.MaxDegreeOfParallelism,
            };

            do
            {
                this.logger.LogInformation($"PageNumber : {pageNumber}");
                rows = await this.dataCompareService.GetDataAsync(type, entity, pageNumber, this.appConfiguration.CompareSettings.PageSize);
                await Parallel.ForEachAsync(rows, parallelOptions, async (row, token) =>
                {
                    try
                    {
                        this.logger.LogInformation("*");
                        await this.dataCompareService.SaveDataForProcessingAsync(RunId, type, entity, row);
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

            this.logger.LogInformation($"Fetching data for type: {type} and entity: {entity} : Completed");
            this.logger.LogInformation($"");
        }

        private async Task CompareDataAsync(IEnumerable<JObject> source, IEnumerable<JObject> destination)
        {

        }
    }
}
