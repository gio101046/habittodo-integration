using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Habitica.Todoist.Integration.Model.Habitica.Enums;
using Habitica.Todoist.Integration.Model.Storage;
using Habitica.Todoist.Integration.Model.Todoist;
using Habitica.Todoist.Integration.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HabiticaTask = Habitica.Todoist.Integration.Model.Habitica.Task;

namespace Habitica.Todoist.Integration.Function.Sync
{
    public static class SyncFunction
    {
        private static IConfiguration configuration { get; set; }
        private static string habiticaUserId => configuration["habiticaUserId"];
        private static string habiticaApiKey => configuration["habiticaApiKey"];
        private static string todoistApiKey => configuration["todoistApiKey"];
        private static string tableStorageConnectionString => configuration["tableStorageConnectionString"];
        private static string giosUserId => "0b6ec4eb-8878-4b9e-8585-7673764a6541";

        [Singleton]
        [FunctionName("SyncFunction")]
        public static async Task Run([TimerTrigger("0 */2 * * * *")]TimerInfo myTimer, ILogger log)
        {
            BuildConfig();

            // initialize all the clients 
            var habiticaClient = new HabiticaServiceClient(habiticaUserId, habiticaApiKey);
            var todoistClient = new TodoistServiceClient(todoistApiKey);
            var tableStorageClient = new TableStorageClient(tableStorageConnectionString);

            // get todoist sync token if available
            var syncToken = "";
            try
            {
                syncToken = tableStorageClient.Query<TodoistSync>()
                    .Where(x => x.PartitionKey == giosUserId)
                    .ToList()
                    .OrderByDescending(x => x.Timestamp)
                    .First().RowKey;
            }
            catch { }

            // get all changed items from todoist
            var response = await todoistClient.GetChangedItems(syncToken);
            var changedItems = response.Items;

            // filter out items by actions
            var addItems = changedItems
                .Where(x => !tableStorageClient
                    .Exists<TodoHabitLink>(giosUserId, x.Id) && x.Is_deleted == 0)
                .ToList();

            var updateItems = changedItems
                .Where(x => tableStorageClient
                    .Exists<TodoHabitLink>(giosUserId, x.Id) && x.Is_deleted == 0 && x.Date_completed == null)
                .ToList();

            var completeItems = changedItems
                .Where(x => x.Is_deleted == 0 && x.Date_completed != null)
                .ToList();

            var deleteItems = changedItems
                .Where(x => tableStorageClient
                    .Exists<TodoHabitLink>(giosUserId, x.Id) && x.Is_deleted == 1)
                .ToList();

            // perform actions
            foreach (var addItem in addItems)
            {
                var task = (await habiticaClient.CreateTask(TaskFromTodoistItem(addItem))).Data;
                var link = new TodoHabitLink(giosUserId, addItem.Id, task.Id);

                await tableStorageClient.InsertOrUpdate(link);
                await tableStorageClient.InsertOrUpdate(link.Reverse());
            }

            foreach (var updateItem in updateItems)
            {
                var habiticaId = tableStorageClient.Query<TodoHabitLink>()
                    .Where(x => x.PartitionKey == giosUserId && x.RowKey == updateItem.Id)
                    .ToList().First().HabiticaId;
                await habiticaClient.UpdateTask(TaskFromTodoistItem(updateItem, habiticaId));
            }

            foreach (var completeItem in completeItems)
            {
                var habiticaId = tableStorageClient.Query<TodoHabitLink>()
                    .Where(x => x.PartitionKey == giosUserId && x.RowKey == completeItem.Id)
                    .ToList().First().HabiticaId;
                await habiticaClient.ScoreTask(habiticaId, ScoreAction.Up);
            }

            foreach (var deleteItem in deleteItems)
            {
                var habiticaId = tableStorageClient.Query<TodoHabitLink>()
                    .Where(x => x.PartitionKey == giosUserId && x.RowKey == deleteItem.Id)
                    .ToList().First().HabiticaId;
                await habiticaClient.DeleteTask(habiticaId);
            }

            // store new todoist sync token
            var todoistSync = new TodoistSync(giosUserId, response.Sync_token);
            await tableStorageClient.InsertOrUpdate(todoistSync);
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

        private static void BuildConfig()
        {
            configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
