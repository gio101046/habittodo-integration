using Habittodo.Data;
using Habittodo.Model.Storage;
using Habittodo.Service;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace Habitica.Todoist.Integration.Console
{
    class Program
    {
        private static IConfiguration configuration { get; set; }

        private static string habiticaUserId => configuration["habitica:userId"];
        private static string habiticaApiKey => configuration["habitica:apiKey"];
        private static string todoistApiKey => configuration["todoist:apiKey"];
        private static string tableStorageConnectionString => configuration["tableStorage:connectionString"];
        private static string giosUserId => "0b6ec4eb-8878-4b9e-8585-7673764a6541";

        static void Main(string[] args)
        {
            ConfigBuild();

            // initialize all the clients 
            var habiticaClient = new HabiticaServiceClient(habiticaUserId, habiticaApiKey);
            var todoistClient = new TodoistServiceClient(todoistApiKey);
            var tableStorageClient = new TableStorageClient(tableStorageConnectionString);

            // get todoist sync token if available
            var syncToken = "";
            try { syncToken = tableStorageClient.Query<TodoistSync>().Where(x => x.PartitionKey == giosUserId).ToList()
                        .OrderByDescending(x => x.Timestamp).First().RowKey; } catch { }

            // get all changed items from todoist
            var response = todoistClient.GetItemChanges(syncToken).ConfigureAwait(false).GetAwaiter().GetResult();
            var changedItems = response.Items;

            /* TESTING */
            
            // store sync token
            var todoistSync = new TodoistSync(giosUserId, response.Sync_token);
            tableStorageClient.InsertOrUpdate(todoistSync).ConfigureAwait(false).GetAwaiter().GetResult();
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
