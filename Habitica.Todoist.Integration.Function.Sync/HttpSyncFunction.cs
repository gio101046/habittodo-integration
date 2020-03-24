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
            // initialize all the clients 
            var habiticaClient = new HabiticaServiceClient(HttpConfiguration.HabiticaUserId, HttpConfiguration.HabiticaApiKey);
            var todoistClient = new TodoistServiceClient(HttpConfiguration.TodoistApiKey);
            var storageClient = new TableStorageClient(HttpConfiguration.TableStorageConnectionString);

            // initialize integration services
            var todoistIntegration = new TodoistIntegrationService(todoistClient, storageClient, HttpConfiguration.GiosUserId);
            var giosUserId = HttpConfiguration.GiosUserId;

            // get all changed items from todoist
            var items = await todoistIntegration.ReadItemChanges();

            // perform actions
            foreach (var addedItem in items.WhereAdded())
            {
                var task = (await habiticaClient.CreateTask(TaskFromTodoistItem(addedItem))).Data;
                var link = new TodoHabitLink(giosUserId, addedItem.Id, task.Id);

                await storageClient.InsertOrUpdate(link);
                await storageClient.InsertOrUpdate(link.Reverse());
            }

            foreach (var updatedItem in items.WhereUpdated())
            {
                var habiticaId = storageClient.Query<TodoHabitLink>()
                    .Where(x => x.PartitionKey == giosUserId && x.RowKey == updatedItem.Id)
                    .ToList().First().HabiticaId;
                await habiticaClient.UpdateTask(TaskFromTodoistItem(updatedItem, habiticaId));
            }

            foreach (var completedItem in items.WhereCompleted())
            {
                var habiticaId = storageClient.Query<TodoHabitLink>()
                    .Where(x => x.PartitionKey == giosUserId && x.RowKey == completedItem.Id)
                    .ToList().First().HabiticaId;
                await habiticaClient.ScoreTask(habiticaId, ScoreAction.Up);
            }

            foreach (var deleteItem in items.WhereDeleted())
            {
                var habiticaId = storageClient.Query<TodoHabitLink>()
                    .Where(x => x.PartitionKey == giosUserId && x.RowKey == deleteItem.Id)
                    .ToList().First().HabiticaId;
                await habiticaClient.DeleteTask(habiticaId);
            }

            // commit read changes
            await todoistIntegration.CommitRead();

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
