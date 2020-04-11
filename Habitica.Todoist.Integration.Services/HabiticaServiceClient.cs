using HabiticaTask = Habitica.Todoist.Integration.Model.Habitica.Task;
using ChecklistItem = Habitica.Todoist.Integration.Model.Habitica.ChecklistItem;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Habitica.Todoist.Integration.Model.Habitica.Responses;
using Habitica.Todoist.Integration.Model.Habitica.Enums;
using System;

namespace Habitica.Todoist.Integration.Services
{
    public class HabiticaServiceClient
    {
        private string userId { get; set; }
        private string apiKey { get; set; }

        private string baseUrl => "https://habitica.com/api/v3/";

        public HabiticaServiceClient(string userId, string apiKey)
        {
            this.userId = userId;
            this.apiKey = apiKey;
        }

        public async Task<HabiticaReponse<HabiticaTask>> CreateTask(HabiticaTask task)
        {
            using (var client = CreateWebClient())
            {
                var request = JsonConvert.SerializeObject(task);
                var json = await client.UploadStringTaskAsync($"{baseUrl}/tasks/user", "POST", request);

                return JsonConvert.DeserializeObject<HabiticaReponse<HabiticaTask>>(json);
            }
        }

        public async Task<HabiticaTask> UpdateTask(HabiticaTask task)
        {
            using (var client = CreateWebClient())
            {
                var request = JsonConvert.SerializeObject(task);
                var json = await client.UploadStringTaskAsync($"{baseUrl}/tasks/{task.Id}", "PUT", request);

                return JsonConvert.DeserializeObject<HabiticaTask>(json);
            }
        }

        public async Task DeleteTask(string taskId)
        {
            using (var client = CreateWebClient())
                await client.UploadStringTaskAsync($"{baseUrl}/tasks/{taskId}", "DELETE", "");
        }

        public async Task ScoreTask(string taskId, ScoreAction action)
        {
            var actionStr = Enum.GetName(action.GetType(), action).ToLower();
            using (var client = CreateWebClient())
                await client.UploadStringTaskAsync($"{baseUrl}/tasks/{taskId}/score/{actionStr}", "POST", "");
        }

        //public async Task<HabiticaReponse<List<HabiticaTask>>> ReadTasks(TaskType taskType = TaskType.Todo)
        //{
        //    var taskTypeStr = Enum.GetName(taskType.GetType(), taskType).ToLower();
        //    using (var client = CreateWebClient())
        //    {
        //        var json = await client.DownloadStringTaskAsync($"{baseUrl}/tasks/user");
        //        var response = JsonConvert.DeserializeObject<HabiticaReponse<List<HabiticaTask>>>(json);
        //        response.Data.RemoveAll(x => x.Type != taskTypeStr);

        //        return response;
        //    }
        //}        //public async Task<HabiticaReponse<List<HabiticaTask>>> ReadTasks(TaskType taskType = TaskType.Todo)

        public async Task<HabiticaReponse<HabiticaTask>> CreateChecklistItem(ChecklistItem checklistItem, string taskId)
        {
            using (var client = CreateWebClient())
            {
                var request = JsonConvert.SerializeObject(checklistItem);
                var json = await client.UploadStringTaskAsync($"{baseUrl}/tasks/{taskId}/checklist", "POST", request);

                return JsonConvert.DeserializeObject<HabiticaReponse<HabiticaTask>>(json);
            }
        }

        public async Task<HabiticaReponse<HabiticaTask>> UpdateChecklistItem(ChecklistItem checklistItem, string taskId)
        {
            using (var client = CreateWebClient())
            {
                var request = JsonConvert.SerializeObject(checklistItem);
                var json = await client.UploadStringTaskAsync($"{baseUrl}/tasks/{taskId}/checklist/{checklistItem.Id}", "PUT", request);

                return JsonConvert.DeserializeObject<HabiticaReponse<HabiticaTask>>(json);
            }
        }

        public async Task DeleteChecklistItem(string taskId, string checklistItemId)
        {
            using (var client = CreateWebClient())
                await client.UploadStringTaskAsync($"{baseUrl}/tasks/{taskId}/checklist/{checklistItemId}", "DELETE", "");
        }

        public async Task ScoreChecklistItem(string taskId, string checklistItemId)
        {
            using (var client = CreateWebClient())
                await client.UploadStringTaskAsync($"{baseUrl}/tasks/{taskId}/checklist/{checklistItemId}/score", "POST", "");
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
