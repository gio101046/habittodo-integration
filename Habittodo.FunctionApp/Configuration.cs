using Microsoft.Extensions.Configuration;

namespace Habittodo.FunctionApp
{
    public class Configuration
    {
        private IConfiguration configuration { get; set; }
        public string HabiticaUserId => configuration["habiticaUserId"];
        public string HabiticaApiKey => configuration["habiticaApiKey"];
        public string TodoistApiKey => configuration["todoistApiKey"];
        public string TableStorageConnectionString => configuration["tableStorageConnectionString"];
        public string UserId => "0b6ec4eb-8878-4b9e-8585-7673764a6541"; // Currently my userId (Gio) 

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
