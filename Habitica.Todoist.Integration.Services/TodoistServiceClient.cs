using Habitica.Todoist.Integration.Model.Todoist;
using Habitica.Todoist.Integration.Model.Todoist.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Habitica.Todoist.Integration.Services
{
    public class TodoistServiceClient
    {
        private string apiKey { get; set; }
        private string baseUrl => "https://api.todoist.com/sync/v8/";

        public TodoistServiceClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public async Task<SyncResponse> GetItemChanges(string syncToken = null)
        {
            if (string.IsNullOrEmpty(syncToken))
                syncToken = "*";

            using (var client = CreateWebClient())
            {
                var body = InitializeRequestBody();
                body["sync_token"] = syncToken;
                body["resource_types"] = "[\"items\"]";

                var json = await client.UploadStringTaskAsync($"{baseUrl}sync", RequestBodyToString(body));
                return JsonConvert.DeserializeObject<SyncResponse>(json);
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
