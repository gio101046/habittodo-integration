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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HabiticaTask = Habitica.Todoist.Integration.Model.Habitica.Task;

namespace Habitica.Todoist.Integration.Function.Sync
{
    public static class ScheduledSyncFunction
    {
        public static Configuration ScheduledConfiguration { get; set; } = new Configuration();

        [Singleton("SyncLock", SingletonScope.Host)]
        [FunctionName("ScheduledSyncFunction")]
        public static async Task Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            // initialize integration services
            var todoistService = new TodoistIntegrationService(ScheduledConfiguration.TodoistApiKey,
                ScheduledConfiguration.TableStorageConnectionString,
                ScheduledConfiguration.GiosUserId);
            var habiticaService = new HabiticaIntegrationService(ScheduledConfiguration.HabiticaUserId,
                ScheduledConfiguration.HabiticaApiKey,
                ScheduledConfiguration.TableStorageConnectionString,
                ScheduledConfiguration.GiosUserId);

            // get all changed items from todoist
            var items = await todoistService.ReadItemChanges();

            // perform actions
            await habiticaService.AddTasks(items.WhereAdded());
            await habiticaService.UpdateTasks(items.WhereUpdated());
            await habiticaService.CompleteTasks(items.WhereCompleted());
            await habiticaService.DeleteTasks(items.WhereDeleted());

            // commit read changes
            await todoistService.CommitRead();
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
