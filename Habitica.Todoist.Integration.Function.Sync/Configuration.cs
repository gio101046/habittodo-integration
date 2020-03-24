using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Habitica.Todoist.Integration.Function.Sync
{
    public class Configuration
    {
        private IConfiguration configuration { get; set; }
        public string HabiticaUserId => configuration["habiticaUserId"];
        public string HabiticaApiKey => configuration["habiticaApiKey"];
        public string TodoistApiKey => configuration["todoistApiKey"];
        public string TableStorageConnectionString => configuration["tableStorageConnectionString"];
        public string GiosUserId => "0b6ec4eb-8878-4b9e-8585-7673764a6541";

        public Configuration()
        {
            BuildConfig();
        }

        private void BuildConfig()
        {
            configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
