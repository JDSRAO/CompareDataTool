using Dapper;
using System.Text;
using Newtonsoft.Json;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CompareDataTool.Infrastructure.DataSources.Dataverse
{
    public class TdsSqlManager
    {
        private static HttpClient client = new HttpClient();

        private readonly string connectionString;
        private readonly string tenantId;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string url;

        private string token = string.Empty;
        private DateTime tokenExpiry;

        public TdsSqlManager(string connectionString, string url, string tenantId, string clientId, string clientSecret)
        {
            this.connectionString = connectionString;
            this.tenantId = tenantId;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.url = url;
        }

        public async Task CreateTableIfNotExists(string tableSchema)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.AccessToken = await this.GetAuthHeaderValueAsync();
                var commandDefin = new CommandDefinition(tableSchema);
                await connection.ExecuteAsync(commandDefin);
            }
        }

        public async Task TruncateUpdateTable(string tableName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.AccessToken = await this.GetAuthHeaderValueAsync();
                var commandDefin = new CommandDefinition($"TRUNCATE TABLE {tableName}");
                await connection.ExecuteAsync(commandDefin);
            }
        }

        public async Task InsertDataAsync<T>(T data) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.InsertAsync(data);
            }
        }

        public async Task UpdateAsync<T>(T data) where T : class
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.UpdateAsync(data);
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.AccessToken = await this.GetAuthHeaderValueAsync();
                return await connection.QueryAsync<T>(sql);
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<T>(sql);
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public Task<IEnumerable<T>> QueryAsync<T>(string storedProcedureName, object parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return connection.QueryAsync<T>(storedProcedureName, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<bool> ExistsAsync<T>(string sql)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var data = await connection.QueryAsync<T>(sql);
                return data.Any();
            }
        }

        public async Task<int> ExecuteAsync(string sql)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                return await connection.ExecuteAsync(sql);
            }
        }

        private async Task<string> GetAuthHeaderValueAsync()
        {
            if (string.IsNullOrEmpty(token) || DateTime.UtcNow >= tokenExpiry.AddMinutes(-1))
            {
                await GenerateTokenAsync().ConfigureAwait(false);
            }

            return token;
        }

        private async Task<string> GenerateTokenAsync()
        {
            var tokenUrl = $"https://login.microsoftonline.com/{this.tenantId}/oauth2/token";

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            var requestContentFormat = "grant_type=client_credentials&client_id={0}&client_secret={1}&resource={2}";
            var requestContent = string.Format(requestContentFormat, this.clientId, this.clientSecret, this.url);

            request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.SendAsync(request).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(content);

            token = tokenResponse.AccessToken;
            tokenExpiry = DateTime.UtcNow.AddMinutes(tokenResponse.ExpiresIn / 60 - 30);

            return tokenResponse.AccessToken;
        }

        private class TokenResponse
        {
            /// <summary>
            /// Gets or sets access token.
            /// </summary>
            [JsonProperty("access_token")]
            public string AccessToken { get; set; } = default!;

            /// <summary>
            /// Gets or sets refresh token.
            /// </summary>
            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; } = default!;

            /// <summary>
            /// Gets or sets expires in seconds for access token.
            /// </summary>
            [JsonProperty("expires_in")]
            public long ExpiresIn { get; set; } = default!;

            /// <summary>
            /// Gets or sets error code if there is any.
            /// </summary>
            [JsonProperty("error")]
            public string Error { get; set; } = default!;

            /// <summary>
            /// Gets or sets error description if there is any.
            /// </summary>
            [JsonProperty("error_description")]
            public string ErrorDescription { get; set; } = default!;
        }
    }
}
