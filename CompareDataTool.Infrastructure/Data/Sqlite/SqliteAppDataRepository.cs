using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using Newtonsoft.Json;
using System.Text;

namespace CompareDataTool.Infrastructure.Data.Sqlite
{
    public class SqliteAppDataRepository : IAppDataRepository
    {
        private readonly AppConfiguration appConfiguration;
        private readonly SqLiteManager sqLiteManager;

        private readonly string fileBasePath = Path.Combine(Directory.GetCurrentDirectory());

        public SqliteAppDataRepository(AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            var dbFilePath = Path.Combine(fileBasePath, this.appConfiguration.CompareSettings.AppDataFile);
            this.sqLiteManager = new SqLiteManager($"Data Source={dbFilePath}");
            if (!File.Exists(dbFilePath))
            {
                this.CreateTableSchemaAsync().GetAwaiter().GetResult();
            }
        }

        public Task<IEnumerable<EntityCountMismatch>> GetCountMismatchesAsync(string runId, int pageNumber, int pageSize)
        {
            var query = new StringBuilder();
            query.AppendLine($"SELECT * FROM EntityCountMismatch");
            query.AppendLine($"ORDER BY Id");
            query.AppendLine($"LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");

            return this.sqLiteManager.QueryAsync<EntityCountMismatch>(query.ToString());
        }

        public Task<IEnumerable<EntityFieldMismatch>> GetEntityFieldMismatchAsync(string runId, int pageNumber, int pageSize)
        {
            var query = new StringBuilder();
            query.AppendLine($"SELECT * FROM EntityFieldMismatch");
            query.AppendLine($"ORDER BY Id");
            query.AppendLine($"LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");

            return this.sqLiteManager.QueryAsync<EntityFieldMismatch>(query.ToString());
        }

        public Task<IEnumerable<EntityRecordMismatch>> GetEntityRecordMismatchAsync(string runId, int pageNumber, int pageSize)
        {
            var query = new StringBuilder();
            query.AppendLine($"SELECT * FROM EntityRecordMismatch");
            query.AppendLine($"ORDER BY Id");
            query.AppendLine($"LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");

            return this.sqLiteManager.QueryAsync<EntityRecordMismatch>(query.ToString());
        }

        public async Task<IEnumerable<string>> GetRowIdAsync(string runId, string type, string entity, int pageNumber, int pageSize)
        {
            var query = new StringBuilder();
            query.AppendLine($"SELECT RowId FROM EntityData");
            query.AppendLine($"ORDER BY Id");
            query.AppendLine($"LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}");
            query.AppendLine($"");
            query.AppendLine($"");
            query.AppendLine("");

            var results = await this.sqLiteManager.QueryAsync<object>(query.ToString());
            return JsonConvert.DeserializeObject<IEnumerable<string>>(JsonConvert.SerializeObject(results));
        }

        public async Task InsertEntityCountMismatchAsync(string runId, string sourceEntity, string destinationEntity, int sourceCount, int destinationCount)
        {
            var query = new StringBuilder();
            query.AppendLine($"INSERT INTO EntityCountMismatch (RunId,SourceEntity,DestinationEntity,SourceCount,DestinationCount, CreatedOn)");
            query.AppendLine("VALUES (@runId, @sourceEntity, @destinationEntity, @sourceCount, @destinationCount, @createdOn)");

            var inputs = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("runId", runId),
                new KeyValuePair<string, object>("sourceEntity", sourceEntity),
                new KeyValuePair<string, object>("destinationEntity", destinationEntity),
                new KeyValuePair<string, object>("sourceCount", sourceCount),
                new KeyValuePair<string, object>("destinationCount", destinationCount),
                new KeyValuePair<string, object>("createdOn", DateTime.UtcNow.ToString("O")),
            };

            await this.sqLiteManager.ExecuteAsync(query.ToString(), inputs.ToArray());
        }

