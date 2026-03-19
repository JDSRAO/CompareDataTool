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

        public async Task<int> GetCountAsync(string entity)
        {
            var query = new StringBuilder();
            query.Append($"SELECT COUNT(1) AS [RecordCount] FROM {entity}");
            var results = await this.sqlManager.QueryAsync<object>(query.ToString());
            var rowsCount = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(results));
            return Convert.ToInt32(rowsCount.ElementAt(0)["RecordCount"]);
        }

        public async Task<IEnumerable<JObject>> GetDataAsync(string entity, int pageNumber, int pageSize)
        {
            var entityMapping = this.appConfiguration.EntityMappings.First(x => x.SourceEntity == entity);
            var primaryColumn = entityMapping.PrimaryKeyMapping.SourcePrimaryKey;
            var fields = entityMapping.FieldMappings.Select(x => x.SourceField);
            var query = new StringBuilder();
            query.AppendLine($"SELECT {string.Join(",", fields)}, {primaryColumn}");
            query.AppendLine($"FROM {entity}");
            if (entityMapping.Filters != null && !string.IsNullOrEmpty(entityMapping.Filters.SourceFilter))
            {
                query.AppendLine($"WHERE {entityMapping.Filters.SourceFilter}");
            }
            query.AppendLine($"ORDER BY {primaryColumn}");
            query.AppendLine($"OFFSET {(pageNumber - 1) * pageSize} ROWS");
            query.AppendLine($"FETCH NEXT {pageSize} ROWS ONLY");
            query.AppendLine($"");

            var results = await this.sqlManager.QueryAsync<object>(query.ToString());
            return JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(results));
        }

        public async Task<JObject> GetDataAsync(string entity, string rowId)
        {
            var entityMapping = this.appConfiguration.EntityMappings.First(x => x.SourceEntity == entity);
            var primaryColumn = entityMapping.PrimaryKeyMapping.SourcePrimaryKey;
            var fields = entityMapping.FieldMappings.Select(x => x.SourceField);
            var query = new StringBuilder();
            query.Append($"SELECT {string.Join(",", fields)}, {primaryColumn}");
            query.AppendLine($"FROM {entity}");
            query.AppendLine($"WHERE {primaryColumn} = '{rowId}'");
            var results = await this.sqlManager.QueryAsync<object>(query.ToString());
            var rows = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(results));
            if (rows != null && rows.Any())
            {
                return rows.ElementAt(0);
            }
            else
            {
                return new JObject();
            }
        }

        public async Task<bool> RecordExistsAsync(string entity, string rowId)
        {
            var entityMapping = this.appConfiguration.EntityMappings.First(x => x.SourceEntity == entity);
            var primaryColumn = entityMapping.PrimaryKeyMapping.SourcePrimaryKey;
            var query = new StringBuilder();
            query.Append($"SELECT COUNT(1) AS [RecordCount] FROM {entity} WHERE {primaryColumn} = '{rowId}'");
            var results = await this.sqlManager.QueryAsync<object>(query.ToString());
            var rowsCount = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(results));
            return Convert.ToInt32(rowsCount.ElementAt(0)["RecordCount"]) > 0;
        }
    }
}
