using Habitica.Todoist.Integration.Model.Habitica;
using Habitica.Todoist.Integration.Model.Habitica.Enums;
using Habitica.Todoist.Integration.Model.Storage;
using Habitica.Todoist.Integration.Model.Todoist;
using Habitica.Todoist.Integration.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Habitica.Todoist.Integration.Console
{
    class Program
    {
        static IConfiguration configuration { get; set; }

        private static string habiticaApiUrl => "https://habitica.com/api/v3/";
        private static string habiticaUserId => configuration["habitica:userId"];
        private static string habiticaApiKey => configuration["habitica:apiKey"];
        private static string todoistApiUrl => "https://api.todoist.com/sync/v8/";
        private static string todoistApiKey => configuration["todoist:apiKey"];
        private static string tableStorageConnectionString => configuration["tableStorage:connectionString"];
        private static string giosUserId => "0b6ec4eb-8878-4b9e-8585-7673764a6541";

        static void Main(string[] args)
        {
            ConfigBuild();

            //var habiticaClient2 = new HabiticaServiceClient(habiticaUserId, habiticaApiKey);

            //var tasks = habiticaClient2.ReadTasks().ConfigureAwait(false).GetAwaiter().GetResult().Data;
            //foreach (var task in tasks)
            //    habiticaClient2.DeleteTask(task.Id).ConfigureAwait(false).GetAwaiter().GetResult();

            //return;

            // initialize all the clients 
            var habiticaClient = new HabiticaServiceClient(habiticaUserId, habiticaApiKey);
            var todoistClient = new TodoistServiceClient(todoistApiKey);
            var tableStorageClient = new TableStorageClient(tableStorageConnectionString);

            // get todoist sync token if available
            var syncToken = "";
            try { syncToken = tableStorageClient.Query<TodoistSync>().Where(x => x.PartitionKey == giosUserId).ToList()
                        .OrderByDescending(x => x.Timestamp).First().RowKey; } catch { }

            // get all changed items from todoist
            var response = todoistClient.GetChangedItems(syncToken).ConfigureAwait(false).GetAwaiter().GetResult();
            var changedItems = response.Items;

            // filter out items by actions
            var addItems = changedItems.Where(x => !tableStorageClient.Exists<TodoHabitLink>(giosUserId, x.Id)
                .ConfigureAwait(false).GetAwaiter().GetResult() && x.Is_deleted == 0).ToList();
            var updateItems = changedItems.Where(x => tableStorageClient.Exists<TodoHabitLink>(giosUserId, x.Id)
                .ConfigureAwait(false).GetAwaiter().GetResult() && x.Is_deleted == 0 && x.Date_completed == null).ToList();
            var completeItems = changedItems.Where(x => x.Is_deleted == 0 && x.Date_completed != null).ToList();
            var deleteItems = changedItems.Where(x => tableStorageClient.Exists<TodoHabitLink>(giosUserId, x.Id)
                .ConfigureAwait(false).GetAwaiter().GetResult() && x.Is_deleted == 1).ToList();

            foreach (var addItem in addItems)
            {
                var task = habiticaClient.CreateTask(TaskFromTodoistItem(addItem)).ConfigureAwait(false).GetAwaiter().GetResult().Data;
                var link = new TodoHabitLink(giosUserId, addItem.Id, task.Id);

                tableStorageClient.InsertOrUpdate(link).ConfigureAwait(false).GetAwaiter().GetResult();
                tableStorageClient.InsertOrUpdate(link.Reverse()).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            foreach (var updateItem in updateItems)
            {
                var habiticaId = tableStorageClient.Query<TodoHabitLink>().Where(x => x.PartitionKey == giosUserId && x.RowKey == updateItem.Id)
                        .ToList().First().HabiticaId;
                habiticaClient.UpdateTask(TaskFromTodoistItem(updateItem, habiticaId)).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            foreach (var completeItem in completeItems)
            {
                var habiticaId = tableStorageClient.Query<TodoHabitLink>().Where(x => x.PartitionKey == giosUserId && x.RowKey == completeItem.Id)
                        .ToList().First().HabiticaId;
                habiticaClient.ScoreTask(habiticaId, ScoreAction.Up).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            foreach (var deleteItem in deleteItems)
            {
                var habiticaId = tableStorageClient.Query<TodoHabitLink>().Where(x => x.PartitionKey == giosUserId && x.RowKey == deleteItem.Id)
                        .ToList().First().HabiticaId;
                habiticaClient.DeleteTask(habiticaId).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            // store sync token
            var todoistSync = new TodoistSync(giosUserId, response.Sync_token);
            tableStorageClient.InsertOrUpdate(todoistSync).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static string GetHabiticaDifficulty(int todoistDifficulty)
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

        public static Task TaskFromTodoistItem(Item item, string id = null)
        {
            var taskTypeStr = Enum.GetName(typeof(TaskType), TaskType.Todo).ToLower();
            var task = new Task
            {
                Id = id,
                Text = item.Content,
                Type = taskTypeStr,
                Date = item.Due?.ToJavaScriptDateStr(),
                Priority = GetHabiticaDifficulty(item.Priority)
            };

            return task;
        }

        static void ConfigBuild()
        {
            new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("secrets.json");

            configuration = new ConfigurationBuilder()
                    .AddJsonFile("secrets.json", true, true)
                    .Build();
        }
    }
}