        public async Task InsertEntityFieldMismatchAsync(string runId, string sourceEntity, string destinationEntity, string rowId, string sourceField, string destinationField, string sourceValue, string destinationValue)
        {
            var query = new StringBuilder();
            query.AppendLine($"INSERT INTO EntityFieldMismatch (RunId,SourceEntity,DestinationEntity,RowId,SourceField,DestinationField,SourceValue,DestinationValue, CreatedOn)");
            query.AppendLine("VALUES (@runId, @sourceEntity, @destinationEntity, @rowId, @sourceField, @destinationField, @sourceValue, @destinationValue, @createdOn)");

            var inputs = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("runId", runId),
                new KeyValuePair<string, object>("sourceEntity", sourceEntity),
                new KeyValuePair<string, object>("destinationEntity", destinationEntity),
                new KeyValuePair<string, object>("rowId", rowId),
                new KeyValuePair<string, object>("sourceField", sourceField),
                new KeyValuePair<string, object>("destinationField", destinationField),
                new KeyValuePair<string, object>("sourceValue", sourceValue),
                new KeyValuePair<string, object>("destinationValue", destinationValue),
                new KeyValuePair<string, object>("createdOn", DateTime.UtcNow.ToString("O")),
            };

            await this.sqLiteManager.ExecuteAsync(query.ToString(), inputs.ToArray());
        }

        public async Task InsertEntityRecordMismatchAsync(string runId, string entity, string rowId, bool existsInSource, bool existsInDestination)
        {
            var query = new StringBuilder();
            query.AppendLine($"INSERT INTO EntityRecordMismatch (RunId,Entity,RowId,ExistsInSource,ExistsInDestination, CreatedOn)");
            query.AppendLine("VALUES (@runId, @entity, @rowId, @existsInSource, @existsInDestination, @createdOn)");

            var inputs = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("runId", runId),
                new KeyValuePair<string, object>("entity", entity),
                new KeyValuePair<string, object>("rowId", rowId),
                new KeyValuePair<string, object>("existsInSource", existsInSource ? 1 : 0),
                new KeyValuePair<string, object>("existsInDestination", existsInDestination ? 1 : 0),
                new KeyValuePair<string, object>("createdOn", DateTime.UtcNow.ToString("O")),
            };

            await this.sqLiteManager.ExecuteAsync(query.ToString(), inputs.ToArray());
        }

        public async Task SaveRowIdAsync(string runId, string type, string entity, string rowId)
        {
            var query = new StringBuilder();
            query.AppendLine($"INSERT INTO EntityData  (RunId,DataSourceType,Entity,RowId,CreatedOn)");
            query.AppendLine("VALUES (@runId, @type, @entity, @rowId, @createdOn)");

            var inputs = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("runId", runId),
                new KeyValuePair<string, object>("type", type),
                new KeyValuePair<string, object>("entity", entity),
                new KeyValuePair<string, object>("rowId", rowId),
                new KeyValuePair<string, object>("createdOn", DateTime.UtcNow.ToString("O")),
            };

            await this.sqLiteManager.ExecuteAsync(query.ToString(), inputs.ToArray());
        }

        private async Task CreateTableSchemaAsync()
        {
            var schemaFilePath = Path.Combine(fileBasePath, "Data", "Sqlite", "AppDataSchema.txt");
            var dbSchema = await File.ReadAllTextAsync(schemaFilePath);
            //var query = new StringBuilder();
            //query.AppendLine($"CREATE TABLE {nameof(AppData)}");
            //query.AppendLine("(");
            //query.AppendLine($"{nameof(AppData.Id)} INTEGER,");
            //query.AppendLine($"{nameof(AppData.RunId)} TEXT,");
            //query.AppendLine($"{nameof(AppData.Type)} TEXT,");
            //query.AppendLine($"{nameof(AppData.Entity)} TEXT,");
            //query.AppendLine($"{nameof(AppData.RowData)} TEXT,");
            //query.AppendLine($"{nameof(AppData.CreatedOn)} TEXT,");
            //query.AppendLine($"PRIMARY KEY(\"{nameof(AppData.Id)}\" AUTOINCREMENT)");
            //query.AppendLine(")");
            //query.AppendLine($"");
            //query.AppendLine("");

            await this.sqLiteManager.CreateTableIfNotExists(dbSchema);
        }
    }
}
