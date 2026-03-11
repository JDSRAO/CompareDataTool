using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CompareDataTool.Infrastructure.DataSources.Dataverse
{
    public class DataverseDataSource : IDataSourceRepository
    {
        private readonly AppConfiguration appConfiguration;
        private readonly TdsSqlManager sqlManager;

        public DataverseDataSource(AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.sqlManager = new TdsSqlManager(
                appConfiguration.EnvironmentSettings.Source.EnvironmentVariables["ConnectionString"],
                appConfiguration.EnvironmentSettings.Source.EnvironmentVariables["Url"],
                appConfiguration.EnvironmentSettings.Source.EnvironmentVariables["TenantId"],
                appConfiguration.EnvironmentSettings.Source.EnvironmentVariables["ClientId"],
                appConfiguration.EnvironmentSettings.Source.EnvironmentVariables["ClientSecret"]);
        }

        public Task<int> GetCountAsync(string type, string entity)
        {
            var query = new StringBuilder();
            query.Append($"SELECT COUNT(1) FROM {entity}");
        }

        public async Task<IEnumerable<JObject>> GetDataAsync(string entity, int pageNumber, int pageSize)
        {
            var entityMapping = this.appConfiguration.EntityMappings.First(x => x.SourceEntity == entity);
            var primaryColumn = entityMapping.PrimaryKeyMapping.SourcePrimaryKey;
            var fields = entityMapping.FieldMappings.Select(x => x.SourceField);
            var query = new StringBuilder();
            query.AppendLine($"SELECT {string.Join(",", fields)}");
            query.AppendLine($"FROM {entity}");
            query.AppendLine($"ORDER BY {primaryColumn}");
            query.AppendLine($"OFFSET {(pageNumber - 1) * pageSize} ROWS");
            query.AppendLine($"FETCH NEXT {pageSize} ROWS ONLY");
            query.AppendLine($"");

            var results = await this.sqlManager.QueryAsync<object>(query.ToString());
            return JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(results));
        }
    }
}
