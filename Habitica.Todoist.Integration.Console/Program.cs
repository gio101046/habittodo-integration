using Habitica.Todoist.Integration.Model.Habitica;
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

        static void Main(string[] args)
        {
            ConfigBuild();

            var todoistClient = new TodoistClientService(todoistApiKey);
            var syncResponse  = todoistClient.GetUpdatedItems().ConfigureAwait(false).GetAwaiter().GetResult();
               
            foreach (var item in syncResponse.Items)
            {
                var newTask = new Task
                {
                    Text = item.GetCleanContent(),
                    Type = TaskType.Todo,
                    Date = item.Due?.ToJavaScriptDateStr(),
                    Priority = GetHabiticaDifficulty(item.GetDifficulty().GetValueOrDefault())
                };

                var habiticaClient = new HabiticaClientService(habiticaUserId, habiticaApiKey);
                var task = habiticaClient.CreateUserTask(newTask).ConfigureAwait(false).GetAwaiter().GetResult();
            }
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
