using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using CompareDataTool.Infrastructure.DataSources.SQL;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CompareDataTool.Infrastructure.DataSources.Dataverse
{
    public class DataverseDataSource : IDataSourceRepository
    {
        private readonly AppConfiguration appConfiguration;
        private readonly SqlManager sqlManager;

        public DataverseDataSource(AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.sqlManager = new SqlManager(appConfiguration.Source.EnvironmentVariables["ConnectionString"]);
        }

        public async Task<IEnumerable<JObject>> GetDataAsync(string entity, int pageNumber, int pageSize)
        {
            var fields = this.appConfiguration.EntityMappings.First(x => x.SourceEntity == entity).FieldMappings.Select(x => x.SourceField);
            var query = new StringBuilder();
            query.AppendLine($"SELECT {string.Join(",", fields)} FROM {entity}");

            var results = await this.sqlManager.QueryAsync<JObject>(query.ToString());
            return results;
        }
    }
}
