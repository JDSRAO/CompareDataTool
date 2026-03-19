using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CompareDataTool.Infrastructure.DataSources.SQL
{
    public class SqlDataSource : IDataSourceRepository
    {
        private readonly AppConfiguration appConfiguration;
        private readonly SqlManager sqlManager;

        public SqlDataSource(AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.sqlManager = new SqlManager(appConfiguration.EnvironmentSettings.Destination.EnvironmentVariables["ConnectionString"]);
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
            var entityMapping = this.appConfiguration.EntityMappings.First(x => x.DestinationEntity == entity);
            var primaryColumn = entityMapping.PrimaryKeyMapping.DestinationPrimaryKey;
            var fields = entityMapping.FieldMappings.Select(x => x.DestinationField);
            var query = new StringBuilder();
            query.AppendLine($"SELECT {string.Join(",", fields)}");
            query.AppendLine($"FROM {entity}");
            if (entityMapping.Filters != null && !string.IsNullOrEmpty(entityMapping.Filters.DestinationFilter))
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
            var entityMapping = this.appConfiguration.EntityMappings.First(x => x.DestinationEntity == entity);
            var primaryColumn = entityMapping.PrimaryKeyMapping.DestinationPrimaryKey;
            var fields = entityMapping.FieldMappings.Select(x => x.DestinationField);
            var query = new StringBuilder();
            query.AppendLine($"SELECT {string.Join(",", fields)}");
            query.AppendLine($"FROM {entity}");
            query.AppendLine($"WHERE {primaryColumn} = '{rowId}'");
            var results = await this.sqlManager.QueryAsync<object>(query.ToString());
            var rows = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(results));
            return rows.ElementAt(0);
        }

        public async Task<bool> RecordExistsAsync(string entity, string rowId)
        {
            var entityMapping = this.appConfiguration.EntityMappings.First(x => x.DestinationEntity == entity);
            var primaryColumn = entityMapping.PrimaryKeyMapping.DestinationPrimaryKey;
            var query = new StringBuilder();
            query.Append($"SELECT COUNT(1) AS [RecordCount] FROM {entity} WHERE {primaryColumn} = '{rowId}'");
            var results = await this.sqlManager.QueryAsync<object>(query.ToString());
            var rowsCount = JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(results));
            return Convert.ToInt32(rowsCount.ElementAt(0)["RecordCount"]) > 0;
        }
    }
}
