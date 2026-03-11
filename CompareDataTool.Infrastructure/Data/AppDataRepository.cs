using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Text;

namespace CompareDataTool.Infrastructure.Data
{
    public class AppDataRepository : IAppDataRepository
    {
        private readonly string fileBasePath = Path.Combine(Directory.GetCurrentDirectory(), "files");
        private readonly AppConfiguration appConfiguration;
        private readonly ParallelOptions parallelOptions;

        public AppDataRepository(AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = this.appConfiguration.CompareSettings.MaxDegreeOfParallelism };
            if (!Directory.Exists(fileBasePath))
            {
                Directory.CreateDirectory(fileBasePath);
            }
        }

        public async Task<IEnumerable<JObject>> GetDataForProcessingAsync(string runId, string type, string entity, int pageNumber, int pageSize)
        {
            var primaryColumn = string.Empty;
            if (type == DataSourceTypes.Source)
            {
                primaryColumn = this.appConfiguration.EntityMappings.First(x => x.SourceEntity == entity).PrimaryKeyMapping.SourcePrimaryKey;
            }
            else
            {
                primaryColumn = this.appConfiguration.EntityMappings.First(x => x.DestinationEntity == entity).PrimaryKeyMapping.DestinationPrimaryKey;
            }

            var filePath = Path.Combine(fileBasePath, runId, type, entity);
            var filePaths = Directory.GetFiles(filePath);
            var currentBatch = filePaths.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray();
            var data = new ConcurrentBag<JObject>();
            await Parallel.ForEachAsync(currentBatch, this.parallelOptions, async (filePath, token) => 
            {
                var content = await File.ReadAllTextAsync(filePath);
                data.Add(JObject.Parse(content));
            });

            return data.AsEnumerable();
        }

        public async Task SaveRowForProcessingAsync(string runId, string type, string entity, JObject data)
        {
            var primaryColumn = string.Empty;
            if (type == DataSourceTypes.Source)
            {
                primaryColumn = this.appConfiguration.EntityMappings.First(x => x.SourceEntity == entity).PrimaryKeyMapping.SourcePrimaryKey;
            }
            else
            {
                primaryColumn = this.appConfiguration.EntityMappings.First(x => x.DestinationEntity == entity).PrimaryKeyMapping.DestinationPrimaryKey;
            }

            var rowId = data[primaryColumn].ToString();
            var filePath = Path.Combine(fileBasePath, runId, type, entity);
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            filePath = Path.Combine(filePath, $"{rowId}.json");

            await File.WriteAllTextAsync(filePath, data.ToString(), Encoding.UTF8);
        }
    }
}
