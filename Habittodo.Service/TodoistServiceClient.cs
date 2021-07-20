using Habittodo.Model.Todoist.Responses;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Habittodo.Service
{
    public class TodoistServiceClient
    {
        private string apiKey { get; set; }
        private string baseUrl => "https://api.todoist.com/sync/v8/";

        public TodoistServiceClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<SyncResponse> GetItems()
        {
            var response = await GetItemChanges("*");
            return response;
        }

        public async Task<SyncResponse> GetItemChanges(string syncToken)
        {
            using (var client = CreateWebClient())
            {
                var body = InitializeRequestBody();
                body["sync_token"] = syncToken;
                body["resource_types"] = "[\"items\"]";

                var json = await client.UploadStringTaskAsync($"{baseUrl}sync", RequestBodyToString(body));
                var response = JsonConvert.DeserializeObject<SyncResponse>(json);
                return response;
            }
        }

        private string RequestBodyToString(Dictionary<string, string> body)
        {
            var bodyStr = "";
            foreach (var pair in body)
                bodyStr += $"{pair.Key}={pair.Value}&";

            return bodyStr;
        }

        private Dictionary<string, string> InitializeRequestBody()
        {
            var body = new Dictionary<string, string>();
            body["token"] = apiKey;

            return body;
        }

        private WebClient CreateWebClient()
        {
            var client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

            return client;
        }
    }
}
