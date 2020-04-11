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
        public static async Task Run([TimerTrigger("0 */30 * * * *")]TimerInfo myTimer, ILogger log)
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
            await habiticaService.Add(items.WhereAdded());
            await habiticaService.Update(items.WhereUpdated());
            await habiticaService.Complete(items.WhereCompleted());
            await habiticaService.Delete(items.WhereDeleted());

            // commit read changes
            await todoistService.CommitRead();
        }
    }
}
