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
            await habiticaService.Add(items.WhereAdded());
            await habiticaService.Update(items.WhereUpdated());
            await habiticaService.Complete(items.WhereCompleted());
            await habiticaService.Delete(items.WhereDeleted());

            // commit read changes
            await todoistService.CommitRead();

            // return success
            return new OkResult();
        }
    }
}
