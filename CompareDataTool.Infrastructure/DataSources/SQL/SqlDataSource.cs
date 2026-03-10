using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using Newtonsoft.Json.Linq;

namespace CompareDataTool.Infrastructure.DataSources.SQL
{
    public class SqlDataSource : IDataSourceRepository
    {
        private readonly AppConfiguration appConfiguration;
        private readonly SqlManager sqlManager;

        public SqlDataSource(AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.sqlManager = new SqlManager(appConfiguration.Destination.EnvironmentVariables["ConnectionString"]);
        }

        public Task<IEnumerable<JObject>> GetDataAsync(string entity, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
