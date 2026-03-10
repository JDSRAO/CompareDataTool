using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace CompareDataTool.Infrastructure.DataSources.Dataverse
{
    public class DynamicsCrmManager
    {
        private const string ApiVersion = "9.1";

        private static HttpClient client = new HttpClient();

        private string d365url { get; }

        private string tenantId { get; }

        private string clientId { get; }

        private string clientSecret { get; }

        private int pageSize { get; }

        private string apiUrl { get; }

        private string token = string.Empty;

        private DateTime tokenExpiry;

        public DynamicsCrmManager(string url, string tenantId, string clientId, string clientSecret, int pageSize)
        {
            d365url = url;
            this.tenantId = tenantId;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.pageSize = pageSize;
            apiUrl = $"{d365url}/api/data/v{ApiVersion}";
        }

        public async Task Create(string entityLogicalName, dynamic content)
        {
            var request = await BuildRequestMessageAsync(entityLogicalName, HttpMethod.Post, null, new StringContent(JsonConvert.SerializeObject(content)));
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string responseContent = string.Empty;
                if (response.Content != null)
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                }

                throw new Exception($"StatusCode : {response.StatusCode} | ReasonPhrase : {response.ReasonPhrase} | content : {responseContent}");
            }
        }

        public async Task Update(string entityLogicalName, string id, dynamic content)
        {
            var request = await BuildRequestMessageAsync($"{entityLogicalName}({id})", HttpMethod.Patch, null, new StringContent(JsonConvert.SerializeObject(content)));
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string responseContent = string.Empty;
                if (response.Content != null)
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                }

                throw new Exception($"StatusCode : {response.StatusCode} | ReasonPhrase : {response.ReasonPhrase} | content : {responseContent}");
            }
        }

        public async Task<T> ExecuteActionAsync<T>(string actionName, dynamic content)
        {
            var url = $"{apiUrl}/{actionName}";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content)),
            };

            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            request.Headers.Authorization = await GetAuthHeaderValueAsync();

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        public async Task<T> Get<T>(string entityLogicalName, string[]? selectFields = null, string? filter = null)
        {
            var queryString = string.Empty;
            if (selectFields != null && selectFields.Length > 0)
            {
                queryString = "$select=" + string.Join(',', selectFields);
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                if (queryString.Length > 0)
                {
                    queryString += "&$filter=" + filter;
                }
                else
                {
                    queryString = "&$filter=" + filter;
                }
            }

            var request = await BuildRequestMessageAsync(entityLogicalName, HttpMethod.Get, queryString, null);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (content != null)
            {
                var data = JObject.Parse(content);
                return JsonConvert.DeserializeObject<T>(data["value"].ToString());
            }
            else
            {
                throw new Exception("No content");
            }
        }

        public async Task<bool> RecordExistsAsync(string entityName, string recordId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}/{entityName}({recordId})");
            request.Headers.Authorization = await GetAuthHeaderValueAsync();
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        if (Convert.ToString(JObject.Parse(content)["error"]["message"]).Contains("Does Not Exist"))
                        {
                            return false;
                        }
                    }
                }

                throw new Exception($"Error from DynamicsCaller StatusCode : {response.StatusCode} \n ReasonPhrase {response.ReasonPhrase}");
            }
        }

        public Task<T> GetScheduledWorkFlowsAsync<T>(string? filter = null, string[]? select = null)
        {
            var selectFields = new List<string>
            {
                "workflowid",
                "category",
                "clientdata",
                "name",
                "type",
            };

            if (select != null)
            {
                selectFields.AddRange(select);
            }

            var scheduledfilter = new StringBuilder("category eq 5 and contains(clientdata,'Recurrence')");
            if (!string.IsNullOrEmpty(filter))
            {
                scheduledfilter.AppendLine($"and {filter}");
            }

            return Get<T>("workflow", selectFields.ToArray(), scheduledfilter.ToString());
        }

        private async Task<HttpRequestMessage> BuildRequestMessageAsync(
            string entityLogicalName,
            HttpMethod method,
            string? queryString = null,
            StringContent? content = null)
        {
            var url = $"{apiUrl}/{entityLogicalName}";
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                url += "?" + queryString;
            }

            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = await GetAuthHeaderValueAsync();
            request.Headers.Add("OData-MaxVersion", "4.0");
            request.Headers.Add("OData-Version", "4.0");
            request.Headers.Add("Prefer", $"odata.maxpagesize={pageSize}");
            request.Headers.Add("Accept", "application/json;odata.metadata=none");
            if (content != null)
            {
                request.Content = content;
                request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            }

            return request;
        }

        private async Task<AuthenticationHeaderValue> GetAuthHeaderValueAsync()
        {
            if (string.IsNullOrEmpty(token) || DateTime.UtcNow >= tokenExpiry.AddMinutes(-1))
            {
                await GenerateTokenAsync().ConfigureAwait(false);
            }

            return AuthenticationHeaderValue.Parse(token);
        }

        private async Task<string> GenerateTokenAsync()
        {
            var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            var requestContentFormat = "grant_type=client_credentials&client_id={0}&client_secret={1}&resource={2}";
            var requestContent = string.Format(requestContentFormat, clientId, clientSecret, d365url);

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
