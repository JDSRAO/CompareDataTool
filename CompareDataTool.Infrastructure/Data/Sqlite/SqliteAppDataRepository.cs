using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using CompareDataTool.Infrastructure.Data.Sqlite.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CompareDataTool.Infrastructure.Data.Sqlite
{
    public class SqliteAppDataRepository : IAppDataRepository
    {
        private readonly AppConfiguration appConfiguration;
        private readonly SqLiteManager sqLiteManager;

        private readonly string fileBasePath = Path.Combine(Directory.GetCurrentDirectory(), "localData");

        public SqliteAppDataRepository(AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            var dbFilePath = $"{fileBasePath}\\{this.appConfiguration.CompareSettings.AppDataFile}";
            this.sqLiteManager = new SqLiteManager($"Data Source={dbFilePath}");
            if (!Directory.Exists(fileBasePath))
            {
                Directory.CreateDirectory(fileBasePath);
            }

            if (!File.Exists(dbFilePath))
            {
                this.CreateTableSchemaAsync().GetAwaiter().GetResult();
            }
        }

        public async Task<IEnumerable<JObject>> GetDataForProcessingAsync(string runId, string type, string entity, int pageNumber, int pageSize)
        {
            var query = new StringBuilder();
            query.AppendLine($"SELECT {nameof(AppData.Data)} FROM {nameof(AppData)}");
            query.AppendLine($"ORDER BY {nameof(AppData.Id)}");
            query.AppendLine($"LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
            query.AppendLine($"");
            query.AppendLine($"");
            query.AppendLine("");

            var results = await this.sqLiteManager.QueryAsync<object>(query.ToString());
            return JsonConvert.DeserializeObject<IEnumerable<JObject>>(JsonConvert.SerializeObject(results));
        }

        public async Task SaveRowForProcessingAsync(string runId, string type, string entity, JObject data)
        {
            var query = new StringBuilder();
            query.AppendLine($"INSERT INTO {nameof(AppData)} ({nameof(AppData.RunId)}, {nameof(AppData.Type)}, {nameof(AppData.Entity)}, {nameof(AppData.Data)}, {nameof(AppData.CreatedOn)})");
            query.AppendLine("VALUES (@runId, @type, @entity, @data, @createdOn)");

            var inputs = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("runId", runId),
                new KeyValuePair<string, object>("type", type),
                new KeyValuePair<string, object>("entity", entity),
                new KeyValuePair<string, object>("data", data.ToString()),
                new KeyValuePair<string, object>("createdOn", DateTime.UtcNow.ToString("O")),
            };

            await this.sqLiteManager.ExecuteAsync(query.ToString(), inputs.ToArray());
        }

        private async Task CreateTableSchemaAsync()
        {
            var query = new StringBuilder();
            query.AppendLine($"CREATE TABLE {nameof(AppData)}");
            query.AppendLine("(");
            query.AppendLine($"{nameof(AppData.Id)} INTEGER,");
            query.AppendLine($"{nameof(AppData.RunId)} TEXT,");
            query.AppendLine($"{nameof(AppData.Type)} TEXT,");
            query.AppendLine($"{nameof(AppData.Entity)} TEXT,");
            query.AppendLine($"{nameof(AppData.Data)} TEXT,");
            query.AppendLine($"{nameof(AppData.CreatedOn)} TEXT,");
            query.AppendLine($"PRIMARY KEY(\"{nameof(AppData.Id)}\" AUTOINCREMENT)");
            query.AppendLine(")");
            query.AppendLine($"");
            query.AppendLine("");

            await this.sqLiteManager.CreateTableIfNotExists(query.ToString());
        }
    }
}
