using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Habitica.Todoist.Integration.Data;
using Habitica.Todoist.Integration.Model.Habitica.Enums;
using Habitica.Todoist.Integration.Model.Storage;
using Habitica.Todoist.Integration.Model.Todoist;
using Habitica.Todoist.Integration.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HabiticaTask = Habitica.Todoist.Integration.Model.Habitica.Task;

namespace Habitica.Todoist.Integration.Function.Sync
{
    public static class HttpSyncFunction
    {
        public static Configuration HttpConfiguration { get; set; } = new Configuration();

        [Singleton("SyncLock", SingletonScope.Host)]
        [FunctionName("HttpSyncFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            // initialize integration services
            var todoistService = new TodoistIntegrationService(HttpConfiguration.TodoistApiKey, 
                HttpConfiguration.TableStorageConnectionString, 
                HttpConfiguration.GiosUserId);
            var habiticaService = new HabiticaIntegrationService(HttpConfiguration.HabiticaUserId,
                HttpConfiguration.HabiticaApiKey,
                HttpConfiguration.TableStorageConnectionString,
                HttpConfiguration.GiosUserId);

            // get all changed items from todoist
            var items = await todoistService.ReadItemChanges();

            // perform actions
            await habiticaService.AddTasks(items.WhereAdded());
            await habiticaService.UpdateTasks(items.WhereUpdated());
            await habiticaService.CompleteTasks(items.WhereCompleted());
            await habiticaService.DeleteTasks(items.WhereDeleted());

            // commit read changes
            await todoistService.CommitRead();

            // return success
            return new OkResult();
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
