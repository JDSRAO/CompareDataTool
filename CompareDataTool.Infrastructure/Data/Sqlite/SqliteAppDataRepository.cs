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

        private const string fileName = "ComareData.db";
        private readonly string fileBasePath = Path.Combine(Directory.GetCurrentDirectory(), "localData");

        public SqliteAppDataRepository(AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.sqLiteManager = new SqLiteManager($"Data Source={fileBasePath}\\{fileName}");
            if (!Directory.Exists(fileBasePath))
            {
                Directory.CreateDirectory(fileBasePath);
                this.CreateTableSchemaAsync().GetAwaiter().GetResult();
            }
        }

        public Task<IEnumerable<JObject>> GetDataForProcessingAsync(string runId, string type, string entity, int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task SaveRowForProcessingAsync(string runId, string type, string entity, JObject data)
        {
            var query = new StringBuilder();
            query.AppendLine($"INSERT INTO {nameof(AppData)} ({nameof(AppData.RunId)}, {nameof(AppData.Type)}, {nameof(AppData.Entity)}, {nameof(AppData.Data)})");
            query.AppendLine("VALUES (");
            query.AppendLine($"'{runId}',");
            query.AppendLine($"'{type}',");
            query.AppendLine($"'{entity}',");
            query.AppendLine($"'{JsonConvert.SerializeObject(data)}'");
            query.AppendLine($"");
            query.AppendLine(");");
            query.AppendLine("");
            query.AppendLine("");
            query.AppendLine("");

            await this.sqLiteManager.ExecuteAsync(query.ToString());
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
            query.AppendLine($"PRIMARY KEY(\"{nameof(AppData.Id)}\" AUTOINCREMENT)");
            query.AppendLine(")");
            query.AppendLine($"");
            query.AppendLine("");

            await this.sqLiteManager.CreateTableIfNotExists(query.ToString());
        }
    }
}
