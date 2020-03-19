using HabiticaTask = Habitica.Todoist.Integration.Model.Habitica.Task;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Habitica.Todoist.Integration.Services
{
    public class HabiticaClientService
    {
        private string userId { get; set; }
        private string apiKey { get; set; }

        private string baseUrl => "https://habitica.com/api/v3/";

        public HabiticaClientService(string userId, string apiKey)
        {
            this.userId = userId;
            this.apiKey = apiKey;
        }

        public async Task<HabiticaTask> CreateUserTask(HabiticaTask task)
        {
            using (var client = CreateWebClient())
            {
                var request = JsonConvert.SerializeObject(task);
                var json = await client.UploadStringTaskAsync($"{baseUrl}/tasks/user", "POST", request);

                return JsonConvert.DeserializeObject<HabiticaTask>(json);
            }
        }

        private WebClient CreateWebClient()
        {
            var client = new WebClient();

            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            client.Headers["x-api-user"] = userId;
            client.Headers["x-api-key"] = apiKey;
            client.Headers["x-client"] = "dotnet-habitica-client";

            return client;
        }
    }
}
