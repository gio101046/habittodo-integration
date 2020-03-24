using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Habitica.Todoist.Integration.Model.Todoist;
using HabiticaTask = Habitica.Todoist.Integration.Model.Habitica.Task;
using Habitica.Todoist.Integration.Model.Habitica.Enums;

namespace Habitica.Todoist.Integration.Function.Sync
{
    public static class HttpSyncFunction
    {
        public static Configuration HttpConfiguration { get; set; } = new Configuration();

        [FunctionName("HttpSyncFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            return new OkObjectResult("TEST");
        }

        private static string GetHabiticaDifficulty(int todoistDifficulty)
        {
            switch (todoistDifficulty)
            {
                case 1:
                    return "0.1";
                case 2:
                    return "1";
                case 3:
                    return "1.5";
                case 4:
                    return "2";
            }
            return null;
        }

        private static HabiticaTask TaskFromTodoistItem(Item item, string id = null)
        {
            var taskTypeStr = Enum.GetName(typeof(TaskType), TaskType.Todo).ToLower();
            var task = new HabiticaTask
            {
                Id = id,
                Text = item.Content,
                Type = taskTypeStr,
                Date = item.Due?.ToJavaScriptDateStr(),
                Priority = GetHabiticaDifficulty(item.Priority)
            };

            return task;
        }
    }
}
