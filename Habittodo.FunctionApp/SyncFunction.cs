using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Habittodo.Service;

namespace Habittodo.FunctionApp
{
    public static class SyncFunction
    {
        private static Configuration SyncConfiguration { get; } = new Configuration();
        
        [Function("SyncFunction")]
        public static async Task Run([TimerTrigger("0 */60 * * * *", RunOnStartup = true)] MyInfo t, FunctionContext c)
        {
            // initialize integration services
            var todoistService = new TodoistIntegrationService(SyncConfiguration.TodoistApiKey,
                SyncConfiguration.TableStorageConnectionString,
                SyncConfiguration.UserId);
            var habiticaService = new HabiticaIntegrationService(SyncConfiguration.HabiticaUserId,
                SyncConfiguration.HabiticaApiKey,
                SyncConfiguration.TableStorageConnectionString,
                SyncConfiguration.UserId);

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

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }
        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }
        public DateTime Next { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}