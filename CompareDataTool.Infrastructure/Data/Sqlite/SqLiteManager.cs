using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using System.Data;

namespace CompareDataTool.Infrastructure.Data.Sqlite
{
    internal class SqLiteManager
    {
        private readonly string connectionString;

        public SqLiteManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task CreateTableIfNotExists(string tableSchema)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                var commandDefin = new CommandDefinition(tableSchema);
                await connection.ExecuteAsync(commandDefin);
            }
        }

        public async Task TruncateUpdateTable(string tableName)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                var commandDefin = new CommandDefinition($"TRUNCATE TABLE {tableName}");
                await connection.ExecuteAsync(commandDefin);
            }
        }

        public async Task InsertDataAsync<T>(T data) where T : class
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.InsertAsync(data);
            }
        }

        public async Task UpdateAsync<T>(T data) where T : class
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.UpdateAsync(data);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                return await connection.QueryAsync<T>(sql);
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<T>(sql);
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public Task<IEnumerable<T>> QueryAsync<T>(string storedProcedureName, object parameters)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                return connection.QueryAsync<T>(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<bool> ExistsAsync<T>(string sql)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                var data = await connection.QueryAsync<T>(sql);
                return data.Any();
            }
        }

        public async Task<int> ExecuteAsync(string sql)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                return await connection.ExecuteAsync(sql);
            }
        }
    }
}
