using Habittodo.Service;
using Microsoft.Extensions.Configuration;
using System.IO;

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

            // initialize integration services
            var todoistService = new TodoistIntegrationService(todoistApiKey,
                tableStorageConnectionString,
                giosUserId);
            var habiticaService = new HabiticaIntegrationService(habiticaUserId,
                habiticaApiKey,
                tableStorageConnectionString,
                giosUserId);

            // get all changed items from todoist
            var items = todoistService.ReadItemChanges().GetAwaiter().GetResult();

            // perform actions
            habiticaService.Add(items.WhereAdded()).GetAwaiter().GetResult();
            habiticaService.Update(items.WhereUpdated()).GetAwaiter().GetResult();
            habiticaService.Complete(items.WhereCompleted()).GetAwaiter().GetResult();
            habiticaService.Delete(items.WhereDeleted()).GetAwaiter().GetResult();

            // commit read changes
            todoistService.CommitRead().GetAwaiter().GetResult();
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
