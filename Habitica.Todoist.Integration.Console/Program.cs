using Habitica.Todoist.Integration.Model.Habitica;
using Habitica.Todoist.Integration.Model.Todoist;
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

            SyncResponse response = null;
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                var token = "*";
                var body = $"token={todoistApiKey}&sync_token={token}&resource_types=[\"items\"]";
                response = JsonConvert.DeserializeObject<SyncResponse>(client.UploadString($"{todoistApiUrl}sync", body));
            }


            foreach (var item in response.Items)
            {
                var newTask = new Task
                {
                    Text = item.GetCleanContent(),
                    Type = TaskType.Todo,
                    Date = item.Due?.ToJavaScriptDateStr(),
                    Priority = GetHabiticaDifficulty(item.GetDifficulty().GetValueOrDefault())
                };

                using (var client = new WebClient())
                {
                    client.Headers["Content-Type"] = "application/json";
                    client.Headers["x-api-user"] = habiticaUserId;
                    client.Headers["x-api-key"] = habiticaApiKey;
                    client.Headers["x-client"] = "console-test";

                    var result = client.UploadString($"{habiticaApiUrl}tasks/user", "POST", JsonConvert.SerializeObject(newTask, Formatting.Indented));

                    System.Console.WriteLine(result);
                    System.Console.ReadKey();
                }
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
